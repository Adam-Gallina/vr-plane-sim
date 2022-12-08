using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class GameController : NetworkBehaviour
{
    public static GameController Instance;

    public bool playing = false;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        PlaneSimNetworkManager.OnServerReadied += SpawnPlayer;
    }

    public override void OnStopServer()
    {
        PlaneSimNetworkManager.OnServerReadied -= SpawnPlayer;
    }

    [Server]
    public virtual void HandleReadyToStart(bool ready)
    {
        RpcHandleReadyToStart(ready);
    }

    [ClientRpc]
    protected virtual void RpcHandleReadyToStart(bool ready)
    {
        Debug.LogWarning("RpcHandleReadyToStart not implemented (recieved '" + ready + "')");
    }

    [Server]
    protected virtual void CheckRoundEnd()
    {

    }

    [Server]
    protected virtual void HandlePlayerWin(NetworkGamePlayer p)
    {
        Debug.LogWarning("HandlePlayerWin not implemented ('" + p.displayName + "' won)");
    }

    [Server]
    public void SpawnPlayer(NetworkConnection conn, int player)
    {
        Transform t = MapController.Instance.GetSpawnPos(player);

        GameObject avatar = Instantiate(conn.identity.GetComponent<NetworkGamePlayer>().gameAvatarPrefab, t.position, t.rotation).gameObject;
        NetworkServer.Spawn(avatar, conn);

        conn.identity.GetComponent<NetworkGamePlayer>().RpcOnAvatarSpawned(avatar);
    }

    [Server]
    public void RespawnPlayer(NetworkConnection conn, int player)
    {
        SpawnPlayer(conn, player);
        RpcHandlePlayerRespawn(player);
    }

    [ClientRpc]
    protected virtual void RpcHandlePlayerRespawn(int player)
    {
        if (PlaneSimNetworkManager.Instance.Players[player].hasAuthority)
        {
            PlaneSimNetworkManager.Instance.Players[player].avatar.CmdSetCanControl(true);
        }
    }
}
