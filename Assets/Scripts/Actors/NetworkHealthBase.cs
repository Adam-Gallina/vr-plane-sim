using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkHealthBase : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField] protected float maxHealth;
    [SyncVar]//(hook = nameof(HandleHealthChanged))]
    protected float currHealth;
    private bool dead = false;

    protected virtual void Awake()
    {
        currHealth = maxHealth;
    }

    public void HandleHealthChanged(float oldValue, float newValue) => Debug.Log($"{name} {oldValue} -> {newValue}");

    [ServerCallback]
    public virtual void Damage(float damage)
    {
        if (dead)
            return;

        currHealth -= damage;

        if (currHealth <= 0 && !dead)
        {
             Death();
        }
    }

    [ServerCallback]
    protected virtual void Death()
    {
        RpcOnDeath();
        dead = true;
    }

    [ClientRpc]
    protected virtual void RpcOnDeath()
    {
        if (isServer)
            NetworkServer.Destroy(gameObject);
    }
}