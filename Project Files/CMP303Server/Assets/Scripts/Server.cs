using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static int maxPlayers { get; private set; }
    public static int portNum { get; private set; }

    //Track clients in server, storing them as keys
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

        Debug.Log("Starting server...");

        //Initialises server data
        InitialiseServerData();

        //Set up TCP listener
        tcpListener = new TcpListener(IPAddress.Any, portNum);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null); //Asynchronous 

        //Set up UDP listener
        udpListener = new UdpClient(port);
        udpListener.BeginReceive(UDPRecieveCallback, null);

        Debug.Log($"Server started on {portNum}");
    }

    private static void TCPConnectCallback(IAsyncResult res)
    {
        //Stores client returned by TCP listener
        TcpClient client = tcpListener.EndAcceptTcpClient(res);

        //continue listening for new clients
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        Debug.Log($"Incoming connection from {client.Client.RemoteEndPoint}...");
                                           
        //Runs through max amount of players that can join the server
        for (int i = 1; i <= maxPlayers; i++)
        {
            //checks if the clients slot is empty and if so then we can connect the client
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(client);

                //return out of the function so that the client only takes out one open slot
                return;
            }
        }

        //Tell if the server is full
        Debug.Log($"{client.Client.RemoteEndPoint} failed to connect: Server full.");
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

            if (data.Length < 4)
            {
                return;
            }

            //Check to make sure the clients id is not equal to 0 to assure no server crash.
            using (Packet pack = new Packet(data))
            {
                int clientID = pack.ReadInt();

                if (clientID == 0)
                {
                    return;
                }

                //Checks if senders udp end point is null
                //if so then it is a new connection and it is just opening the clients port
                if (clients[clientID].udp.endPoint == null)
                {
                    clients[clientID].udp.Connect(clientEndPoint);
                    return;
                }

                if (clients[clientID].udp.endPoint.ToString() == clientEndPoint.ToString())
                {
                    clients[clientID].udp.HandleData(pack);
                }
            }
        }
        catch (Exception exc)
        {
            Debug.Log($"Error recieving UDP data: {exc}");
        }
    }

    // data over the network // packet sending
    public static void SendUDPData(IPEndPoint clientEndPoint, Packet pack)
    {
        //try/catch block to prevent any server crashes
        try
        {
            //makes sure there is an endpoint to send data to
            if (clientEndPoint != null)
            {
                udpListener.BeginSend(pack.ToArray(), pack.Length(), clientEndPoint, null, null);
            }
        }
        catch (Exception exc)
        {
            Debug.Log($"Error sending data to {clientEndPoint} via UDP: {exc}");
        }
    }

    private static void InitialiseServerData()
    {
        //populate the clients dictionary creating a space for a client to fill
        for (int i = 1; i <= maxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        //Fill dictionary with packets definitions
        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeRecieved },
                {(int)ClientPackets.playerMovement, ServerHandle.PlayerMovement},
                {(int)ClientPackets.playerShoots, ServerHandle.PlayerShoots},
                {(int)ClientPackets.playerThrowItem, ServerHandle.PlayerThrowItem},
                {(int)ClientPackets.chatMessageRecieved, ServerHandle.ChatMessageRecieved },
            };

        Debug.Log("Initialised packets.");
    }

    //Stops listening
    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}
