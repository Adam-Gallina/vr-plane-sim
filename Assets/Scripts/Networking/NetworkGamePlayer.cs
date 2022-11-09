using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkGamePlayer : NetworkBehaviour
{
    public GameObject avatarPrefab;

    [SyncVar]
    private string displayName = "Loading...";
    [SyncVar]
    [HideInInspector] public Constants.CamType CamType;

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
}
