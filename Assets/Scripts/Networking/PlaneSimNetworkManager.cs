using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class PlaneSimNetworkManager : NetworkManager
{
    public static PlaneSimNetworkManager Instance;

    [SerializeField] private int minPlayers = 2;

    [Header("Prefabs")]
    [SerializeField] private NetworkLobbyPlayer lobbyPlayerPrefab;
    [SerializeField] private NetworkGamePlayer gamePlayerPrefab;
    [SerializeField] private GameObject avatarSpawnerPrefab;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection, int> OnServerReadied;

    public List<NetworkLobbyPlayer> LobbyPlayers { get; } = new List<NetworkLobbyPlayer>();
    public List<NetworkGamePlayer> GamePlayers { get; } = new List<NetworkGamePlayer>();

    public override void Awake()
    {
        base.Awake();

        Instance = this;
    }

    #region Server Callbacks
    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if (SceneManager.GetActiveScene().buildIndex != Constants.MainMenu.buildIndex)
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            NetworkLobbyPlayer player = conn.identity.gameObject.GetComponent<NetworkLobbyPlayer>();

            LobbyPlayers.Remove(player);

            NotifyPlayersOfReadyState();
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        LobbyPlayers.Clear();
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().buildIndex == Constants.MainMenu.buildIndex)
        {
            NetworkLobbyPlayer roomPlayerInstance = Instantiate(lobbyPlayerPrefab);
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);

            roomPlayerInstance.IsLeader = LobbyPlayers.Count == 0;
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        if (SceneManager.GetActiveScene().buildIndex == Constants.MainMenu.buildIndex && 
            newSceneName != Constants.MainMenu.name)
        {
            for (int i = LobbyPlayers.Count - 1; i >= 0; i--)
            {
                NetworkConnection conn = LobbyPlayers[i].connectionToClient;
                NetworkGamePlayer p = Instantiate(gamePlayerPrefab);
                p.SetDisplayName(LobbyPlayers[i].DisplayName);
                p.SetCamType((Constants.CamType)LobbyPlayers[i].CamType);

                NetworkServer.Destroy(conn.identity.gameObject);

                NetworkServer.ReplacePlayerForConnection(conn, p.gameObject);
            }
        }

        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName == Constants.MainMenu.name)
        {
            Debug.LogWarning("How to return to main?");
        }
        else
        {
            GameObject playerSpawnSystemInstance = Instantiate(avatarSpawnerPrefab);
            NetworkServer.Spawn(playerSpawnSystemInstance);
        }
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
    
        OnServerReadied?.Invoke(conn, GamePlayers.IndexOf(conn.identity.GetComponent<NetworkGamePlayer>()));
    }
    #endregion

    #region Client Callbacks
    public override void OnClientConnect()
    {
        base.OnClientConnect();

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        OnClientDisconnected?.Invoke();
    }
    #endregion

    #region Lobby
    public void NotifyPlayersOfReadyState()
    {
        bool ready = IsReadyToStart();

        foreach (NetworkLobbyPlayer p in LobbyPlayers)
        {
            p.HandleReadyToStart(ready);
        }
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers)
            return false;

        foreach (NetworkLobbyPlayer p in LobbyPlayers)
        {
            if (!p.IsReady)
            {
                return false;
            }
        }

        return true;
    }

    public void StartGame()
    {
        if (SceneManager.GetActiveScene().buildIndex == Constants.MainMenu.buildIndex)
        {
            if (!IsReadyToStart()) { return; }

            ServerChangeScene(Constants.FfaTest.name);
        }
    }
    #endregion
}
