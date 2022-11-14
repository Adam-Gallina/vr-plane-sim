using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultiCamUI : MonoBehaviour
{
    public static MultiCamUI Instance;

    [Header("Player nametag")]
    [SerializeField] protected GameObject nametagPrefab;

    protected virtual void Awake()
    {
        Instance = this;
    }

    public abstract NametagUI SpawnNametag();
}
