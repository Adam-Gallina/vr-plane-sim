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

    [Header("Combat")]
    [SerializeField] private Transform[] bulletSource;
    private int nextSource;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Animations")]
    [SerializeField] protected Transform model;
    [SerializeField] private float modelRotMod;
    [SerializeField] private Transform propeller;
    [SerializeField] private float propellerSpeed;
    [SerializeField] private Transform joystick;
    [SerializeField] private float joystickMod;


    protected Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody>();
    }

    protected void SetDirection(Vector2 dir, float speed = -1)
    {
        if (!hasAuthority)
            return;

        Steer(dir, speed == -1 ? thrustSpeed : speed);
        UpdateJoystick(dir);
        UpdateModel();
    }

    protected void Steer(Vector2 dir, float speed)
    {
        if (!hasAuthority)
            return;

        rb.velocity = transform.forward * speed;

        rb.AddTorque((transform.up * dir.x * turnSpeed + transform.right * dir.y * altSpeed) * Time.deltaTime);
        Vector3 ang = transform.localEulerAngles;
        ang.z = 0;
        transform.localEulerAngles = ang;

    }

    protected void UpdateModel()
    {
        if (!hasAuthority)
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

    protected void Fire()
    {
        if (!hasAuthority)
            return;

        CmdSpawnBullet(nextSource);

        if (++nextSource >= bulletSource.Length)
            nextSource = 0;
    }

    [Command]
    private void CmdSpawnBullet(int source)
    {
        GameObject b = Instantiate(bulletPrefab, bulletSource[source].position, bulletSource[source].rotation);
        NetworkServer.Spawn(b);

        RpcOnSpawnBullet(source);
    }

    [ClientRpc]
    private void RpcOnSpawnBullet(int source)
    {
        bulletSource[source].GetComponent<AudioSource>()?.Play();
    }
}
