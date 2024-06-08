using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;

public class BootstrapNetworkManager : NetworkBehaviour
{
    private static BootstrapNetworkManager instance;

    private void Awake() => instance = this;

    [ServerRpc(RequireOwnership = false)]
    public void StartGameServer(GameObject lsGameObject)
    {
        if(game.sceneNameToLoadOnOpen != "")
        {
            string[] scenesToClose = new string[] {"MainMenu"};
            BootstrapNetworkManager.ChangeNetworkScene(game.sceneNameToLoadOnOpen, scenesToClose);
        }
        StartGameClient(lsGameObject);
    }

    [ObserversRpc]
    void StartGameClient(GameObject lsGameObject)
    {
        LSGame game = lsGameObject.GetComponent<LSGame>();
        game.Open();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeNetworkSceneHelper(string sceneName, string[] scenesToClose)
    {
        Debug.Log("attempting to load "+sceneName);
        instance.CloseScenes(scenesToClose);

        SceneLoadData sld = new SceneLoadData(sceneName);
        ICollection<NetworkConnection> conns = instance.ServerManager.Clients.Values;
        NetworkConnection[] array = new NetworkConnection[conns.Count];
        conns.CopyTo(array, 0);
        instance.SceneManager.LoadConnectionScenes(array, sld);
    }

    public static void ChangeNetworkScene(string sceneName, string[] scenesToClose)
    {
        instance.ChangeNetworkSceneHelper(sceneName, scenesToClose);
    }

    [ObserversRpc]
    void CloseScenes(string[] scenesToClose)
    {
        foreach (var sceneName in scenesToClose)
        {
            Debug.Log("attempting to close " + sceneName);
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}
