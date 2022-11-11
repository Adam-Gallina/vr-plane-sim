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
    [SerializeField] private Transform specialSource;
    [SerializeField] protected GameObject currSpecial;

    [Header("Animations")]
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
            Death();
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

    protected void Fire()
    {
        if (!hasAuthority) return;

        CmdSpawnBullet(nextSource);

        if (++nextSource >= bulletSource.Length)
            nextSource = 0;
    }

    protected void FireSpecial()
    {
        if (!hasAuthority) return;

        CmdSpawnSpecial();
    }

    [Command]
    private void CmdSpawnBullet(int source)
    {
        GameObject b = Instantiate(bulletPrefab, bulletSource[source].position, bulletSource[source].rotation);
        NetworkServer.Spawn(b);
        b.GetComponent<NetworkBullet>().SetSource(this);

        RpcOnSpawnBullet(source);
    }

    [Command]
    private void CmdSpawnSpecial()
    {
        GameObject b = Instantiate(currSpecial, specialSource.position, specialSource.rotation);
        NetworkServer.Spawn(b);
        b.GetComponent<NetworkBullet>().SetSource(this);

    }

    [ClientRpc]
    private void RpcOnSpawnBullet(int source)
    {
        bulletSource[source].GetComponent<AudioSource>()?.Play();
    }

    [ClientRpc]
    protected override void RpcOnDeath()
    {
        GameObject effect = Instantiate(deathPrefab);
        effect.transform.position = transform.position;
        effect.transform.localEulerAngles = transform.localEulerAngles;

        effect.transform.GetChild(0).localEulerAngles = model.localEulerAngles;

        Rigidbody erb = effect.GetComponent<Rigidbody>();
        erb.velocity = rb.velocity;
        erb.angularVelocity = rb.angularVelocity * 10 + transform.right * 5;

        base.RpcOnDeath();
    }
}
