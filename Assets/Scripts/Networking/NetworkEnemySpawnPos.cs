using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkEnemySpawnPos : MonoBehaviour
{
    public bool spawn = true;
    public NetworkIdentity enemyPrefab;

    [SerializeField] private GameObject model;

    private void Awake()
    {
        model.SetActive(false);
    }
}
