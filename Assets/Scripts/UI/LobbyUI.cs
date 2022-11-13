using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance;

    [SerializeField] private Transform playerList;
    private List<Text> playerNameTexts = new List<Text>();
    private List<Text> playerReadyTexts = new List<Text>();
    public Dropdown camType;
    public Button startGameButton;

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

    public void AddPlayer(NetworkLobbyPlayer p)
    {
        p.GetComponent<RectTransform>().SetParent(playerList, false);
        playerNameTexts.Add(p.playerNameText);
        playerReadyTexts.Add(p.playerReadyText);

        UpdateDisplay();
    }

    public void RemovePlayer(NetworkLobbyPlayer p)
    {
        playerNameTexts.Remove(p.playerNameText);
        playerReadyTexts.Remove(p.playerReadyText);

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        for (int i = 0; i < playerNameTexts.Count; i++)
        {
            playerNameTexts[i].text = "Waiting...";
            playerReadyTexts[i].text = string.Empty;
        }

        for (int i = 0; i < PlaneSimNetworkManager.Instance.LobbyPlayers.Count; i++)
        {
            PlaneSimNetworkManager.Instance.LobbyPlayers[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10 - (i * 60));
            playerNameTexts[i].text = PlaneSimNetworkManager.Instance.LobbyPlayers[i].DisplayName;
            playerReadyTexts[i].text = PlaneSimNetworkManager.Instance.LobbyPlayers[i].IsReady ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
        }
    }

    public void PressReady()
    {
        foreach (NetworkLobbyPlayer p in PlaneSimNetworkManager.Instance.LobbyPlayers)
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
        foreach (NetworkLobbyPlayer p in PlaneSimNetworkManager.Instance.LobbyPlayers)
        {
            if (p.hasAuthority)
            {
                p.CmdStartGame(gameMode.value);
                break;
            }
        }
    }
}
