using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnFormation { RoundRobin, Assigned, Random }
public class MapController : MonoBehaviour
{
    public static MapController Instance;

    [SerializeField] private Transform[] spawnPositions;

    private void OnDrawGizmos()
    {
        foreach (Transform t in spawnPositions)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(t.position, 1);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(t.position, t.position + t.forward * 2);
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public Transform GetSpawnPos(int player)
    {
        switch (GameController.Instance.spawnFormat)
        {
            case SpawnFormation.RoundRobin:
                return RoundRobin();
            case SpawnFormation.Assigned:
                return AssignedSpawn(player);
            default:
                Debug.LogWarning($"{GameController.Instance.spawnFormat} has not been implemented, defaulting to RoundRobin");
                return RoundRobin();
        }
    }

    private int nextPos = 0;
    private Transform RoundRobin()
    {
        Transform t = spawnPositions[nextPos];

        if (nextPos++ >= spawnPositions.Length)
            nextPos = 0;

        return t;
    }

    private Transform AssignedSpawn(int player)
    {
        if (player < 0 || player >= spawnPositions.Length)
        {
            Debug.LogWarning($"Missing spawn point {player}");
            return null;
        }

        return spawnPositions[player];
    }
}
