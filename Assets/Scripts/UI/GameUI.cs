using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public abstract class GameUI : MultiCamUI
{
    public static GameUI GInstance;

    [Header("Pause Menu")]
    [SerializeField] protected GameObject pauseMenu;
    [HideInInspector] public bool pauseOpen = false;
    public GameObject returnToLobbyBtn;

    [Header("Death messages")]
    [SerializeField] protected Transform deathMessageParent;
    [SerializeField] protected GameObject deathMessagePrefab;
    [SerializeField] protected float deathMessageHeight;
    [SerializeField] protected float deathMessageDuration;
    protected List<DeathMessageInstance> deathMessageLog = new List<DeathMessageInstance>();

    [Header("Message banner")]
    [SerializeField] protected GameObject banner;
    [SerializeField] protected float defaultBannerDuration;

    [Header("Plane UI")]
    [SerializeField] private Image boostMeter;
    [SerializeField] private Gradient boostMeterColors;

    protected override void Awake()
    {
        base.Awake();

        GInstance = this;

        HideBannerMessage();
    }

    private void OnEnable()
    {
        PlaneSimNetworkManager.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        PlaneSimNetworkManager.OnClientDisconnected -= HandleClientDisconnected;
    }

    private void HandleClientDisconnected()
    {
        if (SceneManager.GetActiveScene().buildIndex != Constants.MainMenu.buildIndex)
            SceneManager.LoadScene(Constants.MainMenu.buildIndex);
    }

    protected virtual void Update()
    {
        if (deathMessageLog.Count > 0 && Time.time >= deathMessageLog[0].endTime)
        {
            IncrementDeathMessages();
        }
    }

    #region Plane UI
    public void SetBoostLevel(float chargeAmount)
    {
        boostMeter.color = boostMeterColors.Evaluate(chargeAmount);
        boostMeter.fillAmount = chargeAmount;
    }
    #endregion

    public void TogglePauseMenu(InputAction.CallbackContext context)
    {
        TogglePauseMenu();
    }
    public void TogglePauseMenu()
    {
        SetPauseMenu(!pauseOpen);
    }

    public abstract void SetPauseMenu(bool open);

    public virtual void SpawnDeathMessage(string msg)
    {
        GameObject dm = Instantiate(deathMessagePrefab, deathMessageParent);

        dm.GetComponentInChildren<Text>().text = msg;

        dm.GetComponent<RectTransform>().localPosition = new Vector3(0, -deathMessageParent.childCount * deathMessageHeight, 0);

        deathMessageLog.Add(new DeathMessageInstance(dm, Time.time + deathMessageDuration));
    }

    public void IncrementDeathMessages()
    {
        Destroy(deathMessageLog[0].rt.gameObject);
        deathMessageLog.RemoveAt(0);

        for (int i = 0; i < deathMessageLog.Count; i++)
        {
            deathMessageLog[i].rt.localPosition = new Vector3(0, -i * deathMessageHeight, 0);
        }
    }

    #region Banner Messages
    public void SetBannerMessage(string msg, float duration=0)
    {
        if (string.IsNullOrEmpty(msg))
        {
            HideBannerMessage();
            return;
        }

        ShowBannerMessage(msg);

        if (duration != -1)
            Invoke(nameof(HideBannerMessage), duration == 0 ? defaultBannerDuration : duration);
    }

    public void ShowBannerMessage(string msg)
    {
        banner.SetActive(true);

        foreach (Text t in banner.GetComponentsInChildren<Text>())
            t.text = msg;
    }

    public void HideBannerMessage()
    {
        banner.SetActive(false);
    }
    #endregion

    protected struct DeathMessageInstance
    {
        public RectTransform rt;
        public float endTime;

        public DeathMessageInstance(GameObject obj, float endTime)
        {
            rt = obj.GetComponent<RectTransform>();
            this.endTime = endTime;
        }
    }

    #region Pause Menu Buttons
    public void LeaveLobbyPressed()
    {
        switch (PlaneSimNetworkManager.Instance.mode)
        {
            case Mirror.NetworkManagerMode.ClientOnly:
                PlaneSimNetworkManager.Instance.StopClient();
                break;
            case Mirror.NetworkManagerMode.Host:
                PlaneSimNetworkManager.Instance.StopHost();
                break;
            default:
                Debug.LogError("Idk what happened but probably ur trying to make a server now so that's pretty cool");
                break;
        }
    }

    public void ReturnToLobbyPressed()
    {
        PlaneSimNetworkManager.Instance.ReturnToLobby();
    }
    #endregion
}
