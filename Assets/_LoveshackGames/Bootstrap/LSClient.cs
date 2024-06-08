using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using Steamworks;

public class LSClient : NetworkBehaviour
{
    public string playerName;

    public void Start()
    {
        playerName = SteamFriends.GetPersonaName().ToString();
        gameObject.name = "Client("+playerName+")";
    }
}
