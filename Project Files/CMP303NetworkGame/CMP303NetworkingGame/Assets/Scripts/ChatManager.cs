using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ChatManager : MonoBehaviour
{
    public static ChatManager instance;

    public TMP_Text textObj;

    int msgCount = 0;

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

    public void SendMsg(string msg)
    {
        ClientSend.Chat(msg);
    }

    public void AddToChat(int playerID, string msg)
    {
        textObj.rectTransform.sizeDelta = new Vector2(textObj.rectTransform.sizeDelta.x, textObj.rectTransform.sizeDelta.y + 24);

        if (playerID == 0)
        {
            textObj.text += $"\n<color=#FF0000>SERVER: <color=#FFFFFF>{msg}";
        }
        else
        {
            textObj.text += $"\n<color=#FFFF00>{GameManager.players[playerID].username}: <color=#FFFFFF>{msg}";
        }

        msgCount++;
    }


}
