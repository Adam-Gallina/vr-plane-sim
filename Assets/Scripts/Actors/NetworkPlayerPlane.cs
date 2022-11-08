using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class NetworkPlayerPlane : NetworkPlaneController
{
    [SerializeField] private Transform camTarget;

    [Header("Combat")]
    [SerializeField] private float fireSpeed;
    private float nextShot;
    private bool firing = false;

    private bool allowMovement = false;
    [SerializeField] private float maxThrustSpeed;

    private PlayerControls inp;
    private FixInput fixJoystick = new FixInput();

    protected override void Awake()
    {
        base.Awake();

        inp = new PlayerControls();
    }

    public override void OnStartClient()
    {
        if (hasAuthority)
        {
            CameraController.Instance.SetTarget(camTarget);
        }
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
        if (!hasAuthority)
            return;

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
