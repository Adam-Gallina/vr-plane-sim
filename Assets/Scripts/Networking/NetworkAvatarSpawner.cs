using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkAvatarSpawner : NetworkBehaviour
{
    public static NetworkAvatarSpawner Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        PlaneSimNetworkManager.OnServerReadied += SpawnPlayer;
    }

    public override void OnStopServer()
    {
        PlaneSimNetworkManager.OnServerReadied -= SpawnPlayer;
    }

    [Server]
    public void SpawnPlayer(NetworkConnection conn, int player)
    {
        Transform t = MapController.Instance.GetSpawnPos(player);

        GameObject avatar = Instantiate(conn.identity.GetComponent<NetworkGamePlayer>().avatarPrefab, t.position, t.rotation);
        NetworkServer.Spawn(avatar, conn);

        conn.identity.GetComponent<NetworkGamePlayer>().RpcOnAvatarSpawned(avatar);
    }
}
