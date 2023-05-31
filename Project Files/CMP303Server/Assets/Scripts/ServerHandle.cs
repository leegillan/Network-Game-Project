using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class ServerHandle
{
    public static void WelcomeRecieved(int fromClient, Packet pack)
    {
        int clientIdCheck = pack.ReadInt();
        string username = pack.ReadString();

        Debug.Log($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected to server successfully and is now player {fromClient}");

        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong cliend ID ({clientIdCheck})!");
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

    public static void PlayerShoots(int fromClient, Packet pack)
    {
        Vector3 shootDir = pack.ReadVector3();

        Server.clients[fromClient].player.Shoot(shootDir);
    }

    public static void PlayerThrowItem(int fromClient, Packet pack)
    {
        Vector3 throwDir = pack.ReadVector3();

        Server.clients[fromClient].player.ThrowItem(throwDir);
    }

    public static void ChatMessageRecieved(int fromClient, Packet pack)
    {
        int fromID = pack.ReadInt();
        string msg = pack.ReadString();

        if (fromClient == fromID)
        {
            ChatManager.instance.ReceiveMessage(fromID, msg);
        }
        else
        {
            ChatManager.instance.ReceiveMessage(0, $"Packet alteration detected from: {Server.clients[fromClient].player.username}");
        }
    }
}
