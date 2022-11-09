using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkBasicEnemy : NetworkPlaneController
{
    [Header("Death Effect")]
    [SerializeField] private GameObject deathPrefab;

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
