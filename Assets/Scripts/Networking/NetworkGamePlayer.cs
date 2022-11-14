using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class NetworkGamePlayer : NetworkCombatUpdates
{
    [HideInInspector] public AvatarBase avatarPrefab;
    public WatchPlayerDeath avatarDeathEffectPrefab;

    [SyncVar]
    public Color planeColor;
    [SyncVar]
    [HideInInspector] public Constants.CamType CamType;
    [SyncVar]
    public AvatarBase avatar;

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

    public override void OnStartAuthority()
    {
        SelectAvatar();
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        PlaneSimNetworkManager.Instance.Players.Add(this);

    }

    public override void OnStopClient()
    {
        PlaneSimNetworkManager.Instance.Players.Remove(this);
    }

    private void OnEnable()
    {
        if (isLocalPlayer)
            SceneManager.sceneLoaded += SelectAvatar;
        else
            SceneManager.sceneLoaded += SpawnNameTag;
    }

    private void OnDisable()
    {
        if (isLocalPlayer)
            SceneManager.sceneLoaded -= SelectAvatar;
        else
            SceneManager.sceneLoaded -= SpawnNameTag;
    }

    private void SpawnNameTag(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == Constants.MainMenu.buildIndex || isLocalPlayer)
            return;

        GameUI.Instance.SpawnNametag().SetLinkedPlayer(this);
    }

    #region Getters/Setters
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
    #endregion

    #region Avatar
    private void SelectAvatar(Scene scene, LoadSceneMode mode)
    {
        SelectAvatar();
    }
    private void SelectAvatar()
    {
        CmdSetAvatar(GameController.Instance.availableAvatars[0]);
    }

    [Command]
    private void CmdSetAvatar(AvatarBase prefab)
    {
        avatarPrefab = prefab;
    }

    [ClientRpc]
    public void RpcOnAvatarSpawned(GameObject avatarObj)
    {
        avatar = avatarObj.GetComponent<NetworkPlayerPlane>();

        if (!hasAuthority) return;

        avatar.CmdSetCombatUpdates(this);
        avatar.CmdSetPlayerName(displayName);
        avatar.CmdSetPlayerColor(planeColor);

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
        NetworkAvatarSpawner.Instance.SpawnPlayer(connectionToClient, PlaneSimNetworkManager.Instance.Players.IndexOf(this));
    }
    #endregion


    public void HandleReadyToStart(bool readyToStart)
    {
        if (!isLeader) { return; }

        LobbyUI.Instance.startGameButton.interactable = readyToStart;
    }

    public bool IsReady()
    {
        if (SceneManager.GetActiveScene().buildIndex == Constants.MainMenu.buildIndex)
        {
            // return avatar.isready
        }

        return true;
    }

    
}
