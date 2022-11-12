using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class NetworkGamePlayer : NetworkBehaviour
{
    public GameObject nametagPrefab;
    public GameObject avatarPrefab;
    public WatchPlayerDeath avatarDeathEffectPrefab;

    [SyncVar]
    public string displayName = "Loading...";
    [SyncVar]
    [HideInInspector] public Constants.CamType CamType;
    [SyncVar]
    public NetworkPlayerPlane avatar;

    private PlaneSimNetworkManager network;
    private PlaneSimNetworkManager Network
    {
        get
        {
            if (network != null) { return network; }
            return network = NetworkManager.singleton as PlaneSimNetworkManager;
        }
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        Network.GamePlayers.Add(this);

    }

    public override void OnStopClient()
    {
        Network.GamePlayers.Remove(this);
    }

    private void OnEnable()
    {
        if (!isLocalPlayer)
            SceneManager.sceneLoaded += SpawnNameTag;

        if (!hasAuthority) return;

        if (avatar)
            avatar.OnPlayerDestroyed += OnAvatarDestroyed;
    }

    private void OnDisable()
    {
        if (!isLocalPlayer)
            SceneManager.sceneLoaded -= SpawnNameTag;

        if (!hasAuthority) return;

        if (avatar)
            avatar.OnPlayerDestroyed -= OnAvatarDestroyed;
    }

    private void SpawnNameTag(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == Constants.MainMenu.buildIndex || isLocalPlayer)
            return;

        Instantiate(nametagPrefab, GameObject.Find("Canvas").transform).GetComponent<NametagUI>().SetLinkedPlayer(this);
    }

    private void OnAvatarDestroyed(Transform t)
    {
        if (!hasAuthority) return;

        CameraController.Instance.SetTarget(null);

        WatchPlayerDeath effect = Instantiate(avatarDeathEffectPrefab);
        effect.SetPosition(t.position, t.forward);

        Invoke("RespawnAvatar", effect.duration);
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Server]
    public void SetCamType(Constants.CamType camType)
    {
        CamType = camType;
    }

    [ClientRpc]
    public void OnAvatarSpawned(GameObject avatarObj)
    {
        avatar = avatarObj.GetComponent<NetworkPlayerPlane>();

        if (!hasAuthority) return;

        avatar.OnPlayerDestroyed += OnAvatarDestroyed;

        CameraController.Instance.SetTarget(avatar.transform);
    }

    [Command]
    private void RespawnAvatar()
    {
        NetworkAvatarSpawner.Instance.SpawnPlayer(connectionToClient, PlaneSimNetworkManager.Instance.GamePlayers.IndexOf(this));
    }
}
