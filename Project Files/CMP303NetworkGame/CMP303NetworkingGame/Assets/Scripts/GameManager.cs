using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    //store all player information on client side
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    //store all projectile information on client side
    public static Dictionary<int, ProjectileManager> projectiles = new Dictionary<int, ProjectileManager>();

    public GameObject localPlayerPre;
    public GameObject playerPre;
    public GameObject projectilePre;

    //Interpolation variables
    public int serverTick { get; set; }
    public float secPerTick { get; set; }
    public int delayTick { get; set; }
    public int clientTick { get; set; }

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
        //gets time between ticks as set in unity - 0.33333 so should be around 16.6ms tick per second
        secPerTick = Time.fixedDeltaTime;
    }

    public void FixedUpdate()
    {
        //counts ticks
        clientTick++;
    }

    //spawns player
    public void SpawnPlayer(int id, string username, Vector3 pos, Quaternion rot)
    {
        GameObject player;

        if(id == Client.instance.myID)
        {
            player = Instantiate(localPlayerPre, pos, rot);
        }
        else
        {
            player = Instantiate(playerPre, pos,  rot);
        }

        player.GetComponent<PlayerManager>().Initialise(id, username);

        players.Add(id, player.GetComponent<PlayerManager>());
    }

    //spawns projectile
    public void SpawnProjectile(int _id, Vector3 pos)
    {
        GameObject projectile = Instantiate(projectilePre, pos, Quaternion.identity);

        projectile.GetComponent<ProjectileManager>().Initialise(_id);
        projectiles.Add(_id, projectile.GetComponent<ProjectileManager>());
    }
}
