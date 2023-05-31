using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform camTransform;

    //ServerModel
    public GameObject serverModel;
    bool serverModelToggle { get; set; }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoots(camTransform.forward);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ClientSend.PlayerThrowItem(camTransform.forward);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            UIManager.instance.ToggleChatMenu();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleServerModel();
        }
    }

    private void FixedUpdate()
    {
        if (UIManager.instance.inChat == true)
        {
            return;
        }

        SendInputToServer();
    }

    private void SendInputToServer()
    {
        bool[] inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space),
        };

        ClientSend.PlayerMovement(inputs);
    }

    //Toggles server model
    public void ToggleServerModel()
    {
        if (serverModelToggle)
        {
            serverModel.SetActive(false);
            serverModelToggle = false;
        }
        else
        {
            serverModel.SetActive(true);
            serverModelToggle = true;
        }
    }
}
