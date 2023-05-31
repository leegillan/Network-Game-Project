using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
    public InputField usernameField;

    //chat
    public GameObject chatMenu;
    public TMP_InputField chatField;

    public bool inChat { get; set; }

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

    //when player clicks buton this is called
    public void ConnectToServer()
    {
        startMenu.SetActive(false);
        usernameField.interactable = false;
        Client.instance.ConnectToServer();

        chatField.interactable = true;
    }

    public void ToggleChatMenu()
    {
        if(chatMenu.activeSelf == true)
        {
            inChat = false;
            chatMenu.SetActive(false);
        }
        else
        {
            inChat = true;
            chatMenu.SetActive(true);
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
