using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkCombatUpdates : NetworkBehaviour
{
    [SyncVar]
    public string displayName = "Unnamed CombatUpdates";

    [Server]
    public void OnEnemyKilled(NetworkCombatBase target, DamageSource damageType)
    {
        RpcEnemyKilled(target, damageType);
    }

    [ClientRpc]
    private void RpcEnemyKilled(NetworkCombatBase target, DamageSource damageType)
    {
        if (damageType == DamageSource.Ground)
            return;

        if (hasAuthority)
            GameUI.Instance.SetBannerMessage($"Killed {target.GetPlayerName()}!");
    }

    [Server]
    public void OnAvatarKilled(NetworkCombatBase source, DamageSource damageType)
    {
        RpcAvatarKilled(source, damageType);
    }

    [ClientRpc]
    protected virtual void RpcAvatarKilled(NetworkCombatBase source, DamageSource damageType)
    {
        switch (damageType)
        {
            case DamageSource.Player:
                GameUI.Instance.SpawnDeathMessage($"{source.GetPlayerName()} killed {displayName}!");
                break;
            case DamageSource.Ground:
                GameUI.Instance.SpawnDeathMessage($"{displayName} couldn't control their plane...");
                break;
        }
    }
}
