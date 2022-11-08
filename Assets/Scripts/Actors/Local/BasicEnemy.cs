using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : PlaneController
{
    [Header("AI")]
    private Vector3 targetPos;
    [SerializeField] private float turnSteerMod = 10;
    [SerializeField] private float altSteerMod = 10;

    [Header("Random Movement")]
    [SerializeField] private float minDirTime;
    [SerializeField] private float maxDirTime;
    //[SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;
    private float nextDirChange;
    private float h;

    [Header("Death Effect")]
    [SerializeField] private GameObject deathPrefab;

    protected void Start()
    {
        h = transform.position.y;
    }

    private void Update()
    {
        if (Time.time > nextDirChange)
        {
            nextDirChange = Time.time + Random.Range(minDirTime, maxDirTime);
            Vector2 newPos = Random.insideUnitCircle * EnemySpawner.Instance.maxSpawnRadius;
            targetPos = new Vector3(EnemySpawner.Instance.transform.position.x + newPos.x, Random.Range(h, h + maxHeight), EnemySpawner.Instance.transform.position.z + newPos.y);
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

        Steer(dir, thrustSpeed);
        UpdateModel();
    }

    protected override void Death()
    {
        GameObject effect = Instantiate(deathPrefab);
        effect.transform.position = transform.position;
        effect.transform.localEulerAngles = transform.localEulerAngles;

        effect.transform.GetChild(0).localEulerAngles = model.localEulerAngles;

        Rigidbody erb = effect.GetComponent<Rigidbody>();
        erb.velocity = rb.velocity;
        erb.angularVelocity = rb.angularVelocity * 10 + transform.right * 5;
        
        Destroy(gameObject);
    }
}
