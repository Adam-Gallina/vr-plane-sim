using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy2D : MonoBehaviour
{
    public float thrustSpeed = 30;
    public float turnSpeed = 60;
    public float altSpeed = 60;

    public float turnSteerMod = 10;
    public float altSteerMod = 10;

    private Rigidbody rb;

    private void Awake()
    {
        rb = transform.GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector3 targetPos = GameObject.Find("Target").transform.position;

        float Dir(float ang, float mod)
        {
            if (Mathf.Abs(ang) < 90)
                return Mathf.Clamp(ang, -1, 1);
            else
                return Mathf.Clamp(ang, -mod, mod) / mod;
        }

        float dirX = -Vector3.SignedAngle(transform.forward, transform.position - targetPos, transform.up);
        float dirY = -Vector3.SignedAngle(transform.forward, transform.position - targetPos, transform.right);

        Vector2 dir = new Vector2(Dir(dirX, turnSteerMod), Dir(dirY, altSteerMod));

        Steer(dir, thrustSpeed);
    }

    protected void Steer(Vector2 dir, float speed)
    {
        rb.velocity = transform.forward * speed;

        rb.AddTorque((transform.up * dir.x * turnSpeed + transform.right * dir.y * altSpeed) * Time.deltaTime);
        Vector3 ang = transform.localEulerAngles;
        ang.z = 0;
        transform.localEulerAngles = ang;

    }
}
