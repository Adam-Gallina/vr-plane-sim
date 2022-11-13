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

    [HideInInspector] public bool changingScenes = false;

    public List<NetworkLobbyPlayer> LobbyPlayers { get; } = new List<NetworkLobbyPlayer>();
    public List<NetworkGamePlayer> GamePlayers { get; } = new List<NetworkGamePlayer>();

    public override void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

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
        if (conn.identity != null && SceneManager.GetActiveScene().buildIndex == Constants.MainMenu.buildIndex)
        {
            NetworkLobbyPlayer player = conn.identity.gameObject.GetComponent<NetworkLobbyPlayer>();
            LobbyPlayers.Remove(player);

            NotifyPlayersOfReadyState();
        }
        else
        {
            NetworkGamePlayer player = conn.identity.gameObject.GetComponent<NetworkGamePlayer>();
            GamePlayers.Remove(player);
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        LobbyPlayers.Clear();
        GamePlayers.Clear();
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
        changingScenes = true;

        if (SceneManager.GetActiveScene().buildIndex == Constants.MainMenu.buildIndex && 
            newSceneName != Constants.MainMenu.name)
        {
            for (int i = LobbyPlayers.Count - 1; i >= 0; i--)
            {
                NetworkConnection conn = LobbyPlayers[i].connectionToClient;
                NetworkGamePlayer p = Instantiate(gamePlayerPrefab);
                p.SetPlaneColor(LobbyPlayers[i].planeColor);
                p.SetDisplayName(LobbyPlayers[i].DisplayName);
                p.SetCamType((Constants.CamType)LobbyPlayers[i].CamType);
                p.IsLeader = LobbyPlayers[i].IsLeader;

                NetworkServer.Destroy(conn.identity.gameObject);

                NetworkServer.ReplacePlayerForConnection(conn, p.gameObject);
            }
        }

        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        changingScenes = false;

        if (sceneName == Constants.MainMenu.name)
        {
            for (int i = GamePlayers.Count - 1; i >= 0; i--)
            {
                NetworkConnection conn = GamePlayers[i].connectionToClient;
                NetworkLobbyPlayer p = Instantiate(lobbyPlayerPrefab);
                p.CmdSetPlaneColor(GamePlayers[i].planeColor);
                p.DisplayName = GamePlayers[i].displayName;
                p.CamType = (int)GamePlayers[i].CamType;
                p.IsLeader = GamePlayers[i].IsLeader;

                NetworkServer.Destroy(conn.identity.gameObject);

                NetworkServer.ReplacePlayerForConnection(conn, p.gameObject);
            }
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
    
        if (SceneManager.GetActiveScene().buildIndex != Constants.MainMenu.buildIndex)
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

    public void ReturnToLobby()
    {
        ServerChangeScene(Constants.MainMenu.name);
    }

    public void StartGame(int map)
    {
        if (SceneManager.GetActiveScene().buildIndex == Constants.MainMenu.buildIndex)
        {
            if (!IsReadyToStart()) { return; }

            switch (map)
            {
                case 0:
                    ServerChangeScene(Constants.FfaTest.name);
                    break;
                case 1:
                    ServerChangeScene(Constants.DogfightTest.name);
                    break;
                default:
                    Debug.LogWarning("Not sure how to open map " + map + ", switching to FFA Test");
                    StartGame(0);
                    return;
            }
        }
    }
    #endregion
}
