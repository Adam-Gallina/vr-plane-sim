using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : HealthBase
{
    [Header("Movement")]
    [SerializeField] protected float thrustSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float altSpeed;

    [Header("Combat")]
    [SerializeField] private Transform[] bulletSource;
    private int nextSource;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float minFireCorrectionDist;
    [SerializeField] private Transform crossHair;

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

    protected void SetDirection(Vector2 dir, float speed=-1)
    {
        Steer(dir, speed == -1 ? thrustSpeed : speed);
        UpdateJoystick(dir);
        UpdateModel();
    }

    protected void Steer(Vector2 dir, float speed)
    {
        rb.velocity = transform.forward * speed;

        rb.AddTorque((transform.up * dir.x * turnSpeed + transform.right * dir.y * altSpeed) * Time.deltaTime);
        Vector3 ang = transform.localEulerAngles;
        ang.z = 0;
        transform.localEulerAngles = ang;

    }

    protected void UpdateModel()
    {
        float speed = rb.angularVelocity.y;
        model.localEulerAngles = new Vector3(0, 0, -speed * modelRotMod);

        propeller.localEulerAngles += new Vector3(0, 0, propellerSpeed * Time.deltaTime);
    }

    protected void UpdateJoystick(Vector2 dir)
    {
        joystick.localEulerAngles = new Vector3(dir.y, 0, -dir.x) * joystickMod;
    }

    protected void Fire()
    {
        bulletSource[nextSource].GetComponent<AudioSource>()?.Play();

        GameObject b = Instantiate(bulletPrefab);
        b.transform.position = bulletSource[nextSource].position;
        Vector3 dir = bulletSource[nextSource].forward;

        /*if (Physics.Raycast(cam.position, crossHair.position - cam.position, out RaycastHit hit, Camera.main.farClipPlane, 1 << Constants.EnvironmentLayer))
        {
            if (Vector3.Distance(transform.position, hit.point) > minFireCorrectionDist)
            {
                dir = (hit.point - b.transform.position).normalized;
            }
        }*/

        b.transform.forward = dir;
        b.GetComponent<Rigidbody>().velocity = dir * (thrustSpeed + bulletSpeed);

        if (++nextSource >= bulletSource.Length)
            nextSource = 0;
    }
}
