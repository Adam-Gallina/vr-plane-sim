using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyTurret : NetworkCombatBase
{
    [SerializeField] protected float fireDelay;
    protected float nextShot;

    [Header("AI")]
    [SerializeField] protected float turnSpeed;
    [SerializeField] protected float idleTurnSpeed;
    [SerializeField] protected float maxRange;
    [SerializeField] protected float minAimAssistRange;
    [SerializeField] protected float maxAimAssistOffset;
    [SerializeField] protected float minAimAssistOffset;
    protected Transform currTarget;

    [Header("Effects")]
    [SerializeField] protected Transform turretModel;
    [SerializeField] protected Transform firePointParent;

    [ServerCallback]
    private void Update()
    {
        currTarget = GetTarget();

        if (currTarget)
        {
            float dist = Vector3.Distance(transform.position, currTarget.position);
            float t = (dist - minAimAssistRange) / (maxRange - minAimAssistRange);
            if (t < 0)
                t = 0;

            Vector3 aimAssist = currTarget.forward * (minAimAssistOffset + (maxAimAssistOffset - minAimAssistOffset) * t);
            TargetPosition(currTarget.position + aimAssist, turnSpeed);

            TryAttack();
        }
        else
        {
            TargetPosition(firePointParent.position + firePointParent.right * 10, idleTurnSpeed);
        }
    }

    protected Transform GetTarget()
    {
        if (currTarget)
        {
            if (Vector3.Distance(transform.position, currTarget.position) < maxRange)
                return currTarget;
        }

        Collider[] colls = Physics.OverlapSphere(firePointParent.position, maxRange, targetLayer.value);
        if (colls.Length == 0)
            return null;
        Transform closest = colls[0].transform;
        float closestDist = float.MaxValue;
        foreach (Collider c in colls)
        {
            float dist = Vector3.Distance(c.transform.position, firePointParent.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = c.transform;
            }
        }

        return closest;
    }

    [Server]
    protected void TargetPosition(Vector3 pos, float speed)
    {
        Vector3 relativePos = pos - firePointParent.position;

        Vector3 rot = Vector3.RotateTowards(firePointParent.forward, relativePos, speed, speed) * Time.deltaTime;

        turretModel.forward = new Vector3(rot.x, 0, rot.z);
        firePointParent.forward = rot;
    }

    [Server]
    protected virtual void TryAttack()
    {
        if (Time.time >= nextShot)
        {
            nextShot = Time.time + fireDelay;
            ServerFire();
        }
    }
}
