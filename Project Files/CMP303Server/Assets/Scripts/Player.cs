using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    public CharacterController charCont;
    public Transform shootOrigin;
    public float gravity = -9.81f;
    public float moveSpeed = 15.0f;
    public float jumpSpeed = 5.0f;
    public float throwForce = 500.0f;
    public float health;
    public float maxHealth;

    private bool[] inputs;
    private float yVel = 0.0f;

    public int serverTick;

    private void Start()
    {
        gravity *= Time.deltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialise(int _id, string _username)
    {
        id = _id;
        username = _username;

        health = maxHealth;

        inputs = new bool[5];
    }

    public void FixedUpdate()
    {
        if(health <= 0.0f)
        {
            return;
        }

        Vector2 inputDirection = Vector2.zero;

        if (inputs[0])
        {
            inputDirection.y += 1;
        }
        if (inputs[1])
        {
            inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            inputDirection.x += 1;
        }

        serverTick++;

        Move(inputDirection);
    }

    private void Move(Vector2 inputDirection)
    {
        Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;

        moveDirection *= moveSpeed;

        //If player is on the ground reset y velocity
        if(charCont.isGrounded)
        {
            yVel = 0.0f;

            if (inputs[4])
            {
                yVel += jumpSpeed;
            }
        }

        yVel += gravity;
        moveDirection.y = yVel;

        charCont.Move(moveDirection);

        //Send data
        ServerSend.PlayerPosition(this);

        //Send rotation to everyone except who it belongs to so that the server 
        //does not overwrite the player rotation and cause any snapping
        //ServerSend.PlayerRotation(this);
    }

    public void SetInput(bool[] _input, Quaternion _rot)
    {
        inputs = _input;
        transform.rotation = _rot;
    }

    public void Shoot(Vector3 dir)
    {
        if(health <= 0.0f)
        {
            return;
        }

        if(Physics.Raycast(shootOrigin.position, dir, out RaycastHit hit))
        {
            if(hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<Player>().TakeDamage(10.0f);
            }
        }
    }

    public void ThrowItem(Vector3 dir)
    {
        //Chekc if player is alive to stop player throwing anything when respawning
        if(health <= 0)
        {
            return;
        }

        NetworkManager.instance.InstantiateProjectile(shootOrigin).Initialise(dir, throwForce, id);
    }

    public void TakeDamage(float dmg)
    {
        if(health <= 0.0f)
        {
            return;
        }

        health -= dmg;

        if(health <= 0f)
        {
            health = 0.0f;
            charCont.enabled = false;
            transform.position = new Vector3(0.0f, 25.0f, 0.0f);

            ServerSend.PlayerPosition(this);

            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    //Uses coroutine to add delay
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5.0f);

        health = maxHealth;
        charCont.enabled = true;

        ServerSend.PlayerRespawned(this);
    }
}
