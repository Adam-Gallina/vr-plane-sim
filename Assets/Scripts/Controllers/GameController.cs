using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex == Constants.GameScene && !PlaneSimNetworkManager.Instance)
            SceneManager.LoadScene(Constants.MenuScene);
    }

    void Start()
    {
        Physics.IgnoreLayerCollision(Constants.PlayerLayer, Constants.EnvironmentLayer);
    }
}
