using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SeekingRocket : NetworkBullet
{
    [SerializeField] private float turnSpeed;

    [Header("Tracking")]
    [SerializeField] private float sphereRange;
    [SerializeField] private float coneRange = 350;
    [SerializeField] private float angle;
    [SerializeField] private float targetSpeedAdjustment = 2;

    private void OnDrawGizmos()
    {
        Vector3 away = transform.position + transform.forward * coneRange;
        float opp = Mathf.Tan(angle * Mathf.Deg2Rad) * coneRange;

        Gizmos.color = Color.blue;

        Gizmos.DrawLine(transform.position, away + transform.right * opp);
        Gizmos.DrawLine(transform.position, away + transform.right * -opp);
        Gizmos.DrawLine(transform.position, away + transform.up * opp);
        Gizmos.DrawLine(transform.position, away + transform.up * -opp);
        Gizmos.DrawWireSphere(away, opp);

        Gizmos.DrawWireSphere(transform.position, sphereRange);
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        Transform target = CheckForTargets();

        if (target)
        {
            rb.AddForce(((target.position + target.forward * targetSpeedAdjustment)- transform.position).normalized * turnSpeed, ForceMode.Force);
        }

        rb.velocity = rb.velocity.normalized * speed;
        transform.forward = rb.velocity;
    }

    private Transform CheckForTargets()
    {
        List<Collider> colls = new List<Collider>();
        colls.InsertRange(0, Physics.OverlapSphere(transform.position, sphereRange, targetLayer.value));
        colls.InsertRange(0, Physics.OverlapBox(transform.position + transform.forward * coneRange / 2, new Vector3(coneRange, coneRange, coneRange), transform.rotation, targetLayer.value));

        Collider closest = null;
        float dist = coneRange + 1;

        foreach (Collider c in colls)
        {
            NetworkPlaneController plane = c.GetComponentInParent<NetworkPlaneController>();
            if (!plane || plane == source)
                continue;

            if (!c.CompareTag(Constants.TagString(targetTag)))
                continue;

            if (Vector3.Angle(transform.forward, c.ClosestPoint(transform.position) - transform.position) > angle)
                continue;

            float d = Vector3.Distance(c.ClosestPoint(transform.position), transform.position);
            if (d < dist)
            {
                closest = c;
                dist = d;
            }
        }

        return closest?.transform;
    }
}
