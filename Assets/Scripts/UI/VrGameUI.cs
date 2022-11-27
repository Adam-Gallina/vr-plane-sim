using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VrGameUI : GameUI
{
    [Header("VR UI")]
    [SerializeField] private Transform healthMeterBkgd;
    [SerializeField] private Transform boostMeterBkgd;
    [SerializeField] private Vector3 boostMeterOffset;
    [SerializeField] private float boostMeterRotation;

    private void OnValidate()
    {
        MovePlaneUI();
    }

    protected override void Update()
    {
        base.Update();

        MovePlaneUI();
    }

    private void MovePlaneUI()
    {
        NetworkPlaneController p = Camera.main?.GetComponentInParent<NetworkPlaneController>();

        deathMessageParent.gameObject.SetActive(p);
        healthMeterBkgd.gameObject.SetActive(p);
        boostMeterBkgd.gameObject.SetActive(p);

        if (p)
        {
            deathMessageParent.position = p.deathMessagePos.position;
            deathMessageParent.rotation = p.deathMessagePos.rotation;

            banner.transform.position = p.bannerPos.position;
            banner.transform.LookAt(Camera.main.transform.position);

            healthMeterBkgd.position = p.healthMeterPos.position;
            healthMeterBkgd.rotation = p.healthMeterPos.rotation;

            boostMeterBkgd.position = p.boostMeterPos.position;
            boostMeterBkgd.rotation = p.boostMeterPos.rotation;
        }
    }

    public override void SpawnDeathMessage(string msg)
    {
        GameObject dm = Instantiate(deathMessagePrefab, deathMessageParent);

        dm.GetComponentInChildren<TextMeshProUGUI>().text = msg;

        dm.GetComponent<RectTransform>().localPosition = new Vector3(0, -deathMessageParent.childCount * deathMessageHeight, 0);

        deathMessageLog.Add(new DeathMessageInstance(dm, Time.time + deathMessageDuration));
    }

    public override void SetPauseMenu(bool open)
    {
        //throw new System.NotImplementedException();
    }
}
