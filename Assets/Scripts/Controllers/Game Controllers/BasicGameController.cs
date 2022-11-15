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

            foreach (NetworkGamePlayer p in PlaneSimNetworkManager.Instance.Players)
            {
                if (p.hasAuthority)
                    ((NetworkPlayerPlane)p.avatar).canMove = true;
            }
        }
    }

    private IEnumerator StartSequence()
    {
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
}
