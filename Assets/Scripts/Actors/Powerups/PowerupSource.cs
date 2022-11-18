using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PowerupSource : NetworkHealthBase
{
    [SyncVar]
    [HideInInspector] public int powerupId;

    [SerializeField] private PowerupBase powerupPrefab;

    [ServerCallback]
    protected override void Death(NetworkCombatBase source, DamageSource sourceType)
    {
        if (powerupPrefab == null)
            Debug.LogWarning(name + " does not have a powerupPrefab set");


        PowerupBase powerup = Instantiate(powerupPrefab);
        NetworkServer.Spawn(powerup.gameObject);
        source.SetSpecial(powerup);

        ((BasicGameController)GameController.Instance).OnPowerupCollected(powerupId);

        base.Death(source, sourceType);
    }
}
