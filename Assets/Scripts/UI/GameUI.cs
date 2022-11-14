using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] protected GameObject deathMessagePrefab;
    [SerializeField] protected float deathMessageHeight;
    [SerializeField] protected float deathMessageDuration;
    protected List<DeathMessageInstance> deathMessageLog = new List<DeathMessageInstance>();

    [Header("Message banner")]
    [SerializeField] protected GameObject banner;
    [SerializeField] protected float defaultBannerDuration;

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

    private void Update()
    {
        if (deathMessageLog.Count > 0 && Time.time >= deathMessageLog[0].endTime)
        {
            IncrementDeathMessages();
        }
    }

    public void TogglePauseMenu(InputAction.CallbackContext context)
    {
        TogglePauseMenu();
    }
    public void TogglePauseMenu()
    {
        SetPauseMenu(!pauseOpen);
    }
    public abstract void SetPauseMenu(bool open);

    public abstract void SpawnDeathMessage(string msg);
    public abstract void IncrementDeathMessages();

    public void SetBannerMessage(string msg, float duration=0)
    {
        ShowBannerMessage(msg);

        Invoke(nameof(HideBannerMessage), duration == 0 ? defaultBannerDuration : duration);
    }

    public abstract void ShowBannerMessage(string msg);
    public abstract void HideBannerMessage();

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

    private void HandleClientDisconnected()
    {
        if (SceneManager.GetActiveScene().buildIndex != Constants.MainMenu.buildIndex)
            SceneManager.LoadScene(Constants.MainMenu.buildIndex);
    }
}
