using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEffect : MonoBehaviour
{
    [SerializeField] protected float despawnTime = 5;
    [SerializeField] protected ParticleSystem system;

    protected virtual void Start()
    {
        system.Play();

        Invoke("Despawn", despawnTime);
    }

    protected void Despawn()
    {
        Destroy(gameObject);
    }
}
