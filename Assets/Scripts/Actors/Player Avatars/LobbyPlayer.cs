using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class LobbyPlayer : NametagUI
{
    [SerializeField] private Image playerColorImage;
    [SerializeField] private Text playerReadyText;

    public override void SetLinkedPlayer(NetworkGamePlayer player)
    {
        LinkedPlayer = player;
        LobbyUI.LInstance.AddPlayer(this);

        if (!player.hasAuthority)
            return;

        if (string.IsNullOrEmpty(player.displayName))
            SetDisplayName(MainMenu.DisplayName);

        LobbyUI.LInstance.startGameButton.gameObject.SetActive(player.IsLeader);
        LobbyUI.LInstance.gameMode.gameObject.SetActive(player.IsLeader);

        LobbyUI.LInstance.readyButton.onClick.AddListener(ToggleReady);
        LobbyUI.LInstance.startGameButton.onClick.AddListener(StartGame);
        LobbyUI.LInstance.colorSelect.onColorSelect.AddListener(SetPlaneColor);
        LobbyUI.LInstance.camType.onValueChanged.AddListener(SetCamType);
    }

    protected override void Update()
    {
        
    }

    private void OnDestroy()
    {
        LobbyUI.LInstance.RemovePlayer(this);

        if (LinkedPlayer.hasAuthority)
        {
            LobbyUI.LInstance.readyButton.onClick.RemoveListener(ToggleReady);
            LobbyUI.LInstance.startGameButton.onClick.RemoveListener(StartGame);
            LobbyUI.LInstance.colorSelect.onColorSelect.RemoveListener(SetPlaneColor);
            LobbyUI.LInstance.camType.onValueChanged.RemoveListener(SetCamType);
        }
    }

    public void UpdateUI()
    {
        playerColorImage.color = LinkedPlayer.playerColor;
        playerNameText.text = LinkedPlayer.displayName;
        playerReadyText.text = LinkedPlayer.IsReady ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
    }

    public void ToggleReady()
    {
        LinkedPlayer.CmdSetIsReady(!LinkedPlayer.IsReady);
    }

    public void StartGame()
    {
        if (!LinkedPlayer.IsLeader) return;

        PlaneSimNetworkManager.Instance.StartGame(LobbyUI.LInstance.gameMode.value);
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
    #endregion
}
