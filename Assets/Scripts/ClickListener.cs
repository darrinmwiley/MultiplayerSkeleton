using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet.Managing;
using Steamworks;

public class ClickListener : MonoBehaviour
{
    public void OnClick(){
        string alias = SteamFriends.GetPersonaName().ToString();
        RPCService.DispatchEventToAllClients("button clicked by "+alias);
    }
}
