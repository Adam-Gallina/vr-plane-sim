using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkAvatarSpawner : NetworkBehaviour
{
    private int nextSpawn = 0;

    public override void OnStartServer()
    {
        PlaneSimNetworkManager.OnServerReadied += SpawnPlayer;
    }

    [ServerCallback]
    private void OnDestroy()
    {
        PlaneSimNetworkManager.OnServerReadied -= SpawnPlayer;
    }

    [Server]
    public void SpawnPlayer(NetworkConnection conn)
    {
        Transform t = MapController.Instance.GetSpawnPos(nextSpawn);
        GameObject avatar = Instantiate(conn.identity.GetComponent<NetworkGamePlayer>().avatarPrefab, t.position, t.rotation);
        NetworkServer.Spawn(avatar, conn);
        nextSpawn++;
    }
}
