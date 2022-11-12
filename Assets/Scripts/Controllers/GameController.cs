using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [Header("Map Rules")]
    public bool pvp = true;
    public SpawnFormation spawnFormat = SpawnFormation.Assigned;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex != Constants.MainMenu.buildIndex && !PlaneSimNetworkManager.Instance)
            SceneManager.LoadScene(Constants.MainMenu.buildIndex);

        Instance = this;
    }
}
