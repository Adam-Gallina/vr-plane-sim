using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [SerializeField] private GameObject desktopCamPrefab;
    [SerializeField] private GameObject vrCamPrefab;

    private List<Transform> targets = new List<Transform>();

    private void Awake()
    {
        Instance = this;

        if (PlaneSimNetworkManager.Instance)
        {
            foreach (NetworkGamePlayer p in PlaneSimNetworkManager.Instance.GamePlayers)
            {
                if (p.hasAuthority)
                {
                    SpawnCam(p.CamType);
                }
            }
        }
    }

    public void SpawnCam(Constants.CamType camType)
    {
        switch (camType)
        {
            case Constants.CamType.Desktop:
                Instantiate(desktopCamPrefab, transform);
                break;
            case Constants.CamType.VR:
                Instantiate(vrCamPrefab, transform);
                break;
            default:
                Debug.LogError($"Asked to spawn {camType}, but has not been set up");
                return;
        }
    }

    public void SetTarget(Transform target)
    {
        transform.SetParent(target, false);
    }

    public void PushTarget(Transform target)
    {
        targets.Insert(0, transform.parent);
        SetTarget(target);
    }

    public void PopTarget()
    {
        SetTarget(targets[0]);
        targets.RemoveAt(0);
    }
}