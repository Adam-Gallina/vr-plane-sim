using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DogfightGameController : BasicGameController
{
    [SerializeField] private int killsRequiredToWin = 10;

    [ServerCallback]
    private void Update()
    {
        CheckRoundEnd();
    }

    [Server]
    protected override void CheckRoundEnd()
    {
        foreach (NetworkGamePlayer p in PlaneSimNetworkManager.Instance.Players)
        {
            if (p.enemyKills >= killsRequiredToWin)
            {
                HandlePlayerWin(p);
            }
        }
    }
}
