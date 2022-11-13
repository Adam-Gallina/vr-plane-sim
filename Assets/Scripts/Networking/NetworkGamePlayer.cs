using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class NetworkGamePlayer : NetworkCombatUpdates
{
    public GameObject avatarPrefab;
    public WatchPlayerDeath avatarDeathEffectPrefab;

    [SyncVar]
    public Color planeColor;
    [SyncVar]
    [HideInInspector] public Constants.CamType CamType;
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

    private bool isLeader;
    public bool IsLeader
    {
        get
        {
            return isLeader;
        }
        set
        {
            isLeader = value;

            if (hasAuthority)
            {
                GameUI.Instance.returnToLobbyBtn.SetActive(value);
            }
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
    }

    private void OnDisable()
    {
        if (!isLocalPlayer)
            SceneManager.sceneLoaded -= SpawnNameTag;
    }

    private void SpawnNameTag(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == Constants.MainMenu.buildIndex || isLocalPlayer)
            return;

        GameUI.Instance.SpawnNametag().SetLinkedPlayer(this);
    }

    [Server]
    public void SetPlaneColor(Color col)
    {
        planeColor = col;
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
        avatar.SetCombatUpdates(this);
        avatar.SetCombatName(displayName);

        avatar.body.GetComponent<Renderer>().material.color = planeColor;

        if (!hasAuthority) return;

        CameraController.Instance.SetTarget(avatar.transform);
    }

    [ClientRpc]
    protected override void RpcAvatarKilled(NetworkCombatBase source, DamageSource damageType)
    {
        base.RpcAvatarKilled(source, damageType);

        if (!hasAuthority) return;

        CameraController.Instance.SetTarget(null);

        WatchPlayerDeath effect = Instantiate(avatarDeathEffectPrefab);
        effect.SetPosition(avatar.transform.position, avatar.transform.forward);

        if (GameController.Instance.allowPlayerRespawn)
            Invoke(nameof(RespawnAvatar), effect.duration);
    }

    [Command]
    private void RespawnAvatar()
    {
        NetworkAvatarSpawner.Instance.SpawnPlayer(connectionToClient, PlaneSimNetworkManager.Instance.GamePlayers.IndexOf(this));
    }
}
