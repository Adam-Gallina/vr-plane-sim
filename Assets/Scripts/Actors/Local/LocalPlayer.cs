using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayer : PlaneController
{
    private bool allowMovement = false;

    [Header("Camera")]
    [SerializeField] protected Transform cam;
    [SerializeField] private float speedH = 2.0f;
    [SerializeField] private float speedV = 2.0f;
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    [Header("Combat")]
    [SerializeField] private float fireSpeed;
    private float nextShot;
    private bool firing = false;

    [SerializeField] private float maxThrustSpeed;

    private PlayerControls inp;
    private FixInput fixJoystick = new FixInput();

    protected override void Awake()
    {
        base.Awake();

        inp = new PlayerControls();
    }

    private void OnEnable()
    {
        inp.Player.Enable();

        inp.Player.Fire.started += OnStartFire;
        inp.Player.Fire.canceled += OnStopFire;

        inp.Player.Middle.started += ToggleMovement;

        //pc.Player.Right.started += Press4;
    }

    private void OnDisable()
    {
        inp.Player.Disable();

        inp.Player.Fire.started -= OnStartFire;

        inp.Player.Fire.canceled -= OnStopFire;
        inp.Player.Middle.started -= ToggleMovement;

        //pc.Player.Right.started -= Press4;
    }

    private void Update()
    {
        float speedMod = (-inp.Player.Speed.ReadValue<float>() + 1) / 2; // (1, -1) -> (0, 1)
        float s = thrustSpeed + (maxThrustSpeed - thrustSpeed) * speedMod;

        if (allowMovement)
            SetDirection(HandleMovement(), s);
        else
            Steer(Vector2.zero, 0);

        HandleInput();

        UpdateModel();

        //if (inp.altFire)
        //    UpdateCamera();
    }

    private Vector2 HandleMovement()
    {
        Vector2 dir = inp.Player.Movement.ReadValue<Vector2>();

        string currControl = inp.Player.Movement.activeControl != null ? inp.Player.Movement.activeControl.name : "none";
        if (currControl == "stick")
        {
            dir = fixJoystick.GetCorrectedVec2(dir);
        }

        return dir;
    }

    private void HandleInput()
    {
        if (firing && Time.time > nextShot)
        {
            nextShot = Time.time + fireSpeed;
            Fire();
        }
    }

    private void UpdateCamera()
    {
        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");

        if (pitch > 90)
            pitch = 90;
        else if (pitch < -90)
            pitch = -90;

        cam.localEulerAngles = new Vector3(pitch, yaw, 0.0f);
    }

    #region Input Callbacks
    private void OnStartFire(InputAction.CallbackContext obj)
    {
        firing = true;
    }

    private void OnStopFire(InputAction.CallbackContext obj)
    {
        firing = false;
    }

    private void ToggleMovement(InputAction.CallbackContext obj)
    {
        allowMovement = !allowMovement;
    }
    #endregion
}

class FixInput
{
    private float threshold = 0.5f;
    private Vector2 lastDir = new Vector2();

    private float VerifyInput(float value, float lastVal)
    {
        if (value != 0 || Mathf.Abs(lastVal) < threshold)
            return value;

        return Mathf.Sign(lastVal);
    }

    public Vector2 GetCorrectedVec2(Vector2 dir)
    {
        if (dir.x > 0)
            dir.x = 1 - dir.x;
        else if (dir.x < 0)
            dir.x = -1 - dir.x;

        if (dir.y > 0)
            dir.y = 1 - dir.y;
        else if (dir.y < 0)
            dir.y = -1 - dir.y;

        dir = new Vector2(VerifyInput(dir.x, lastDir.x),
                          VerifyInput(dir.y, lastDir.y));

        lastDir = dir;

        return dir;
    }
}