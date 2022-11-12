using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchPlayerDeath : MonoBehaviour
{
    [SerializeField] private float startOffset;
    [SerializeField] private float offset;
    public float duration;

    [SerializeField] private Transform camTarget;

    public void SetPosition(Vector3 lookPos, Vector3 dir)
    {
        transform.forward = dir;

        CameraController.Instance.PushTarget(camTarget);

        StartCoroutine(Anim(lookPos, dir));
    }

    private void EndWatchParty()
    {
        if (CameraController.Instance.IsTarget(transform))
            CameraController.Instance.PopTarget();
        Destroy(gameObject);
    }

    private IEnumerator Anim(Vector3 pos, Vector3 dir)
    {
        transform.position = pos + dir * -startOffset;

        float start = Time.time;
        float end = Time.time + duration;

        while (Time.time < end)
        {
            float t = (Time.time - start) / duration;
            transform.position = pos - dir * (startOffset + (offset - startOffset) * t);
            yield return new WaitForEndOfFrame();
        }

        EndWatchParty();
    }
}
