using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkAvatarSpawner : NetworkBehaviour
{
    public static NetworkAvatarSpawner Instance;

    private void Awake()
    {
        Instance = this;
    }

    

    
}
