using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using Steamworks;

public class LSGame : NetworkBehaviour{ 

    //todo: game open also needs to be done across the network? unless we just let the scene handle the rest . lets try that first

    public string gameTitle;
    public string sceneNameToLoadOnOpen;

    public void Open(){
        Debug.Log("Opening "+gameTitle);
    }

}
