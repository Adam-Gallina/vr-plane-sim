using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MainMenuGameController : GameController
{
    [ClientRpc]
    protected override void RpcHandleReadyToStart(bool ready)
    {
        LobbyUI.LInstance.startGameButton.interactable = ready;
    }
}
