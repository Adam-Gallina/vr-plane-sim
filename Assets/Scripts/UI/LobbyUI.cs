using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance;

    [SerializeField] private Transform playerList;
    private List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>();
    private List<Image> playerColorTexts = new List<Image>();
    private List<Text> playerNameTexts = new List<Text>();
    private List<Text> playerReadyTexts = new List<Text>();
    public Dropdown camType;
    public Button startGameButton;

    [SerializeField] private GameObject planeBody;
    [HideInInspector] public Color planeColor;

    public Dropdown gameMode;

    private void Awake()
    {
        Instance = this;

        startGameButton.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void AddPlayer(LobbyPlayer p)
    {
        p.GetComponent<RectTransform>().SetParent(playerList, false);
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
        for (int i = 0; i < playerNameTexts.Count; i++)
        {
            playerNameTexts[i].text = "Waiting...";
            playerReadyTexts[i].text = string.Empty;
        }

        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            lobbyPlayers[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10 - (i * 60));
            lobbyPlayers[i].UpdateUI();
        }
    }

    public void PressReady()
    {
        foreach (LobbyPlayer p in lobbyPlayers)
        {
            if (p.hasAuthority)
            {
                p.CmdReadyUp();
                break;
            }
        }
    }

    public void PressStart()
    {
        foreach (LobbyPlayer p in lobbyPlayers)
        {
            if (p.hasAuthority)
            {
                p.CmdStartGame(gameMode.value);
                break;
            }
        }
    }

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
    }

    public void OnColorChange(Color col)
    {
        planeColor = col;
        planeBody.GetComponent<Renderer>().material.color = col;
    }

    public void SelectColor(Color col)
    {
        OnColorChange(col);

        foreach (LobbyPlayer p in lobbyPlayers)
        {
            if (p.hasAuthority)
            {
                p.CmdSetPlaneColor(col);
                break;
            }
        }
    }
}
