using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class LobbyPlayer : AvatarBase
{
    [SerializeField] private Image playerColorImage;
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text playerReadyText;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;
    [SyncVar(hook = nameof(HandlePlaneColorChanged))]
    public Color planeColor;
    [SyncVar]
    public int CamType;

    public override void OnStartAuthority()
    {
        if (string.Equals(DisplayName, "Loading..."))
            CmdSetDisplayName(MainMenu.DisplayName);

        LobbyUI.Instance.startGameButton.gameObject.SetActive(((NetworkGamePlayer)Player).IsLeader);
        LobbyUI.Instance.gameMode.gameObject.SetActive(((NetworkGamePlayer)Player).IsLeader);
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        //Network.LobbyPlayers.Add(this);

        LobbyUI.Instance.AddPlayer(this);

        LobbyUI.Instance.camType.onValueChanged.AddListener((int val) => CmdSetCamType(val));
    }

    public override void OnStopClient()
    {
        //Network.LobbyPlayers.Remove(this);

        LobbyUI.Instance.RemovePlayer(this);

        LobbyUI.Instance.camType.onValueChanged.RemoveAllListeners();
    }

    public void UpdateUI()
    {
        playerColorImage.color = planeColor;
        playerNameText.text = DisplayName;
        playerReadyText.text = ((NetworkGamePlayer)Player).IsLeader ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => LobbyUI.Instance.UpdateDisplay();
    public void HandleDisplayNameChanged(string oldValue, string newValue) => LobbyUI.Instance.UpdateDisplay();
    public void HandlePlaneColorChanged(Color oldValue, Color newValue) => LobbyUI.Instance.UpdateDisplay();

    [Command]
    public void CmdStartGame(int map)
    {
        if (PlaneSimNetworkManager.Instance.Players[0].connectionToClient != connectionToClient) { return; }

        PlaneSimNetworkManager.Instance.StartGame(map);
    }

    #region Getters/Setters
    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    public void CmdSetPlaneColor(Color col)
    {
        planeColor = col;
    }

    [Command]
    private void CmdSetCamType(int camType)
    {
        CamType = camType;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        PlaneSimNetworkManager.Instance.NotifyPlayersOfReadyState();
    }
    #endregion
}
