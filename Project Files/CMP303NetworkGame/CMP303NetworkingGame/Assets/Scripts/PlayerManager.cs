using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth;

    public MeshRenderer model;

    //Health
    public GameObject healthUI;
    public Slider healthBar;

    public float GetHealth() { return health; }
    public float GetMaxHealth() { return maxHealth; }

    public Vector3 realServerPos;
    public Quaternion realServerRot;

    public void Initialise(int _id, string _userName)
    {
        id = _id;
        username = _userName;
        health = maxHealth;

        healthUI.SetActive(true);
    }

    public void UpdatePosition()
    {
        if (TryGetComponent<Interpolator>(out Interpolator interp))
        {
            //for 100ms of delay we do 100/16.6ms which would equal 6 ticks
            GameManager.instance.delayTick = GameManager.instance.clientTick - 6;
            interp.NewUpdate(GameManager.instance.clientTick, realServerPos);
            //moves the server model instantly without the interpolation
            transform.Find("serverPositionModel").position = realServerPos;
        }
    }

    public void UpdateRotation()
    {
        if (TryGetComponent<Interpolator>(out Interpolator interp))
        {
            //for 100ms of delay we do 100/16.6ms which would equal 6 ticks
            GameManager.instance.delayTick = GameManager.instance.clientTick - 6;
            interp.NewUpdate(GameManager.instance.clientTick, realServerRot);
            //moves the server model instantly without the interpolation
            transform.Find("serverPositionModel").rotation = realServerRot;
        }
    }

    //let clients determine whhen player has died
    //no need to send a packet when player dies
    //we use packets to send player health when it changes
    public void SetHealth(float _health)
    {
        health = _health;

        SetHealthBar(health);

        if (health <= 0.0f)
        {
            Die();
        }    
    }

    public void SetHealthBar(float health)
    {
        healthBar.value = health;
    }

    public void Die()
    {
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }
}
