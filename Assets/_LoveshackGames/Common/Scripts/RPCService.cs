using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;

public class RPCService : NetworkBehaviour
{

    public static RPCService instance;

    public override void OnStartClient(){
        instance = this;
        base.OnStartClient();
        if (base.IsOwner)
        {

        }else{
            GetComponent<RPCService>().enabled = false;
        }
    }

    public static void DispatchEventToAllClients(string str)
    {
        instance.DispatchEventToAllClientsHelper(str);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DispatchEventToAllClientsHelper(string str)
    {
        OnEvent(str);
    }

    [ObserversRpc]
    public void OnEvent(string str)
    {
        Debug.Log("event recieved: "+str);
    } 
}
