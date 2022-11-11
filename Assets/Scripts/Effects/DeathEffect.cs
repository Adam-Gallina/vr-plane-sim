using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffect : SimpleEffect
{
    protected override void Start()
    {
        system.Play();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == Constants.EnvironmentLayer)
        {
            Invoke("Despawn", despawnTime);
        }
    }
}
