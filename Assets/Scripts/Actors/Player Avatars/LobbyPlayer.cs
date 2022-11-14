using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class LobbyPlayer : NametagUI
{
    [SerializeField] private Image playerColorImage;
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text playerReadyText;

    public override void SetLinkedPlayer(NetworkGamePlayer player)
    {
        LinkedPlayer = player;

        if (player.hasAuthority)
        {
            LobbyUI.LInstance.startGameButton.gameObject.SetActive(player.IsLeader);
            LobbyUI.LInstance.gameMode.gameObject.SetActive(player.IsLeader);
        }

        if (string.IsNullOrEmpty(player.displayName))
            SetDisplayName(MainMenu.DisplayName);

        LobbyUI.LInstance.AddPlayer(this);

        LobbyUI.LInstance.camType.onValueChanged.AddListener((int val) => SetCamType(val));
    }


    protected override void Update()
    {
        
    }

    private void OnDestroy()
    {
        LobbyUI.LInstance.RemovePlayer(this);

        if (LinkedPlayer.hasAuthority)
            LobbyUI.LInstance.camType.onValueChanged.RemoveAllListeners();
    }

    public void UpdateUI()
    {
        playerColorImage.color = LinkedPlayer.playerColor;
        playerNameText.text = LinkedPlayer.displayName;
        playerReadyText.text = LinkedPlayer.IsReady ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
    }


    public void StartGame(int map)
    {
        if (!LinkedPlayer.IsLeader) return;

        PlaneSimNetworkManager.Instance.StartGame(map);
    }

    #region Getters/Setters
    private void SetDisplayName(string displayName)
    {
        LinkedPlayer.CmdSetDisplayName(displayName);
    }

    public void SetPlaneColor(Color col)
    {
        LinkedPlayer.CmdSetPlayerColor(col);
    }

    private void SetCamType(int camType)
    {
        LinkedPlayer.CmdSetCamType((Constants.CamType)camType);
    }

    public void ToggleReady()
    {
        LinkedPlayer.CmdSetIsReady(!LinkedPlayer.IsReady);
    }
    #endregion
}
