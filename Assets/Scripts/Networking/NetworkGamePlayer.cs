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
    [HideInInspector] public Constants.CamType CamType = Constants.CamType.Unknown;
    [SyncVar(hook = nameof(OnReadyChanged))]
    public bool IsReady = false;

    public bool IsLeader;


    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        PlaneSimNetworkManager.Instance.Players.Add(this);

        SpawnGameUI();
    }

    public override void OnStopClient()
    {
        PlaneSimNetworkManager.Instance.Players.Remove(this);
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += SpawnGameUI;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= SpawnGameUI;
    }

    private void SpawnGameUI(Scene oldScene, Scene newScene)
    {
        SpawnGameUI();
    }
    private void SpawnGameUI()
    {
        StartCoroutine(WaitForMapController());
    }

    private IEnumerator WaitForMapController()
    {
        if (!MapController.Instance)
            yield return new WaitUntil(() => MapController.Instance);

        MapController.Instance.SpawnUI();

        if (!MultiCamUI.Instance)
            yield return new WaitUntil(() => MultiCamUI.Instance);

        if (!isLocalPlayer || SceneManager.GetActiveScene().buildIndex == Constants.MainMenu.buildIndex)
            MultiCamUI.Instance?.SpawnNametag().SetLinkedPlayer(this);
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
        if (GameController.Instance.playing)
            GameController.Instance.RespawnPlayer(connectionToClient, PlaneSimNetworkManager.Instance.Players.IndexOf(this));
    }
    #endregion
}
