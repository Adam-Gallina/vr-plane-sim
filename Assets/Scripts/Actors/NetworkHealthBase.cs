using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum DamageSource { Player, Ground, AI }
public class NetworkHealthBase : NetworkCombatBase
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

    //public void HandleHealthChanged(float oldValue, float newValue) => Debug.Log($"{name} {oldValue} -> {newValue}");

    [ServerCallback]
    public virtual void Damage(float damage, NetworkCombatBase source, DamageSource sourceType)
    {
        if (dead)
            return;

        currHealth -= damage;

        if (currHealth <= 0 && !dead)
        {
             Death(source, sourceType);
        }
    }

    [ServerCallback]
    protected virtual void Death(NetworkCombatBase source, DamageSource sourceType)
    {
        if (dead)
            return;

        if (player)
            player.OnAvatarKilled(source, sourceType);

        source.player?.OnEnemyKilled(this, sourceType);

        RpcOnDeath(source, sourceType);

        dead = true;
    }

    [ClientRpc]
    protected virtual void RpcOnDeath(NetworkCombatBase source, DamageSource sourceType)
    {
        if (isServer)
            NetworkServer.Destroy(gameObject);
    }
}
