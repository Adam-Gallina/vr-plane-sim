using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkLobbyPlayer : NetworkBehaviour
{
    public Text playerNameText;
    public Text playerReadyText;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;

    [SyncVar]
    public int CamType;

    private bool isLeader;
    public bool IsLeader
    {
        set
        {
            isLeader = value;

            if (hasAuthority)
            {
                LobbyUI.Instance.startGameButton.gameObject.SetActive(value);
                LobbyUI.Instance.gameMode.gameObject.SetActive(value);
            }
        }
    }

    private PlaneSimNetworkManager network;
    private PlaneSimNetworkManager Network
    {
        get
        {
            if (network != null) { return network; }
            return network = NetworkManager.singleton as PlaneSimNetworkManager;
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetDisplayName(MainMenu.DisplayName);
    }

    public override void OnStartClient()
    {
        Network.LobbyPlayers.Add(this);

        LobbyUI.Instance.AddPlayer(this);

        LobbyUI.Instance.camType.onValueChanged.AddListener((int val) => CmdSetCamType(val));
    }

    public override void OnStopClient()
    {
        Network.LobbyPlayers.Remove(this);

        LobbyUI.Instance.RemovePlayer(this);

        LobbyUI.Instance.camType.onValueChanged.RemoveAllListeners();
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => LobbyUI.Instance.UpdateDisplay();
    public void HandleDisplayNameChanged(string oldValue, string newValue) => LobbyUI.Instance.UpdateDisplay();

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!isLeader) { return; }

        LobbyUI.Instance.startGameButton.interactable = readyToStart;
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        Network.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame(int map)
    {
        if (Network.LobbyPlayers[0].connectionToClient != connectionToClient) { return; }

        Network.StartGame(map);
    }

    [Command]
    private void CmdSetCamType(int camType)
    {
        CamType = camType;
    }
}
