using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BasicGameController : GameController
{
    [SerializeField] protected string[] bannerMessages = new string[] { "READY?", "3", "2", "1", "GO!" };
    [SerializeField] protected float messageTime = 1;

    public override void OnStartClient()
    {
        GameUI.GInstance?.SetBannerMessage("Waiting for players...", -1);
    }

    [Server]
    public override void HandleReadyToStart(bool ready)
    {
        StartCoroutine(StartSequence());
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

    private IEnumerator StartSequence()
    {
        SpawnAllPowerups();

        foreach (string m in bannerMessages)
        {
            RpcSendServerBannerMessage(m, -1);
            yield return new WaitForSeconds(messageTime);
        }

        RpcSendServerBannerMessage(string.Empty, 0);

        RpcHandleReadyToStart(true);
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
}
