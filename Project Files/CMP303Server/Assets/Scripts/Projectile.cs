using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectileID = 1;

    public int id;
    public Rigidbody rigidBody;
    public int thrownBy;
    public Vector3 initialForce;
    public float exlposionRadius = 1.5f;
    public float explosionDamage = 75.0f;

    private void Start()
    {
        id = nextProjectileID;
        nextProjectileID++;

        projectiles.Add(id, this);

        ServerSend.SpawnProjectile(this, thrownBy);

        rigidBody.AddForce(initialForce);
        StartCoroutine(ExplodeAfterTime());
    }

    private void FixedUpdate()
    {
        ServerSend.ProjectilePosition(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    public void Initialise(Vector3 initialMovementDir, float initialForceDir, int thrownByPlayer)
    {
        initialForce = initialMovementDir * initialForceDir;
        thrownBy = thrownByPlayer;
    }

    private void Explode()
    {
        ServerSend.ProjectileExploded(this);

        //Checks if there are any colliders within a radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, exlposionRadius);

        foreach (Collider collider in colliders)
        {
            if(collider.CompareTag("Player"))
            {
                collider.GetComponent<Player>().TakeDamage(explosionDamage);
            }
        }

        projectiles.Remove(id);
        Destroy(gameObject);
    }    

    private IEnumerator ExplodeAfterTime()
    {
        yield return new WaitForSeconds(10.0f);
        
        Explode();
    }
}
