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
    //[SerializeField] private float maxBulletAngle;
    [SerializeField] private Transform specialSource;
    [SyncVar]
    [SerializeField] protected PowerupBase currSpecial;
    [SerializeField] private DamageSource damageType = DamageSource.AI;
    public float maxMissileDetectionRange = 20;
    [SerializeField] protected Constants.Tag targetTag;
    [SerializeField] protected LayerMask targetLayer;

    [Server]
    protected void ServerFire()
    {
        SpawnBullet(nextSource);

        if (++nextSource >= bulletSource.Length)
            nextSource = 0;
    }

    protected void Fire()
    {
        if (!hasAuthority) return;

        CmdFire(nextSource);

        if (++nextSource >= bulletSource.Length)
            nextSource = 0;
    }

    protected void FireSpecial(bool removeSpecial = true)
    {
        if (!hasAuthority) return;

        CmdSpawnSpecial(removeSpecial);
    }

    [Command]
    private void CmdFire(int source)
    {
        SpawnBullet(source);
    }

    [Server]
    private void SpawnBullet(int source)
    {
        NetworkBullet b = Instantiate(bulletPrefab, bulletSource[source].position, bulletSource[source].rotation);
        NetworkServer.Spawn(b.gameObject);
        b.SetSource(this, damageType, targetTag, targetLayer);

        RpcOnSpawnBullet(source);
    }

    [ClientRpc]
    private void RpcOnSpawnBullet(int source)
    {
        if (bulletSource[source].GetComponent<AudioSource>())
            bulletSource[source].GetComponent<AudioSource>()?.Play();
    }

    [Server]
    public void SetSpecial(PowerupBase powerup)
    {
        currSpecial = powerup;
    }


    [Command]
    private void CmdSpawnSpecial(bool removeSpecial)
    {
        SpawnSpecial(removeSpecial);
    }


    [Server]
    protected void SpawnSpecial(bool removeSpecial = true)
    {
        if (currSpecial.UsePowerup(this, specialSource, damageType, targetTag, targetLayer, removeSpecial) && removeSpecial)
            currSpecial = null;
    }
}
