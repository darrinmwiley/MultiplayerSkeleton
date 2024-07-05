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

    public static void OpenGame(LSGame game)
    {
        instance.OpenGameServer(game.gameObject);    
    }

    [ServerRpc(RequireOwnership = false)]
    public void OpenGameServer(GameObject gameObj)
    {
        LSGame game = gameObj.GetComponent<LSGame>();
        if(game.sceneNameToLoadOnOpen != "")
        {
            SceneLoadData sld = new SceneLoadData(game.sceneNameToLoadOnOpen);
            ICollection<NetworkConnection> conns = instance.ServerManager.Clients.Values;
            NetworkConnection[] array = new NetworkConnection[conns.Count];
            conns.CopyTo(array, 0);
            instance.SceneManager.LoadConnectionScenes(array, sld);
        }
        OpenGameClient(gameObj);
    }

    [ObserversRpc]
    void OpenGameClient(GameObject gameObj)
    {
        LSGame game = gameObj.GetComponent<LSGame>();
        game.Open();
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
