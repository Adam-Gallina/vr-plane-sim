using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SeekingRocket : NetworkBullet
{
    [SerializeField] private float turnSpeed;

    [Header("Tracking")]
    [SerializeField] private float range;
    [SerializeField] private float angle;

    private void OnDrawGizmos()
    {
        Vector3 away = transform.position + transform.forward * range;
        float opp = Mathf.Tan(angle * Mathf.Deg2Rad) * range;

        Gizmos.color = Color.blue;

        Gizmos.DrawLine(transform.position, away + transform.right * opp);
        Gizmos.DrawLine(transform.position, away + transform.right * -opp);
        Gizmos.DrawLine(transform.position, away + transform.up * opp);
        Gizmos.DrawLine(transform.position, away + transform.up * -opp);
        Gizmos.DrawWireSphere(away, opp);
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        Transform target = CheckForTargets();

        if (target)
        {
            rb.AddForce((target.position - transform.position).normalized * turnSpeed, ForceMode.Impulse);
        }

        rb.velocity = rb.velocity.normalized * speed;
    }

    private Transform CheckForTargets()
    {
        Collider[] colls = Physics.OverlapBox(transform.position + transform.forward * range / 2, new Vector3(range, range, range), transform.rotation, targetLayer);

        Collider closest = null;
        float dist = range + 1;

        foreach (Collider c in colls)
        {
            NetworkPlaneController plane = c.GetComponent<NetworkPlaneController>();
            if (!plane)
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
