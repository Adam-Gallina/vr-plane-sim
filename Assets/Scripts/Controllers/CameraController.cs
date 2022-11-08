using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SetTarget(Transform target)
    {
        transform.SetParent(target, false);

        Debug.Log("Watching " + target);
    }
}
