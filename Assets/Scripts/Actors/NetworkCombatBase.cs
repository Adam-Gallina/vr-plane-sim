using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkCombatBase : AvatarBase
{
    [Header("Combat")]
    [SerializeField] private Transform[] bulletSource;
    private int nextSource;
    [SerializeField] private NetworkBullet bulletPrefab;
    [SerializeField] private Transform specialSource;
    [SyncVar]
    [SerializeField] protected PowerupBase currSpecial;
    [SerializeField] private DamageSource damageType = DamageSource.AI;
    public float maxMissileDetectionRange = 20;

    protected void Fire()
    {
        if (!hasAuthority) return;

        CmdSpawnBullet(nextSource);

        if (++nextSource >= bulletSource.Length)
            nextSource = 0;
    }

    protected void FireSpecial()
    {
        if (!hasAuthority) return;

        CmdSpawnSpecial();
    }

    [Command]
    private void CmdSpawnBullet(int source)
    {
        NetworkBullet b = Instantiate(bulletPrefab, bulletSource[source].position, bulletSource[source].rotation);
        NetworkServer.Spawn(b.gameObject);
        b.SetSource(this, damageType);

        RpcOnSpawnBullet(source);
    }

    [ClientRpc]
    private void RpcOnSpawnBullet(int source)
    {
        bulletSource[source].GetComponent<AudioSource>()?.Play();
    }

    [Server]
    public void SetSpecial(PowerupBase powerup)
    {
        currSpecial = powerup;
    }

    [Command]
    private void CmdSpawnSpecial()
    {
        if (currSpecial.UsePowerup(this, specialSource, damageType))
            currSpecial = null;
    }
}
