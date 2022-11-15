using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class NetworkGamePlayer : NetworkCombatUpdates
{
    public AvatarBase gameAvatarPrefab;
    public WatchPlayerDeath avatarDeathEffectPrefab;

    [SyncVar]
    public AvatarBase avatar;

    [SyncVar(hook = nameof(OnPlayerColorChanged))]
    public Color playerColor;
    [SyncVar]
    [HideInInspector] public Constants.CamType CamType;
    [SyncVar(hook = nameof(OnReadyChanged))]
    public bool IsReady = false;

    public bool IsLeader;


    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        PlaneSimNetworkManager.Instance.Players.Add(this);

        SpawnNameTag();
    }

    public override void OnStopClient()
    {
        PlaneSimNetworkManager.Instance.Players.Remove(this);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SpawnNameTag;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SpawnNameTag;
    }

    private void SpawnNameTag(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != Constants.MainMenu.buildIndex && isLocalPlayer)
            return;

        SpawnNameTag();
    }
    private void SpawnNameTag()
    {
        MultiCamUI.Instance.SpawnNametag().SetLinkedPlayer(this);
    }

    #region Getters/Setters
    [Command]
    public void CmdSetPlayerColor(Color col)
    {
        playerColor = col;
    }
    [Command]
    public void CmdSetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Command]
    public void CmdSetCamType(Constants.CamType camType)
    {
        CamType = camType;
    }
    
    [Command]
    public void CmdSetIsReady(bool isReady)
    {
        IsReady = isReady;

        PlaneSimNetworkManager.Instance.NotifyPlayersOfReadyState();
    }

    private void OnPlayerColorChanged(Color oldcol, Color newcol)
    {
        OnPlayerInfoChanged();
    }
    protected override void OnDisplayNameChanged(string oldval, string newval)
    {
        OnPlayerInfoChanged();
    }
    private void OnReadyChanged(bool oldval, bool newval)
    {
        OnPlayerInfoChanged();
    }

    private void OnPlayerInfoChanged()
    {
        if (SceneManager.GetActiveScene().buildIndex == Constants.MainMenu.buildIndex)
            LobbyUI.LInstance.UpdateDisplay();
    }
    #endregion

    #region Avatar
    [ClientRpc]
    public void RpcOnAvatarSpawned(GameObject avatarObj)
    {
        avatar = avatarObj.GetComponent<NetworkPlayerPlane>();

        if (!hasAuthority) return;

        avatar.CmdSetCombatUpdates(this);
        avatar.CmdSetPlayerName(displayName);
        avatar.CmdSetPlayerColor(playerColor);

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

        if (MapController.Instance.allowPlayerRespawn)
            Invoke(nameof(RespawnAvatar), effect.duration);
    }

    [Command]
    private void RespawnAvatar()
    {
        NetworkAvatarSpawner.Instance.SpawnPlayer(connectionToClient, PlaneSimNetworkManager.Instance.Players.IndexOf(this));
    }
    #endregion
}
