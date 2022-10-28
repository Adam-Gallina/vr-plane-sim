using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    [SerializeField] private float despawnTime = 5;
    [SerializeField] private ParticleSystem explosionSystem;

    private void Start()
    {
        explosionSystem.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == Constants.EnvironmentLayer)
        {
            Invoke("Despawn", despawnTime);
        }
    }

    private void Despawn()
    {
        Destroy(gameObject);
    }
}
