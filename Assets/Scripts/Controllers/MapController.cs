using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SpawnFormation { RoundRobin, Assigned, Random }
public class MapController : MonoBehaviour
{
    public static MapController Instance;

    public GameController gameControllerPrefab;

    [SerializeField] private Transform[] spawnPositions;

    [SerializeField] private GameObject desktopUI;
    [SerializeField] private GameObject vrUI;
    private GameObject ui;

    [Header("Debug")]
    public bool DEBUG_toggleControl;

    [Header("Map Rules")]
    //public List<AvatarBase> availableAvatars;
    public bool pvp = true;
    public SpawnFormation spawnFormat = SpawnFormation.Assigned;
    public bool allowAiRespawn = true;
    public bool allowPlayerRespawn = true;
    public bool startMaxBoost = true;
    public bool boostRegen = true;

    private void OnDrawGizmos()
    {
        if (spawnPositions.Length == 0)
            return;

        foreach (Transform t in spawnPositions)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(t.position, 1);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(t.position, t.position + t.forward * 2);
        }
    }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex != Constants.MainMenu.buildIndex && !PlaneSimNetworkManager.Instance)
            SceneManager.LoadScene(Constants.MainMenu.buildIndex);

        Instance = this;
    }

    public void SpawnUI()
    {
        if (ui)
            return;

        Constants.CamType camType = Constants.CamType.Desktop;

        foreach (NetworkGamePlayer p in PlaneSimNetworkManager.Instance.Players)
        {
            if (p.hasAuthority)
            {
                camType = p.CamType;
                break;
            }
        }

        if (!desktopUI || !vrUI)
            return;

        switch (camType)
        {
            case Constants.CamType.Desktop:
                ui = Instantiate(desktopUI);
                break;
            case Constants.CamType.VR:
                ui = Instantiate(vrUI);
                break;
        }
    }

    public Transform GetSpawnPos(int player)
    {
        switch (spawnFormat)
        {
            case SpawnFormation.RoundRobin:
                return RoundRobin();
            case SpawnFormation.Assigned:
                return AssignedSpawn(player);
            default:
                Debug.LogWarning($"{spawnFormat} has not been implemented, defaulting to RoundRobin");
                return RoundRobin();
        }
    }

    private int nextPos = 0;
    private Transform RoundRobin()
    {
        Transform t = spawnPositions[nextPos];

        if (nextPos++ >= spawnPositions.Length)
            nextPos = 0;

        return t;
    }

    private Transform AssignedSpawn(int player)
    {
        if (player < 0 || player >= spawnPositions.Length)
        {
            Debug.LogWarning($"Missing spawn point {player}");
            return null;
        }

        return spawnPositions[player];
    }
}
