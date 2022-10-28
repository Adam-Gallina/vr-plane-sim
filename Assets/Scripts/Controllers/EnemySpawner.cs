using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    public GameObject enemyPrefab;
    public int spawnCount;
    public float minSpawnRadius;
    public float maxSpawnRadius;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxSpawnRadius);
    }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        float da = 360f / spawnCount;
        for (int i = 0; i < spawnCount; i++)
        {
            float ang = da * i;
            transform.localEulerAngles = new Vector3(0, ang, 0);
            SpawnEnemy(transform.position + transform.forward * Random.Range(minSpawnRadius, maxSpawnRadius));
        }
    }

    private void SpawnEnemy(Vector3 pos)
    {
        GameObject o = Instantiate(enemyPrefab);
        o.transform.position = pos;
        o.transform.localEulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
    }
}
