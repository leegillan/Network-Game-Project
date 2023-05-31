using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace CMP303Game
{
    class Server
    {
        public static int maxPlayers { get; private set; }
        public static int portNum{ get; private set; }

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        //PacketHandler setup variables
        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        public static TcpListener tcpListener;

        public static UdpClient udpListener;

        public static void Start(int maxP, int port)
        {
            maxPlayers = maxP;
            portNum = port;

            Console.WriteLine("Starting server...");
            InitialiseServerData();

            tcpListener = new TcpListener(IPAddress.Any, portNum);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            udpListener = new UdpClient(port);
            udpListener.BeginReceive(UDPRecieveCallback, null);

            Console.WriteLine($"Server started on {portNum}");
        }

        private static void TCPConnectCallback(IAsyncResult res)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(res);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= maxPlayers; i++)
            {
                if(clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full.");
        }

        //Return any bytes received and set ip endpoint to where the data came from
        private static void UDPRecieveCallback(IAsyncResult res)
        {
            try
            {
                //No specific IP address endpoint or port
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(res, ref clientEndPoint);

                //Call begin recieve so that we dont miss any incoming data
                udpListener.BeginReceive(UDPRecieveCallback, null);

                if(data.Length < 4)
                {
                    return;
                }

                //Check to make sure the clients id is not equal to 0 to assure no server crash.
                using (Packet pack = new Packet(data))
                {
                    int clientID = pack.ReadInt();

                    if(clientID == 0)
                    {
                        return;
                    }

                    //Checks if senders udp end point is null
                    //if so then it is a new connection and it is just opening the clients port
                    if(clients[clientID].udp.endPoint == null)
                    {
                        clients[clientID].udp.Connect(clientEndPoint);
                        return;
                    }

                    if(clients[clientID].udp.endPoint.ToString() == clientEndPoint.ToString())
                    {
                        clients[clientID].udp.HandleData(pack);
                    }
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine($"Error recieving UDP data: {exc}");
            }
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet pack)
        {
            try
            {
                if(clientEndPoint != null)
                {
                    udpListener.BeginSend(pack.ToArray(), pack.Length(), clientEndPoint, null, null);
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine($"Error sending data to {clientEndPoint} via UDP: {exc}");
            }
        }

        private static void InitialiseServerData()
        {
            for (int i = 1; i <= maxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeRecieved },
                {(int)ClientPackets.playerMovement, ServerHandle.PlayerMovement},
            };

            Console.WriteLine("Initialised packets.");
        }
    }
}
