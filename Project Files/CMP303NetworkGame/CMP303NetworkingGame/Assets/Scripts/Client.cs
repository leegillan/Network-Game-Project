using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    //localhost ip address
    public string ip = "127.0.0.1";

    //same as server port for local connection
    public int port = 26950;

    //local clients ID
    public int myID = 0;

    //reference to protocols
    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;

    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packetHandlers;


    //sets up instance
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this);
        }
    }

    private void OnApplicationQuit()
    {
        //Disconnect when the game is closed
        Disconnect();
    }

    //Trys to connect to server
    public void ConnectToServer()
    {
        tcp = new TCP();
        udp = new UDP();

        InitialiseClientData();

        isConnected = true;
        //Connect tcp
        //udp gets connected once tcp is done
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet recievedData;
        private byte[] recieveBuffer;

        //Tries to connect to the server via TCP
        public void Connect()
        {
            socket = new TcpClient { ReceiveBufferSize = dataBufferSize, SendBufferSize = dataBufferSize };

            recieveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        //Initialises the newly connected client's TCP-related data
        private void ConnectCallback(IAsyncResult res) //Asynchronous
        {
            socket.EndConnect(res);

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            recievedData = new Packet();

            //start reading from stream
            stream.BeginRead(recieveBuffer, 0, dataBufferSize, RecieveCallback, null);
        }

        //send data across network uusing TCP
        public void SendData(Packet pack)
        {
            try
            {
                //checks if there is a socket avaialable to write to the server
                if(socket != null)
                {
                    stream.BeginWrite(pack.ToArray(), 0, pack.Length(), null, null);
                }
            }
            catch(Exception exc)
            {
                Debug.Log($"Error sending data to server cia TCP: {exc}");
            }
        }

        //similar to servers recive callback
        //Reads incoming data from stream
        private void RecieveCallback(IAsyncResult res)
        {
            
            try
            {
                int byteLength = stream.EndRead(res);

                if (byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(recieveBuffer, data, byteLength);

                //Takes in boolean returned by handle data method and resets recieved data
                recievedData.Reset(HandleData(data));

                stream.BeginRead(recieveBuffer, 0, dataBufferSize, RecieveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        //Prepares received data to be used by the appropriate packet handler methods
        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            recievedData.SetBytes(data);

          
            if (recievedData.UnreadLength() >= 4)
            {  
                //If client's received data contains a packet
                packetLength = recievedData.ReadInt();

                //if packet contains no data
                if (packetLength <= 0)
                {
                    //Reset receivedData instance to allow it to be reused
                    return true;
                }
            }

            //While packet contains data 
            //AND packet data length doesn't exceed the length of the packet we're reading
            while (packetLength > 0 && packetLength <= recievedData.UnreadLength())
            {
                byte[] packetBytes = recievedData.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet pack = new Packet(packetBytes))
                    {
                        int packID = pack.ReadInt();

                        //Calls appropriate method to handle the packet
                        packetHandlers[packID](pack);
                    }
                });

                //resets packet length
                packetLength = 0;

                // If client's received data contains another packet
                if (recievedData.UnreadLength() >= 4)
                {
                    packetLength = recievedData.ReadInt();

                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if(packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        //Disconnects from the server and cleans up the TCP connection.
        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            recievedData = null;
            recieveBuffer = null;
            socket = null;
        }

    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        //Has different port number from the server - one which the client is communicating from
        //Tries to connect to the server via UDP
        public void Connect(int localPort)
        {
            socket = new UdpClient(localPort);

            socket.Connect(endPoint);
            socket.BeginReceive(RecieveCallback, null);

            //Create new packet and send it immedialtely to establish a connection with server 
            //and open up local ports so that client can receive messages
            using (Packet pack = new Packet())
            {
                SendData(pack);
            }
        }

        //Sends data to the client via UDP.
        public void SendData(Packet pack)
        {
            try
            {
                pack.InsertInt(instance.myID);

                if(socket != null)
                {
                    socket.BeginSend(pack.ToArray(), pack.Length(), null, null);
                }
            }
            catch(Exception exc)
            {
                Debug.Log($"Error sending data to server via UDP: {exc}");
            }
        }

        //Receives incoming UDP data
        private void RecieveCallback(IAsyncResult res)
        {
            try
            {
                byte[] data = socket.EndReceive(res, ref endPoint);
                socket.BeginReceive(RecieveCallback, null);

                if (data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch
            {
                Disconnect();
            }
        }

        //Prepares received data to be used by the appropriate packet handler methods
        private void HandleData(byte[] data)
        {
            using (Packet pack = new Packet(data))
            {
                //Read out packet length and read specified amount of bytes back into data variable
                //Removes the first 4 bytes from array whiich represent the length of the packet
                int packLength = pack.ReadInt();
                data = pack.ReadBytes(packLength);
            }

            //Create new packet with now shortened byte array
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet pack = new Packet(data))
                {
                    //Reads out packet ID 
                    int packID = pack.ReadInt();
                    packetHandlers[packID](pack);
                }
            });
        }

        //Disconnects from the server and cleans up the UDP connection.
        private void Disconnect()
        {
            instance.Disconnect();

            endPoint = null;
            socket = null;
        }
    }

    //Initialises all necessary client data
    private void InitialiseClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
            { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
            { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
            { (int)ServerPackets.playerDisconnects, ClientHandle.PlayerDisconnects },
            { (int)ServerPackets.playerHealth, ClientHandle.PlayerHealth },
            { (int)ServerPackets.playerRespawned, ClientHandle.PlayerRespawned },
            { (int)ServerPackets.spawnProjectile, ClientHandle.SpawnProjectile },
            { (int)ServerPackets.projectilePosition, ClientHandle.ProjectilePosition },
            { (int)ServerPackets.projectileExploded, ClientHandle.ProjectlieExploded },
            { (int)ServerPackets.chatMessage, ClientHandle.Chat },
        };

        Debug.Log("Initialized packets.");
    }

    //Disconnects from the server and stops all network traffic
    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();

            Debug.Log("Disconnected from server.");
        }
    }
}
