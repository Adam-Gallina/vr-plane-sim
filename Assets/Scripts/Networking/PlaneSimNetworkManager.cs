using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class PlaneSimNetworkManager : NetworkManager
{
    public static PlaneSimNetworkManager Instance;

    private GameController gc;

    [SerializeField] private int minPlayers = 2;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection, int> OnServerReadied;

    [HideInInspector] public bool sceneChanging = false;

    public List<NetworkGamePlayer> Players { get; } = new List<NetworkGamePlayer>();

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
    public override void OnStartServer()
    {
        OnServerSceneChanged(SceneManager.GetActiveScene().name);
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
            NetworkGamePlayer player = conn.identity.gameObject.GetComponent<NetworkGamePlayer>();
            Players.Remove(player);
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        GameObject p = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, p);

        p.GetComponent<NetworkGamePlayer>().IsLeader = Players.Count == 0;
    }

    public override void OnServerChangeScene(string newSceneName)
    {
        foreach (NetworkGamePlayer p in Players)
            p.IsReady = false;

        sceneChanging = true;

        base.OnServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        gc = Instantiate(MapController.Instance.gameControllerPrefab);
        NetworkServer.Spawn(gc.gameObject);

        sceneChanging = false;
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);

        if (SceneManager.GetActiveScene().buildIndex != Constants.MainMenu.buildIndex)
        {
            OnServerReadied?.Invoke(conn, Players.IndexOf(conn.identity.GetComponent<NetworkGamePlayer>()));
            conn.identity.GetComponent<NetworkGamePlayer>().IsReady = true;
            NotifyPlayersOfReadyState();
        }
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
        gc.HandleReadyToStart(IsReadyToStart());
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers)
            return false;

        foreach (NetworkGamePlayer p in Players)
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
                case 2:
                    ServerChangeScene(Constants.RaceTest.name);
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
