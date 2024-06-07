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
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}
