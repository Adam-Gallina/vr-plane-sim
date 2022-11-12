using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [SerializeField] private GameObject desktopCamPrefab;
    [SerializeField] private GameObject vrCamPrefab;

    [Header("Camera Movement")]
    [SerializeField] private float speedH = 2.0f;
    [SerializeField] private float speedV = 2.0f;
    [SerializeField] Vector2 camBoundsMin;
    [SerializeField] Vector2 camBoundsMax;
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private List<Transform> targets = new List<Transform>();

    private PlayerControls inp;

    private void Awake()
    {
        Instance = this;

        inp = new PlayerControls();

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

    private void OnEnable()
    {
        inp.Player.AltMouse.Enable();
        inp.Player.CameraPan.Enable();
    }

    private void OnDisable()
    {
        inp.Player.AltMouse.Disable();
        inp.Player.CameraPan.Disable();
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

    private void Update()
    {
        if (inp.Player.AltMouse.IsPressed())
        {
            Vector2 delta = inp.Player.CameraPan.ReadValue<Vector2>();
            yaw += speedH * delta.x;
            pitch -= speedV * delta.y;

            yaw = Mathf.Clamp(yaw, camBoundsMin.x, camBoundsMax.x);
            pitch = Mathf.Clamp(pitch, camBoundsMin.y, camBoundsMax.y);

            Camera.main.transform.localEulerAngles = new Vector3(pitch, yaw, 0.0f);
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
