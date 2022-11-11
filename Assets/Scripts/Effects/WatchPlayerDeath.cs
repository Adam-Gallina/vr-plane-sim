using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchPlayerDeath : MonoBehaviour
{
    [SerializeField] private float offset;
    public float duration;

    [SerializeField] private Transform camTarget;

    public void SetPosition(Vector3 lookPos, Vector3 dir)
    {
        transform.position = lookPos + dir * -offset;
        transform.forward = dir;

        CameraController.Instance.PushTarget(camTarget);

        Invoke("EndWatchParty", duration);
    }

    private void EndWatchParty()
    {
        CameraController.Instance.PopTarget();
        Destroy(gameObject);
    }
}
