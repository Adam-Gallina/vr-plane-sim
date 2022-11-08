using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] protected LayerMask targetLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer.value) > 0)
        {
            HealthBase target = other.gameObject.GetComponentInParent<HealthBase>();
            if (target)
                target.Damage(1);

            Destroy(gameObject);
        }
        else if (other.gameObject.layer == Constants.EnvironmentLayer)
        {
            Destroy(gameObject);
        }
    }
}
