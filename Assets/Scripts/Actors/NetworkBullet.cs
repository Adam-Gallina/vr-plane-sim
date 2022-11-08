using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkBullet : NetworkBehaviour
{
    [SerializeField] protected LayerMask targetLayer;

    [SerializeField] private float speed = 250;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer.value) > 0)
        {
            //HealthBase target = other.gameObject.GetComponentInParent<HealthBase>();
            //if (target)
            //    target.Damage(1);

            NetworkServer.Destroy(gameObject);
        }
        else if (other.gameObject.layer == Constants.EnvironmentLayer)
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}
