using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BasicGameController : GameController
{
    [SerializeField] protected string[] startBannerMessages = new string[] { "READY?", "3", "2", "1", "GO!" };
    [SerializeField] protected string[] endBannerMessages = new string[] { "Returning to lobby..." };
    [SerializeField] protected float messageTime = 1;

    private bool started = false;
    private bool ended = false;

    public override void OnStartClient()
    {
        GameUI.GInstance?.SetBannerMessage("Waiting for players...", -1);
    }

    [Server]
    public override void HandleReadyToStart(bool ready)
    {
        if (!started)
        {
            started = true;
            StartCoroutine(StartSequence());
        }
    }

    [ClientRpc]
    protected override void RpcHandleReadyToStart(bool ready)
    {
        if (ready)
        {
            playing = true;

            if (MapController.Instance.DEBUG_toggleControl)
                return;

            foreach (NetworkGamePlayer p in PlaneSimNetworkManager.Instance.Players)
            {
                if (p.hasAuthority)
                    p.avatar.CmdSetCanControl(true);
            }
        }
    }

    [Server]
    private IEnumerator StartSequence()
    {
        SpawnAllPowerups();
        SpawnAllMapEnemies();

        foreach (string m in startBannerMessages)
        {
            RpcSendServerBannerMessage(m, -1);
            yield return new WaitForSeconds(messageTime);
        }

        RpcSendServerBannerMessage(string.Empty, 0);

        RpcHandleReadyToStart(true);
    }

    [Server]
    protected override void HandlePlayerWin(NetworkGamePlayer p)
    {
        if (!ended)
        {
            ended = true;
            StartCoroutine(OnPlayerWin(p));
            Debug.Log(p.displayName + " won!");
        }
    }

    [Server]
    protected IEnumerator OnPlayerWin(NetworkGamePlayer p)
    {
        foreach (string m in endBannerMessages)
        {
            RpcSendServerBannerMessage(p.displayName + " won!\n" + m, -1);
            yield return new WaitForSeconds(messageTime);
        }

        RpcSendServerBannerMessage(string.Empty, 0);

        PlaneSimNetworkManager.Instance.ReturnToLobby();
    }

    [ClientRpc]
    private void RpcSendServerBannerMessage(string message, float duration)
    {
        if (!GameUI.GInstance)
        {
            Debug.LogWarning("Trying to send message '" + message + "' to players, but no GameUI exists");
            return;
        }

        GameUI.GInstance.SetBannerMessage(message, duration);
    }

    #region Map Enemies
    private void SpawnAllMapEnemies()
    {
        foreach (NetworkEnemySpawnPos spawner in MapController.Instance.GetMapEnemies())
            if (spawner.spawn)
                SpawnMapEnemy(spawner);
    }

    private void SpawnMapEnemy(NetworkEnemySpawnPos spawner)
    {
        GameObject newEnemy = Instantiate(spawner.enemyPrefab.gameObject, spawner.transform.position, spawner.transform.rotation);
        NetworkServer.Spawn(newEnemy);
    }
    #endregion

    #region Powerups
    [Server]
    private void SpawnAllPowerups()
    {
        for (int i = 0; i < MapController.Instance.TotalPowerupSpawns; i++)
        {
            SpawnPowerup(MapController.Instance.GetRandomPowerup(),
                         MapController.Instance.GetPowerupPos(i),
                         i);
        }
    }

    [Server]
    public void OnPowerupCollected(int powerupId)
    {
        if (MapController.Instance.allowPowerupRespawn)
            StartCoroutine(RespawnPowerup(MapController.Instance.GetRandomPowerup(),
                                          MapController.Instance.GetPowerupPos(powerupId),
                                          MapController.Instance.powerupRespawnTime,
                                          powerupId));
    }

    [Server]
    private IEnumerator RespawnPowerup(PowerupSource prefab, Transform spawnPos, float time, int id)
    {
        yield return new WaitForSeconds(time);
        SpawnPowerup(prefab, spawnPos, id);
    }

    [Server]
    private void SpawnPowerup(PowerupSource prefab, Transform spawnPos, int id)
    {
        PowerupSource newPowerup = Instantiate(prefab, spawnPos.position, spawnPos.rotation);
        NetworkServer.Spawn(newPowerup.gameObject);

        newPowerup.powerupId = id;
    }
    #endregion
}
