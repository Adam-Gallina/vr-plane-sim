using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkCombatUpdates : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnDisplayNameChanged))]
    public string displayName = "Unnamed CombatUpdates";

    [SyncVar]
    public int enemyKills = 0;
    [SyncVar]
    public int avatarDeaths = 0;

    [Server]
    public void OnEnemyKilled(NetworkCombatBase target, DamageSource damageType)
    {
        RpcEnemyKilled(target, damageType);

        enemyKills++;
    }

    [ClientRpc]
    private void RpcEnemyKilled(NetworkCombatBase target, DamageSource damageType)
    {
        if (damageType == DamageSource.Ground)
            return;

        if (hasAuthority)
            GameUI.GInstance?.SetBannerMessage($"Killed {target.GetPlayerName()}!");
    }

    [Server]
    public void OnAvatarKilled(NetworkCombatBase source, DamageSource damageType)
    {
        RpcAvatarKilled(source, damageType);

        avatarDeaths++;
    }

    [ClientRpc]
    protected virtual void RpcAvatarKilled(NetworkCombatBase source, DamageSource damageType)
    {
        switch (damageType)
        {
            case DamageSource.Player:
                GameUI.GInstance?.SpawnDeathMessage($"{source.GetPlayerName()} killed {displayName}!");
                break;
            case DamageSource.Ground:
                GameUI.GInstance?.SpawnDeathMessage($"{displayName} couldn't control their plane...");
                break;
        }
    }

    protected virtual void OnDisplayNameChanged(string oldname, string newname)
    {

    }
}
