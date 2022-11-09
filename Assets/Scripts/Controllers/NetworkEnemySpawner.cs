using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkEnemySpawner : NetworkBehaviour
{
    public static NetworkEnemySpawner Instance;

    public GameObject enemyPrefab;
    public int spawnCount;
    public Vector3 spawnDir;
    //public float minSpawnRadius;
    //public float maxSpawnRadius;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < spawnCount; i++)
            Gizmos.DrawWireSphere(transform.position + spawnDir * i, 2);

        //Gizmos.DrawWireSphere(transform.position, maxSpawnRadius);
    }

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        for (int i = 0; i < spawnCount; i++)
            SpawnEnemy(transform.position + spawnDir * i);

        /*float da = 360f / spawnCount;
        for (int i = 0; i < spawnCount; i++)
        {
            float ang = da * i;
            transform.localEulerAngles = new Vector3(0, ang, 0);
            SpawnEnemy(transform.position + transform.forward * Random.Range(minSpawnRadius, maxSpawnRadius));
        }*/
    }
    
    [Server]
    private void SpawnEnemy(Vector3 pos)
    {
        GameObject o = Instantiate(enemyPrefab, pos, Quaternion.identity);
        NetworkServer.Spawn(o);
        //o.transform.localEulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
    }
}
