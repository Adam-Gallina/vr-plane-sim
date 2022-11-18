using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] public GameObject startPagePanel;
    [SerializeField] public GameObject lobbyPanel;

    [SerializeField] private InputField nameInputField;
    [SerializeField] private InputField ipAddressField;

    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    public static string DisplayName { get; private set; }

    private void Start()
    {
        if (PlaneSimNetworkManager.Instance.mode != Mirror.NetworkManagerMode.Offline)
        {
            startPagePanel.SetActive(false);
            lobbyPanel.SetActive(true);

            nameInputField.text = PlayerPrefs.GetString(Constants.PlayerNamePref, "Player");
            if (PlayerPrefs.HasKey(Constants.LastIpPref))
                ipAddressField.text = PlayerPrefs.GetString(Constants.LastIpPref);
        }
        else
        {
            startPagePanel.SetActive(true);
            lobbyPanel.SetActive(false);
            SetPlayerName(nameInputField.text);
        }
    }

    private void OnEnable()
    {
        PlaneSimNetworkManager.OnClientConnected += HandleClientConnected;
        PlaneSimNetworkManager.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        PlaneSimNetworkManager.OnClientConnected -= HandleClientConnected;
        PlaneSimNetworkManager.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void SetPlayerName(string name)
    {
        DisplayName = name;
        UpdateButtons();
    }

    public void UpdateButtons()
    {
        hostButton.interactable = !string.IsNullOrEmpty(DisplayName);
        joinButton.interactable = !string.IsNullOrEmpty(DisplayName) && !string.IsNullOrEmpty(ipAddressField.text);
    }

    #region Buttons
    public void HostLobby()
    {
        PlaneSimNetworkManager.Instance.StartHost();

        startPagePanel.SetActive(false);
    }

    public void JoinLobby()
    {
        string ipAddress = ipAddressField.text;

        PlaneSimNetworkManager.Instance.networkAddress = ipAddress;
        PlaneSimNetworkManager.Instance.StartClient();

        joinButton.interactable = false;
    }
    #endregion

    #region Callbacks
    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        startPagePanel.SetActive(false);
        lobbyPanel.SetActive(true);

        PlayerPrefs.SetString(Constants.LastIpPref, ipAddressField.text);
        PlayerPrefs.SetString(Constants.PlayerNamePref, name);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;

        UpdateButtons();

        startPagePanel.SetActive(true);
        lobbyPanel.SetActive(false);

        LobbyUI.LInstance.ClearPlayers();
    }
    #endregion
}
