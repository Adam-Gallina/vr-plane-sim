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
    public void RespawnPlayer(NetworkConnection conn, int player)
    {
        if (MapController.Instance.allowPlayerRespawn)
        {
            NetworkAvatarSpawner.Instance.SpawnPlayer(conn, player);
            RpcHandlePlayerRespawn(player);
        }
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
