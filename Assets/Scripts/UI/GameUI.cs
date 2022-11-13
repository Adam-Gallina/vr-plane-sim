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
    protected List<DeathMessageInstance> deathMessageLog = new List<DeathMessageInstance>();

    [Header("Kill banner")]
    [SerializeField] protected GameObject killMessagePrefab;
    [SerializeField] protected float killMessageHeight;
    [SerializeField] protected float killMessageDuration;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (deathMessageLog.Count > 0 && Time.time >= deathMessageLog[0].endTime)
        {
            IncrementDeathMessages();
        }
    }

    public abstract NametagUI SpawnNametag();

    public abstract void SpawnDeathMessage(string msg);
    public abstract void IncrementDeathMessages();

    public abstract void SpawnKillMessage(string target);

    protected struct DeathMessageInstance
    {
        public RectTransform rt;
        public float endTime;

        public DeathMessageInstance(GameObject obj, float endTime)
        {
            rt = obj.GetComponent<RectTransform>();
            this.endTime = endTime;
        }
    }
}
