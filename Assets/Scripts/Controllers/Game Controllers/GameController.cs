using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class GameController : NetworkBehaviour
{
    [Server]
    public virtual void HandleReadyToStart(bool ready)
    {
        RpcHandleReadyToStart(ready);
    }

    [ClientRpc]
    protected virtual void RpcHandleReadyToStart(bool ready)
    {
        Debug.LogError("RpcHandleReadyToStart not implemented (recieved '" + ready + "')");
    }
}
