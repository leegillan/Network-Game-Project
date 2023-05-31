using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    //Sends a packet to the server via TCP
    private static void SendTCPData(Packet pack)
    {
        pack.WriteLength();

        Client.instance.tcp.SendData(pack);
    }

    //Sends a packet to the server via UDP
    private static void SendUDPData(Packet pack)
    {
        pack.WriteLength();

        Client.instance.udp.SendData(pack);
    }

    public static void WelcomeReceived()
    {
        using (Packet pack = new Packet((int)ClientPackets.welcomeReceived))
        {
            pack.Write(Client.instance.myID);
            pack.Write(UIManager.instance.usernameField.text);

            SendTCPData(pack);
        }
    }

    //Sends player input to the server
    public static void PlayerMovement(bool[] inputs)
    {
        //creates a new packet
        using (Packet pack = new Packet((int)ClientPackets.playerMovement))
        {
            pack.Write(inputs.Length);

            foreach (bool input in inputs)
            {
                pack.Write(input);
            }

            pack.Write(GameManager.players[Client.instance.myID].transform.rotation);

            //Use UDP because movement will be constantly sent to the server and we can afford to lose some packets
            //speed is key for this
            SendUDPData(pack);
        }
    }

    //normal rotation is insufficient as it doesn't account for player's camera vertical rotation
    //no guarantee that the rotation that is set when the player shoots will be the one that server decides to be current when the server recieves the packet
    //due to latency and potential packet loss
    public static void PlayerShoots(Vector3 dir)
    {
        using (Packet pack = new Packet((int)ClientPackets.playerShoot))
        {
            pack.Write(dir);

            SendTCPData(pack);
        }
    }

    public static void PlayerThrowItem(Vector3 dir)
    {
        using (Packet pack = new Packet((int)ClientPackets.playerThrowItem))
        {
            pack.Write(dir);

            SendTCPData(pack);
        }
    }

    //Chat
    public static void Chat(string _msg)
    {
        using (Packet _packet = new Packet((int)ClientPackets.chatMessageRecieved))
        {
            _packet.Write(Client.instance.myID);
            _packet.Write(_msg);

            SendTCPData(_packet);
        }
    }
}
