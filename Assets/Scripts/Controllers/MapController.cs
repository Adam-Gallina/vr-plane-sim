using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SpawnFormation { RoundRobin, Assigned, Random }
public class MapController : MonoBehaviour
{
    public static MapController Instance;

    public GameController gameControllerPrefab;

    [SerializeField] private Transform[] spawnPositions = new Transform[0];
    [SerializeField] private Transform[] powerupPositions = new Transform[0];
    [SerializeField] private PowerupSource[] availablePowerups = new PowerupSource[0];
    public int TotalPowerupSpawns { get {
            if (powerupPositions == null)
                return 0;
            return powerupPositions.Length; 
        } }

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
    public bool allowPowerupRespawn = true;
    public float powerupRespawnTime = 3;

    private void OnDrawGizmos()
    {
        if (spawnPositions.Length > 0)
        {
            foreach (Transform t in spawnPositions)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(t.position, 1);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(t.position, t.position + t.forward * 2);
            }
        }

        if (powerupPositions.Length > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform t in powerupPositions)
            {
                Gizmos.DrawWireSphere(t.position, 5);
            }
        }
    }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex != Constants.MainMenu.buildIndex && !PlaneSimNetworkManager.Instance)
            SceneManager.LoadScene(Constants.MainMenu.buildIndex);

        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void SpawnUI()
    {
        if (ui)
            return;

        Constants.CamType camType = Constants.CamType.Unknown;

        foreach (NetworkGamePlayer p in PlaneSimNetworkManager.Instance.Players)
        {
            if (p.hasAuthority)
            {
                camType = p.CamType;
                break;
            }
        }

        if (!desktopUI || !vrUI)
        {
            Debug.LogError("No UI set, cannot spawn");
            return;
        }

        switch (camType)
        {
            case Constants.CamType.Unknown:
                break;
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


    public NetworkEnemySpawnPos[] GetMapEnemies()
    {
        return GetComponentsInChildren<NetworkEnemySpawnPos>();
    }

    public Transform GetPowerupPos(int powerup)
    {
        if (powerupPositions.Length < powerup)
        {
            Debug.LogError("Requesting invalid powerup position (" + powerup + ")");
            return null;
        }

        return powerupPositions[powerup];
    }

    public PowerupSource GetRandomPowerup()
    {
        return availablePowerups[Random.Range(0, availablePowerups.Length)];
    }
}
