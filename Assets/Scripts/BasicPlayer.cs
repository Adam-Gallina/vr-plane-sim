using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BasicPlayer : NetworkBehaviour
{
    [SerializeField] private Transform camCenter;
    [SerializeField] private float speed = 10;

    private PlayerControls inp;

    private void Awake()
    {
        inp = new PlayerControls();
    }

    private void Start()
    {
        if (hasAuthority)
        {
            Camera.main.transform.SetParent(camCenter, false);
        }
    }

    private void OnEnable()
    {
        inp.Player.Enable();
    }

    private void OnDisable()
    {
        inp.Player.Disable();
    }

    private void Update()
    {
        if (!hasAuthority)
            return;

        Vector2 dir = inp.Player.Movement.ReadValue<Vector2>();

        transform.Translate(new Vector3(dir.x, 0, dir.y) * Time.deltaTime * speed);
    }
}
