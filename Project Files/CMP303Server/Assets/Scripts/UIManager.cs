using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //chat
    public TMP_InputField chatField;

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

    public void ChatSendMsg(string msg)
    {
        if (!string.IsNullOrEmpty(msg))
        {
            ChatManager.instance.SendMsg(msg);
            chatField.text = "";
        }
    }
}
