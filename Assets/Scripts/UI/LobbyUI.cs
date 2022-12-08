using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MultiCamUI
{
    public static LobbyUI LInstance;

    private List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>();

    [Header("UI")]
    [SerializeField] private Transform playerParent;
    public Dropdown camType;
    public Button readyButton;
    public Dropdown gameMode;
    public Button startGameButton;

    [Header("Plane Preview")]
    public FlexibleColorPicker colorSelect;
    [SerializeField] private GameObject planeBody;

    protected override void Awake()
    {
        base.Awake();

        LInstance = this;

        startGameButton.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public override NametagUI SpawnNametag()
    {
        return Instantiate(nametagPrefab, playerParent).GetComponent<NametagUI>();
    }

    public void AddPlayer(LobbyPlayer p)
    {
        lobbyPlayers.Add(p);

        UpdateDisplay();
    }

    public void RemovePlayer(LobbyPlayer p)
    {
        lobbyPlayers.Remove(p);

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            lobbyPlayers[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10 - (i * 60));
            lobbyPlayers[i].UpdateUI();
        }
    }

    public void ClearPlayers()
    {
        foreach (LobbyPlayer p in lobbyPlayers)
            Destroy(p.gameObject);
    }

    #region UICallbacks
    public void PressLeave()
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

        ClearPlayers();
    }

    public void OnColorChange(Color col)
    {
        planeBody.GetComponent<Renderer>().material.color = col;
    }
    #endregion
}
