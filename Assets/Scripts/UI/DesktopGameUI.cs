using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DesktopGameUI : GameUI
{
    [Header("Desktop UI")]
    [SerializeField] private Transform deathMessages;
    [SerializeField] private Image boostMeter;
    [SerializeField] private Gradient boostMeterColors;

    public override NametagUI SpawnNametag()
    {
        return Instantiate(nametagPrefab, transform).GetComponent<NametagUI>();
    }

    public override void SetBoostLevel(float chargeAmount)
    {
        boostMeter.color = boostMeterColors.Evaluate(chargeAmount);
        boostMeter.fillAmount = chargeAmount;
    }

    public override void SetPauseMenu(bool open)
    {
        pauseOpen = open;
        pauseMenu.SetActive(open);
    }

    public override void SpawnDeathMessage(string msg)
    {
        GameObject dm = Instantiate(deathMessagePrefab, deathMessages);

        dm.GetComponentInChildren<Text>().text = msg;

        dm.GetComponent<RectTransform>().localPosition = new Vector3(0, -deathMessages.childCount * deathMessageHeight, 0);

        deathMessageLog.Add(new DeathMessageInstance(dm, Time.time + deathMessageDuration ));
    }

    public override void IncrementDeathMessages()
    {
        Destroy(deathMessageLog[0].rt.gameObject);
        deathMessageLog.RemoveAt(0);

        for (int i = 0; i < deathMessageLog.Count; i++)
        {
            deathMessageLog[i].rt.localPosition = new Vector3(0, -i * deathMessageHeight, 0);
        }
    }

    public override void ShowBannerMessage(string msg)
    {
        banner.SetActive(true);

        foreach (Text t in banner.GetComponentsInChildren<Text>())
            t.text = msg;
    }

    public override void HideBannerMessage()
    {
        banner.SetActive(false);
    }
}
