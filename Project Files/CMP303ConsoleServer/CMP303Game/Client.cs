using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.Numerics;

namespace CMP303Game
{
    class Client
    {
        public static int dataBufferSize = 4096;

        //client data to be stored
        public int id;
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

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                recievedData = new Packet();
                recieveBuffer = new byte[dataBufferSize];

                stream.BeginRead(recieveBuffer, 0, dataBufferSize, RecieveCallback, null);

                // TODO: send welcome packet
                ServerSend.Welcome(id, "Hello Asshat, welcome to the server!");
            }

            public void SendData(Packet pack)
            {
                try
                {
                    if(socket != null)
                    {
                        stream.BeginWrite(pack.ToArray(), 0, pack.Length(), null, null);
                    }
                }
                catch(Exception exc)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {exc}");;
                }
            }

            private void RecieveCallback(IAsyncResult res)
            {
                try
                {
                    int byteLength = stream.EndRead(res);

                    if(byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(recieveBuffer, data, byteLength);

                    recievedData.Reset(HandleData(data));

                    //TODO: handle data
                    stream.BeginRead(recieveBuffer, 0, dataBufferSize, RecieveCallback, null);
                }
                catch(Exception exc)
                {
                    Console.WriteLine($"Error recieving TCP data: {exc}");

                    Server.clients[id].Disconnect();
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                recievedData.SetBytes(data);

                if (recievedData.UnreadLength() >= 4)
                {
                    packetLength = recievedData.ReadInt();

                    if (packetLength <= 0)
                    {
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

            public void Connect (IPEndPoint endP)
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
            player = new Player(id, playerName, new Vector3(0, 0, 0));

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
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

            player = null;

            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}
