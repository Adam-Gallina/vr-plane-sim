using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkGamePlayer : NetworkBehaviour
{
    public GameObject avatarPrefab;
    public WatchPlayerDeath avatarDeathEffectPrefab;

    [SyncVar]
    private string displayName = "Loading...";
    [SyncVar]
    [HideInInspector] public Constants.CamType CamType;

    private NetworkPlayerPlane avatar;

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
        if (!hasAuthority) return;

        if (avatar)
            avatar.OnPlayerDestroyed += OnAvatarDestroyed;
    }

    private void OnDisable()
    {
        if (!hasAuthority) return;

        if (avatar)
            avatar.OnPlayerDestroyed -= OnAvatarDestroyed;
    }

    private void OnAvatarDestroyed()
    {
        if (!hasAuthority) return;

        CameraController.Instance.SetTarget(null);

        WatchPlayerDeath effect = Instantiate(avatarDeathEffectPrefab);
        effect.SetPosition(avatar.transform.position, avatar.transform.forward);

        avatar = null;

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
        if (!hasAuthority) return;

        avatar = avatarObj.GetComponent<NetworkPlayerPlane>();
        avatar.OnPlayerDestroyed += OnAvatarDestroyed;

        CameraController.Instance.SetTarget(avatar.transform);
    }

    [Command]
    private void RespawnAvatar()
    {
        NetworkAvatarSpawner.Instance.SpawnPlayer(connectionToClient, PlaneSimNetworkManager.Instance.GamePlayers.IndexOf(this));
    }
}
