using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class GameController : NetworkBehaviour
{
    public static GameController Instance;

    public bool playing = false;

    public override void OnStartClient()
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
}
