using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;
    public GameObject projectilePrefab;

    public delegate void Notify(int id);
    public event Notify PlayerConnected;
    public event Notify PlayerDisconnected;

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

    private void Start()
    {
        //Limit frames of server to tick rate to stop overhead and bad perfomance
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        //starts server as soon as it is open, takes in max players that can join and the port we want to listen on
        Server.Start(5, 26950);

    }

    private void OnApplicationQuit()
    {
        Server.Stop();        
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, new Vector3(0.0f, 0.5f, 0.0f), Quaternion.identity).GetComponent<Player>();
    }

    public Projectile InstantiateProjectile(Transform shootOrigin)
    {
        return Instantiate(projectilePrefab, shootOrigin.position + shootOrigin.forward, Quaternion.identity).GetComponent<Projectile>();
    }

    public void ConnectedPlayer(int id)
    {
        PlayerConnected?.Invoke(id);
    }

    public void DisconnectPlayer(int id)
    {
        PlayerDisconnected?.Invoke(id);
    }
}
