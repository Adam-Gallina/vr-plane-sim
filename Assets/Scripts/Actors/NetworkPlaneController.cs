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
    [HideInInspector] public bool canMove = false;

    [Header("Animations")]
    [SerializeField] protected Renderer body;
    [SerializeField] protected Transform model;
    [SerializeField] private float modelRotMod;
    [SerializeField] private Transform propeller;
    [SerializeField] private float propellerSpeed;
    [SerializeField] private Transform joystick;
    [SerializeField] private float joystickMod;

    [Header("Death Effect")]
    [SerializeField] private GameObject deathPrefab;

    protected Rigidbody rb;

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

    protected void SetDirection(Vector2 dir, float speed = -1)
    {
        if (!hasAuthority)
            return;

        Steer(dir, speed == -1 ? thrustSpeed : speed);
        UpdateJoystick(dir);
        UpdateModel();
    }

    protected void Steer(Vector2 dir, float speed, bool ignoreAuthority=false)
    {
        if (!hasAuthority && !ignoreAuthority)
            return;

        rb.velocity = transform.forward * speed;

        rb.AddTorque((transform.up * dir.x * turnSpeed + transform.right * dir.y * altSpeed) * Time.deltaTime);
        Vector3 ang = transform.localEulerAngles;
        ang.z = 0;
        transform.localEulerAngles = ang;

    }

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
}
