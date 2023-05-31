using System;
using System.Collections.Generic;
using System.Text;

namespace CMP303Game
{
    class ServerSend
    {
        private static void SendTCPData(int toClient, Packet pack)
        {
            pack.WriteLength();
            Server.clients[toClient].tcp.SendData(pack);
        }

        private static void SendUDPData(int toClient, Packet pack)
        {
            pack.WriteLength();
            Server.clients[toClient].udp.SendData(pack);
        }


        private static void SendTCPDataToAll(Packet pack)
        {
            pack.WriteLength();

            for (int i = 1; i <= Server.maxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(pack);
            }
        }

        private static void SendTCPDataToAll(int exceptClient, Packet pack)
        {
            pack.WriteLength();

            for (int i = 1; i <= Server.maxPlayers; i++)
            {
                if (i != exceptClient)
                { 
                    Server.clients[i].tcp.SendData(pack);
                }
            }
        }


        private static void SendUDPDataToAll(Packet pack)
        {
            pack.WriteLength();

            for (int i = 1; i <= Server.maxPlayers; i++)
            {
                Server.clients[i].udp.SendData(pack);
            }
        }

        private static void SendUDPDataToAll(int exceptClient, Packet pack)
        {
            pack.WriteLength();

            for (int i = 1; i <= Server.maxPlayers; i++)
            {
                if (i != exceptClient)
                {
                    Server.clients[i].udp.SendData(pack);
                }
            }
        }

        //Welcome message
        public static void Welcome(int toClient, string msg)
        {
            using (Packet pack = new Packet((int)ServerPackets.welcome))
            {
                pack.Write(msg);
                pack.Write(toClient);

                SendTCPData(toClient, pack);
            }
        }

        public static void SpawnPlayer(int toClient, Player _player)
        {
            using (Packet pack = new Packet((int)ServerPackets.spawnPlayer))
            {
                pack.Write(_player.id);
                pack.Write(_player.username);
                pack.Write(_player.position);
                pack.Write(_player.rotation);

                SendTCPData(toClient, pack);
            }
        }

        public static void PlayerPosition(Player _player)
        {
            using (Packet pack = new Packet((int)ServerPackets.playerPosition))
            {
                pack.Write(_player.id);
                pack.Write(_player.position);

                SendUDPDataToAll(pack);
            }
        }

        public static void PlayerRotation(Player _player)
        {
            using (Packet pack = new Packet((int)ServerPackets.playerRotation))
            {
                pack.Write(_player.id);
                pack.Write(_player.rotation);

                SendUDPDataToAll(_player.id, pack);
            }
        }
    }
}
