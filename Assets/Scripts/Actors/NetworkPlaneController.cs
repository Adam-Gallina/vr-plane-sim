using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkPlaneController : NetworkHealthBase
{
    [Header("Movement")]
    [SerializeField] protected float thrustSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float altSpeed;

    [Header("Drift")]
    [SerializeField] protected float driftTurnSpeed;
    [SerializeField] protected float driftAltSpeed;
    [SerializeField] protected float driftGravity;
    private float driftStart;

    [Header("Boost")]
    [SerializeField] protected BoostChargeLevel[] boostChargeLevels;
    private float boostSpeedMod;
    private float boostDuration;

    [Header("Animations")]
    [SerializeField] protected Renderer body;
    public Transform model;
    [SerializeField] private float modelRotMod;
    [SerializeField] private Transform propeller;
    [SerializeField] private float propellerSpeed;
    [SerializeField] private Transform joystick;
    [SerializeField] private float joystickMod;

    [Header("VR UI")]
    public Transform deathMessagePos;
    public Transform bannerPos;
    public Transform healthMeterPos;
    public Transform boostMeterPos;

    [Header("Death Effect")]
    [SerializeField] private GameObject deathPrefab;

    protected Rigidbody rb;

    private void OnValidate()
    {
        for (int i = 1; i < boostChargeLevels.Length; i++)
        {
            if (boostChargeLevels[i].chargeLevel > boostChargeLevels[i - 1].chargeLevel)
            {
                Debug.LogError("Warning: Charge levels must be listed in descending order", gameObject);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody>();
    }

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == Constants.EnvironmentLayer)
        {
            Death(this, DamageSource.Ground);
        }
    }

    #region Movement
    protected void Steer(Vector2 dir, float speed, bool ignoreAuthority=false)
    {
        if (!hasAuthority && !ignoreAuthority)
            return;

        rb.velocity = transform.forward * speed;

        rb.AddTorque((transform.up * dir.x * turnSpeed + transform.right * dir.y * altSpeed) * Time.deltaTime);
        Vector3 ang = transform.localEulerAngles;
        ang.z = 0;
        transform.localEulerAngles = ang;

        if (driftStart != 0)
        {
            driftStart = 0;
            CalcBoostCharge();
        }
    }

    protected void Drift(Vector2 dir, bool ignoreAuthority=false)
    {
        if (!hasAuthority && !ignoreAuthority)
            return;

        rb.AddForce(Vector3.down * driftGravity);

        rb.AddTorque((transform.up * dir.x * driftTurnSpeed + transform.right * dir.y * driftAltSpeed) * Time.deltaTime);
        Vector3 ang = transform.localEulerAngles;
        ang.z = 0;
        transform.localEulerAngles = ang;

        if (driftStart == 0)
            driftStart = Time.time;
    }
    #endregion

    #region Boost
    protected float CalcBoostMod()
    {
        if (boostDuration <= 0)
            return 1;

        boostDuration -= Time.deltaTime;

        return boostSpeedMod;
    }

    private void CalcBoostCharge()
    {
        foreach (BoostChargeLevel l in boostChargeLevels)
        {
            if (Time.time - driftStart >= l.chargeLevel)
            {
                boostSpeedMod = l.boostSpeedMod;
                boostDuration = l.boostDuration;
                return;
            }
        }

        boostSpeedMod = 1;
        boostDuration = 0;
    }
    #endregion

    #region Animations
    protected void UpdateModel(bool ignoreAuthority=false)
    {
        if (!hasAuthority && !ignoreAuthority)
            return;

        float speed = rb.angularVelocity.y;
        model.localEulerAngles = new Vector3(0, 0, -speed * modelRotMod);

        propeller.localEulerAngles += new Vector3(0, 0, propellerSpeed * Time.deltaTime);
    }

    protected void UpdateJoystick(Vector2 dir)
    {
        if (!hasAuthority)
            return;

        joystick.localEulerAngles = new Vector3(dir.y, 0, -dir.x) * joystickMod;
    }
    #endregion

    protected override void OnSetPlayerColor(Color oldCol, Color newCol)
    {
        body.material.color = newCol;
    }

    [ClientRpc]
    protected override void RpcOnDeath(NetworkCombatBase source, DamageSource sourceType)
    {
        GameObject effect = Instantiate(deathPrefab);
        effect.transform.position = transform.position;
        effect.transform.localEulerAngles = transform.localEulerAngles;
        if (body)
            effect.GetComponent<DeathEffect>().body.GetComponent<Renderer>().material.color = body.GetComponent<Renderer>().material.color;

        effect.transform.GetChild(0).localEulerAngles = model.localEulerAngles;

        Rigidbody erb = effect.GetComponent<Rigidbody>();
        erb.velocity = rb.velocity;
        erb.angularVelocity = rb.angularVelocity * 10 + transform.right * 5;

        base.RpcOnDeath(source, sourceType);
    }

    [System.Serializable]
    protected struct BoostChargeLevel
    {
        public float chargeLevel;
        public float boostSpeedMod;
        public float boostDuration;
    }
}
