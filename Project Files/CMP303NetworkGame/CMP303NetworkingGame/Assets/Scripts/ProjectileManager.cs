using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public int id;
    public GameObject explosionPrefab;

    public void Initialise(int _id)
    {
        id = _id;
    }

    public void Explode(Vector3 pos)
    {
        transform.position = pos;
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        GameManager.projectiles.Remove(id);
        Destroy(gameObject);
    }
}
