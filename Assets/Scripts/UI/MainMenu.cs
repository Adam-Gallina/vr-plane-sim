using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject startPagePanel;
    [SerializeField] private GameObject lobbyPanel;

    [SerializeField] private InputField nameInputField;
    [SerializeField] private InputField ipAddressField;

    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    public static string DisplayName { get; private set; }

    private void Start()
    {
        startPagePanel.SetActive(true);
        lobbyPanel.SetActive(false);
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
        hostButton.interactable = !string.IsNullOrEmpty(name);
        joinButton.interactable = !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(ipAddressField.text);
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
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;

        UpdateButtons();

        startPagePanel.SetActive(true);
        lobbyPanel.SetActive(false);
    }
    #endregion
}
