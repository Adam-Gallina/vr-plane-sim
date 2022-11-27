using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PowerupProjectile : PowerupBase
{
    [SerializeField] private NetworkBullet projectile;

    [Server]
    public override bool UsePowerup(NetworkCombatBase source, Transform spawn, DamageSource sourceType, Constants.Tag targetTag, LayerMask targetLayer, bool destroyOnUse = true)
    {
        SpawnProjectile(source, spawn, sourceType, targetTag, targetLayer);

        if (destroyOnUse)
            NetworkServer.Destroy(gameObject);

        return true;
    }

    [Server]
    private void SpawnProjectile(NetworkCombatBase source, Transform spawn, DamageSource sourceType, Constants.Tag targetTag, LayerMask targetLayer)
    {
        NetworkBullet newP = Instantiate(projectile, spawn.position, spawn.rotation);
        NetworkServer.Spawn(newP.gameObject);

        newP.SetSource(source, sourceType, targetTag, targetLayer);
    }
}
