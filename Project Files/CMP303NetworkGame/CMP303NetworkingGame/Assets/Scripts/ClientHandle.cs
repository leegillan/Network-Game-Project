using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet pack)
    {
        //same order as being written into packet to avoid errors
        string msg = pack.ReadString();
        int myID = pack.ReadInt();

        Debug.Log($"Message from Server: {msg}");
        Client.instance.myID = myID;

        ClientSend.WelcomeReceived();

        //Passes local port that tcp connection is using
       //Now that we have the client's id, connect UDP
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);

    }

    public static void SpawnPlayer(Packet pack)
    {
        //same order as being written into packet to avoid errors
        int id = pack.ReadInt();
        string username = pack.ReadString();
        Vector3 pos = pack.ReadVector3();
        Quaternion rot = pack.ReadQuaternion();
        
        GameManager.instance.SpawnPlayer(id, username, pos, rot);
    }

    public static void PlayerPosition(Packet pack)
    {
        int id = pack.ReadInt();
        Vector3 pos = pack.ReadVector3();
        int serverTick = pack.ReadInt();

        GameManager.instance.serverTick = serverTick;

        if (GameManager.players.TryGetValue(id, out PlayerManager _player))
        {
            if (id == Client.instance.myID)
            {
                _player.realServerPos = pos;
                _player.UpdatePosition();
            }
            else
            {
                _player.transform.position = pos;
            }
        }

    }

    public static void PlayerRotation(Packet pack)
    {
        int id = pack.ReadInt();
        Quaternion rot = pack.ReadQuaternion();
        int serverTick = pack.ReadInt();

        GameManager.instance.serverTick = serverTick;

        if (GameManager.players.TryGetValue(id, out PlayerManager _player))
        {
            if (id == Client.instance.myID)
            {
                _player.realServerRot = rot;
                _player.UpdateRotation();
            }
            else
            {
                _player.transform.rotation = rot;
            }
        }
    }

    public static void PlayerDisconnects(Packet pack)
    {
        int id = pack.ReadInt();

        Destroy(GameManager.players[id].gameObject);

        GameManager.players.Remove(id);
    }

    public static void PlayerHealth(Packet pack)
    {
        int id = pack.ReadInt();
        float health = pack.ReadFloat();

        GameManager.players[id].SetHealth(health);
    }

    public static void PlayerRespawned(Packet pack)
    {
        int id = pack.ReadInt();

        GameManager.players[id].Respawn();
    }

    public static void SpawnProjectile(Packet pack)
    {
        int projectileID = pack.ReadInt();
        Vector3 pos = pack.ReadVector3();

        GameManager.instance.SpawnProjectile(projectileID, pos);
    }

    public static void ProjectilePosition(Packet pack)
    {
        int projectileID = pack.ReadInt();
        Vector3 pos = pack.ReadVector3();

        if (GameManager.projectiles.TryGetValue(projectileID,  out ProjectileManager _projectile))
        {
            _projectile.transform.position = pos;
        }
    }

    public static void ProjectlieExploded(Packet pack)
    {
        int projectileID = pack.ReadInt();
        Vector3 pos = pack.ReadVector3();

        GameManager.projectiles[projectileID].Explode(pos);
    }

    public static void Chat(Packet pack)
    {
        int playerID = pack.ReadInt();
        string message = pack.ReadString();

        ChatManager.instance.AddToChat(playerID, message);
    }
}
