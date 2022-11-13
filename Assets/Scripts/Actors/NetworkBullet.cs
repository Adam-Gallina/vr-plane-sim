using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkBullet : NetworkBehaviour
{
    protected NetworkCombatBase spawner;
    protected DamageSource source;

    [SerializeField] protected Constants.Tag targetTag;
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected float damage = 1;
    [SerializeField] protected GameObject hitEffect;
    private bool canDamage = true;

    [SerializeField] protected float speed = 250;

    protected Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
    }

    private bool isQuitting = false;
    private void OnApplicationQuit() => isQuitting = true;
    private void OnDestroy()
    {
        if (!isQuitting)
        {
            if (hitEffect)
                Instantiate(hitEffect, transform.position, transform.rotation);
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<NetworkPlaneController>() == spawner)
            return;

        if (((1 << other.gameObject.layer) & targetLayer.value) > 0)
        {
            if (!other.CompareTag(Constants.TagString(targetTag)))
                return;

            NetworkHealthBase target = other.gameObject.GetComponentInParent<NetworkHealthBase>();
            if (target && canDamage)
            {
                target.Damage(damage, spawner, source);
                canDamage = false;
            }

            NetworkServer.Destroy(gameObject);
        }
        else if (other.gameObject.layer == Constants.EnvironmentLayer)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    [Server]
    public void SetSource(NetworkCombatBase spawner, DamageSource sourceType) 
    { 
        this.spawner = spawner;
        source = sourceType;
    } 
}
