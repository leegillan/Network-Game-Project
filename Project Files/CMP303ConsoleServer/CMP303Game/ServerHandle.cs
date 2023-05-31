using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;

namespace CMP303Game
{
    class ServerHandle
    {
        public static void WelcomeRecieved(int fromClient, Packet pack)
        {
            int clientIdCheck = pack.ReadInt();
            string username = pack.ReadString();

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected to server successfully and is now player {fromClient}");

            if(fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong cliend ID ({clientIdCheck})!");
            }

            //TODO: Send player into game

            Server.clients[fromClient].SendIntoGame(username);
        }

        public static void PlayerMovement(int fromClient, Packet pack)
        {
            bool[] inputs = new bool[pack.ReadInt()];

            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = pack.ReadBool();
            }

            Quaternion rot = pack.ReadQuaternion();

            Server.clients[fromClient].player.SetInput(inputs, rot);
        }
    }
}
