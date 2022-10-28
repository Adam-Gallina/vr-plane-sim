using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBase : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected float maxHealth;
    protected float currHealth;

    protected virtual void Awake()
    {
        currHealth = maxHealth;
    }

    public virtual void Damage(float damage)
    {
        currHealth -= damage;

        if (currHealth <= 0)
        {
            Death();
        }
    }

    protected virtual void Death()
    {
        Destroy(gameObject);
    }
}
