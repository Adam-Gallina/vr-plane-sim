using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Transform GetSpawnPos(int pos)
    {
        if (pos >= spawnPositions.Length)
        {
            Debug.LogError($"Missing spawn point {pos}");
            return null;
        }

        return spawnPositions[pos];
    }
}
