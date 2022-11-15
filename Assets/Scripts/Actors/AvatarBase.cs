using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AvatarBase : NetworkBehaviour
{
    private NetworkCombatUpdates player;
    public NetworkCombatUpdates Player
    {
        get
        {
            if (player != null)
                return player;

            foreach (NetworkCombatUpdates p in PlaneSimNetworkManager.Instance.Players)
            {
                if (p.connectionToClient == connectionToClient)
                {
                    player = p;
                    Debug.Log("Found player (" + p?.displayName + "): " + p);
                    return player;
                }
            }

            return null;
        }
    }
    [SyncVar]
    [SerializeField] protected string playerName;
    [SyncVar(hook = nameof(OnSetPlayerColor))]
    private Color playerColor;
    [SyncVar]
    protected bool canControl = false;

    #region Getters/Setters
    [Command]
    public void CmdSetCombatUpdates(NetworkCombatUpdates player)
    {
        this.player = player;
    }

    [Command]
    public void CmdSetPlayerName(string newName)
    {
        playerName = newName;
    }

    public string GetPlayerName()
    {
        return string.IsNullOrEmpty(playerName) ? "Unnamed Avatar (" + name + ")" : playerName;
    }

    [Command]
    public void CmdSetPlayerColor(Color col)
    {
        playerColor = col;
    }

    protected virtual void OnSetPlayerColor(Color oldCol, Color newCol)
    {
    }

    [Command]
    public void CmdSetCanControl(bool canControl)
    {
        this.canControl = canControl;
    }
    #endregion
}
