using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;

public class Client
{
    //4MB of data
    public static int dataBufferSize = 4096;

    //client data to be stored
    public int id;

    //The instances of the callback methods
    public TCP tcp;
    public UDP udp;

    public Player player;

    //creates new client data
    public Client(int clientID)
    {
        id = clientID;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    //Where we store the instance that we get back from the servers connect callback
    public class TCP
    {
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;

        private Packet recievedData;
        private byte[] recieveBuffer;

        public TCP(int _id)
        {
            id = _id;
        }

        //Initialises newly connected clients TCP-related data 
        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();

            recievedData = new Packet();
            recieveBuffer = new byte[dataBufferSize];

            stream.BeginRead(recieveBuffer, 0, dataBufferSize, RecieveCallback, null);

            //send welcome packet
            ServerSend.Welcome(id, "Hello Asshat, welcome to the server!");
        }

        //Sends data to client via TCP
        public void SendData(Packet pack)
        {
            try
            {
                if (socket != null)
                {
                    //Send data to correct client
                    stream.BeginWrite(pack.ToArray(), 0, pack.Length(), null, null);
                }
            }
            catch (Exception exc)
            {
                Debug.Log($"Error sending data to player {id} via TCP: {exc}"); ;
            }
        }

        //Reads incoming data from the stream
        private void RecieveCallback(IAsyncResult res)
        {
            //Try catch block so that any errors wont result in server crashing
            try
            {
                //Returns an int that represents the number of bytes we read from the stream
                int byteLength = stream.EndRead(res);

                //if there is no data then we will disconnect the client as it would mean that the client has stopped communicating
                if (byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                //if the client has received data then we create a new byte array with the correct size
                byte[] data = new byte[byteLength];

                //copy the recieved bytes into new array
                Array.Copy(recieveBuffer, data, byteLength);

                recievedData.Reset(HandleData(data));

                //Continue reading data from stream
                stream.BeginRead(recieveBuffer, 0, dataBufferSize, RecieveCallback, null);
            }
            catch (Exception exc)
            {
                Debug.Log($"Error recieving TCP data: {exc}");

                Server.clients[id].Disconnect();
            }
        }

        //Prepares received data for the correct packet handler
        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            recievedData.SetBytes(data);

            //if clients received data contains a packet
            if (recievedData.UnreadLength() >= 4)
            {
                packetLength = recievedData.ReadInt();

                //if packet contains data it will return out of the function
                if (packetLength <= 0)
                {
                    //resets the receivedData to let it be reused
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= recievedData.UnreadLength())
            {
                byte[] packetBytes = recievedData.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet pack = new Packet(packetBytes))
                    {
                        int packID = pack.ReadInt();
                        Server.packetHandlers[packID](id, pack);
                    }
                });

                packetLength = 0;

                if (recievedData.UnreadLength() >= 4)
                {
                    packetLength = recievedData.ReadInt();

                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        //Disconnects the TCP connection and cleans up
        public void Disconnect()
        {
            socket.Close();
            stream = null;
            recievedData = null;
            recieveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endPoint;

        private int id;

        //initialise ID
        public UDP(int _id)
        {
            id = _id;
        }

        //Initialises newly connected clients UDP data
        public void Connect(IPEndPoint endP)
        {
            endPoint = endP;
        }


        public void SendData(Packet pack)
        {
            Server.SendUDPData(endPoint, pack);
        }

        public void HandleData(Packet packData)
        {
            int packetLength = packData.ReadInt();
            byte[] packBytes = packData.ReadBytes(packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet pack = new Packet(packBytes))
                {
                    int packID = pack.ReadInt();
                    Server.packetHandlers[packID](id, pack);
                }
            });
        }

        public void Disconnect()
        {
            endPoint = null;
        }
    }

    //Send player into game
    public void SendIntoGame(string playerName)
    {
        player = NetworkManager.instance.InstantiatePlayer();
        player.Initialise(id, playerName);

        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != id)
                {
                    ServerSend.SpawnPlayer(id, _client.player);
                }
            }
        }

        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                ServerSend.SpawnPlayer(_client.id, player);
            }
        }
    }

    private void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

        ThreadManager.ExecuteOnMainThread(() =>
        {
            UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        });

        tcp.Disconnect();
        udp.Disconnect();

        ServerSend.PlayerDisconnects(id);
    }
}
