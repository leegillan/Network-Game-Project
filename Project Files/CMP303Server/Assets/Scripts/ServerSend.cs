using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
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
            pack.Write(_player.transform.position);
            pack.Write(_player.transform.rotation);

            SendTCPData(toClient, pack);
        }
    }

    public static void PlayerPosition(Player _player)
    {
        using (Packet pack = new Packet((int)ServerPackets.playerPosition))
        {
            pack.Write(_player.id);
            pack.Write(_player.transform.position);
            pack.Write(_player.serverTick);

            SendUDPDataToAll(pack);
        }
    }

    public static void PlayerRotation(Player _player)
    {
        using (Packet pack = new Packet((int)ServerPackets.playerRotation))
        {
            pack.Write(_player.id);
            pack.Write(_player.transform.rotation);
            pack.Write(_player.serverTick);

            SendUDPDataToAll(_player.id, pack);
        }
    }

    public static void PlayerDisconnects(int _playerID)
    {
        using (Packet pack = new Packet((int)ServerPackets.playerDisconnects))
        {
            pack.Write(_playerID);

            //Use TCP so packet doen't get lost
            SendTCPDataToAll(pack);
        }
    }

    public static void PlayerHealth(Player player)
    {
        using (Packet pack = new Packet((int)ServerPackets.playerHealth))
        {
            pack.Write(player.id);
            pack.Write(player.health);

            //Use TCP so packet doen't get lost
            SendTCPDataToAll(pack);
        }
    }

    public static void PlayerRespawned(Player player)
    {
        using (Packet pack = new Packet((int)ServerPackets.playerRespawned))
        {
            pack.Write(player.id);

            //Use TCP so packet doen't get lost
            SendTCPDataToAll(pack);
        }
    }

    public static void SpawnProjectile(Projectile projectile, int thrownBy)
    {
        using (Packet pack = new Packet((int)ServerPackets.spawnProjectile))
        {
            pack.Write(projectile.id);
            pack.Write(projectile.transform.position);

            //Use TCP so packet doen't get lost
            SendTCPDataToAll(pack);
        }
    }

    public static void ProjectilePosition(Projectile projectile)
    {
        using (Packet pack = new Packet((int)ServerPackets.projectilePosition))
        {
            pack.Write(projectile.id);
            pack.Write(projectile.transform.position);

            //Use TCP so packet doen't get lost
            SendUDPDataToAll(pack);
        }
    }

    public static void ProjectileExploded(Projectile projectile)
    {
        using (Packet pack = new Packet((int)ServerPackets.projectileExploded))
        {
            pack.Write(projectile.id);
            pack.Write(projectile.transform.position);

            //Use TCP so packet doen't get lost
            SendTCPDataToAll(pack);
        }
    }

    //Welcome message
    public static void Chat(Message _msg)
    {
        using (Packet pack = new Packet((int)ServerPackets.chatMessage))
        {
            pack.Write(_msg.playerID);
            pack.Write(_msg.message);

            SendTCPDataToAll(pack);
        }
    }
}
