using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("Player nametag")]
    [SerializeField] protected GameObject nametagPrefab;

    [Header("Death messages")]
    [SerializeField] protected GameObject deathMessagePrefab;
    [SerializeField] protected float deathMessageHeight;
    [SerializeField] protected float deathMessageDuration;

    [Header("Kill banner")]
    [SerializeField] protected GameObject killMessagePrefab;
    [SerializeField] protected float killMessageHeight;
    [SerializeField] protected float killMessageDuration;

    private void Awake()
    {
        Instance = this;
    }

    public abstract NametagUI SpawnNametag();

    public abstract void SpawnDeathMessage(string msg);

    public abstract void SpawnKillMessage(string target);
}
