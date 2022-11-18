using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class PowerupBase : NetworkBehaviour
{
    public string powerupName;

    public abstract bool UsePowerup(NetworkCombatBase source, Transform spawn, DamageSource sourceType);
}
