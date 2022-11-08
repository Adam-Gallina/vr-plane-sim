using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BasicPlayer : NetworkHealthBase
{
    [SerializeField] private Transform camCenter;
    [SerializeField] private float speed = 10;

    private PlayerControls inp;

    protected override void Awake()
    {
        base.Awake();

        inp = new PlayerControls();
    }

    public override void OnStartClient()
    {
        Debug.Log("Client Start, " + hasAuthority);
    }

    private void Start()
    {
        Debug.Log("Start");
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
