using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public static ChatManager instance;
    public TMP_Text textObj;
    int msgCount = 0;

    public List<Message> messages = new List<Message>()
    {
        {new Message(0, Constants.WELCOME_MSG) },
    };

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

    public void SendMsg(string _content)
    {
        ReceiveMessage(0, _content);
    }

    public void ReceiveMessage(int sender, string msg)
    {
        Message _msg;

        _msg = new Message(sender, msg);
        messages.Add(_msg);
        AddToChat(_msg);
        ServerSend.Chat(_msg);

    }

    public void AddToChat(Message _msg)
    {
        int playerID = _msg.playerID;
        string message = _msg.message;

        textObj.rectTransform.sizeDelta = new Vector2(textObj.rectTransform.sizeDelta.x, textObj.rectTransform.sizeDelta.y + 24);

        if (playerID == 0)
        {
            textObj.text += $"\n<color=#FF0000>SERVER: <color=#FFFFFF>{message}";
        }
        else
        {
            textObj.text += $"\n<color=#FFFF00>{Server.clients[playerID].player.username}: <color=#FFFFFF>{message}";
        }

        msgCount++;
    }
}

public struct Message
{
    public int playerID { get; }
    public string message { get; }

    public Message(int sentBy, string msg)
    {
        playerID = sentBy;
        message = msg;
    }

    public override string ToString()
    {
        string msg = "";

        msg += Server.clients[playerID].player.username + ": ";
        msg += message;

        return msg;
    }
}


