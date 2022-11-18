using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class NetworkPlayerPlane : NetworkPlaneController
{
    [SerializeField] private Transform camTarget;

    private bool boostPressed = false;

    [Header("Combat")]
    [SerializeField] private float fireSpeed;
    private float nextShot;
    private bool firePressed = false;

    [Header("Special")]
    [SerializeField] private float specialCooldown;
    private float nextSpecial;

    private bool isQuitting = false;

    private PlayerControls inp;
    private FixInput fixJoystick = new FixInput();

    protected override void Awake()
    {
        base.Awake();

        inp = new PlayerControls();
    }

    public override void OnStartClient()
    {
        gameObject.tag = MapController.Instance.pvp ? Constants.EnemyTag : Constants.AllyTag;
        model.gameObject.tag = MapController.Instance.pvp ? Constants.EnemyTag : Constants.AllyTag;
    }

    private void OnEnable()
    {
        inp.Player.Enable();

        inp.Player.Fire.started += OnStartFire;
        inp.Player.Fire.canceled += OnStopFire;

        inp.Player.AltFire.started += OnUseSpecial;

        inp.Player.Right.started += ToggleMovement;
    }

    private void OnDisable()
    {
        inp.Player.Disable();

        inp.Player.Fire.started -= OnStartFire;
        inp.Player.Fire.canceled -= OnStopFire;

        inp.Player.AltFire.started -= OnUseSpecial;

        inp.Player.Right.started -= ToggleMovement;
    }

    private void Update()
    {
        if (!hasAuthority)
            return;

        UpdateUI();

        if (!canControl)
        {
            SetDirection(Vector2.zero, false, 0);
            return;
        }

        if (MapController.Instance.boostRegen && !(boostPressed && CanBoost()))
            RegenBoost();

        HandleInput();

        SetDirection(HandleMovement(), boostPressed && CanBoost(), CalcSpeed());

        UpdateModel();
    }

    private void OnApplicationQuit() => isQuitting = true;
    private void OnDestroy()
    {
        if (hasAuthority && !isQuitting)
        {
            if (CameraController.Instance.IsTarget(transform))
                CameraController.Instance.SetTarget(null);
        }
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

    private float CalcSpeed()
    {
        if (boostPressed && CanBoost())
            return boostSpeed;

        return thrustSpeed;
    }

    private void HandleInput()
    {
        if (GameUI.GInstance.pauseOpen)
            return;

        if (firePressed && Time.time > nextShot)
        {
            nextShot = Time.time + fireSpeed;
            Fire();
        }

        boostPressed = inp.Player.Speed.ReadValue<float>() > 0;
    }

    private void UpdateUI()
    {
        GameUI.GInstance.SetBoostLevel(currBoost / maxBoost);
    }

    #region Input Callbacks
    private void OnStartFire(InputAction.CallbackContext obj)
    {
        if (GameUI.GInstance.pauseOpen)
            return;

        firePressed = true;
    }

    private void OnStopFire(InputAction.CallbackContext obj)
    {
        firePressed = false;
    }

    private void OnUseSpecial(InputAction.CallbackContext obj)
    {
        if (!canControl)
            return;

        if (GameUI.GInstance.pauseOpen)
            return;

        if (currSpecial && Time.time > nextSpecial)
        {
            FireSpecial();
            nextSpecial = Time.time + specialCooldown;
        }
    }

    private void ToggleMovement(InputAction.CallbackContext obj)
    {
        if (GameUI.GInstance.pauseOpen)
            return;

        if (MapController.Instance.DEBUG_toggleControl && hasAuthority)
            CmdSetCanControl(!canControl);
    }
    #endregion

    protected override void Death(NetworkCombatBase source, DamageSource sourceType)
    {
        if (dead)
            return;

        Player?.OnAvatarKilled(source, sourceType);
        source.Player?.OnEnemyKilled(this, sourceType);

        base.Death(source, sourceType);
    }
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