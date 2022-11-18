using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkCombatBase : AvatarBase
{
    [Header("Combat")]
    [SerializeField] private Transform[] bulletSource;
    private int nextSource;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform specialSource;
    [SerializeField] protected GameObject currSpecial;
    [SerializeField] private DamageSource damageType = DamageSource.AI;

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
        GameObject b = Instantiate(bulletPrefab, bulletSource[source].position, bulletSource[source].rotation);
        NetworkServer.Spawn(b);
        b.GetComponent<NetworkBullet>().SetSource(this, damageType);

        RpcOnSpawnBullet(source);
    }

    [ClientRpc]
    private void RpcOnSpawnBullet(int source)
    {
        bulletSource[source].GetComponent<AudioSource>()?.Play();
    }

    [Command]
    private void CmdSpawnSpecial()
    {
        GameObject b = Instantiate(currSpecial, specialSource.position, specialSource.rotation);
        NetworkServer.Spawn(b);
        b.GetComponent<NetworkBullet>().SetSource(this, damageType);

    }
}
