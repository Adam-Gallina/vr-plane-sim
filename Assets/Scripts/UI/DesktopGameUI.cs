using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DesktopGameUI : GameUI
{
    public override void SetPauseMenu(bool open)
    {
        pauseOpen = open;
        pauseMenu.SetActive(open);
    }
}
