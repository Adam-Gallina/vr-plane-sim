using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkBasicEnemy : NetworkPlaneController
{
    [Header("AI")]
    [SerializeField] private float turnSteerMod = 10;
    [SerializeField] private float altSteerMod = 10;
    [SyncVar]
    private Vector3 targetPos;

    [Header("Random Movement")]
    [SerializeField] private float minDirTime;
    [SerializeField] private float maxDirTime;
    [SerializeField] private float maxHeight;
    private float nextDirChange;
    private float h;

    protected void Start()
    {
        h = transform.position.y;
    }

    public override void OnStartAuthority()
    {
        CmdSetCanControl(true);
    }

    [ServerCallback]
    protected virtual void Update()
    {
        if (Time.time > nextDirChange)
        {
            nextDirChange = Time.time + Random.Range(minDirTime, maxDirTime);
            Vector2 newPos = Random.insideUnitCircle * NetworkEnemySpawner.Instance.maxSpawnRadius;
            targetPos = new Vector3(NetworkEnemySpawner.Instance.transform.position.x + newPos.x, Random.Range(h, h + maxHeight), NetworkEnemySpawner.Instance.transform.position.z + newPos.y);
        }

        float Dir(float ang, float mod)
        {
            if (Mathf.Abs(ang) < 90)
                return Mathf.Clamp(ang, -1, 1);
            else
                return Mathf.Clamp(ang, -mod, mod) / mod;
        }

        float dirX = -Vector3.SignedAngle(transform.forward, transform.position - targetPos, transform.up);
        float dirY = -Vector3.SignedAngle(transform.forward, transform.position - targetPos, transform.right);

        Vector2 dir = new Vector2(Dir(dirX, turnSteerMod), Dir(dirY, altSteerMod));

        Steer(dir, thrustSpeed, isServer);
        UpdateModel(isServer);
    }

    [ServerCallback]
    protected override void Death(NetworkCombatBase source, DamageSource sourceType)
    {
        if (MapController.Instance.allowAiRespawn)
            NetworkEnemySpawner.Instance.SpawnRandomEnemy();

        base.Death(source, sourceType);
    }
}
