using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Steamworks;

public class LobbyInfo : MonoBehaviour
{
    public static LobbyInfo instance;
    public List<string> members = new List<string>();
    public List<GameObject> clients;
    public bool isOwner;

    void Awake() => instance = this;

    public static void SetMembers(List<string> members){
        instance.members = members;
    }

    public static List<string> GetMembers()
    {
        return instance.members;
    }

    public static bool IsPlayerInLobby(){
        return instance.members.Count != 0;
    }

    public static void UpdateLobbyInfo(ulong CurrentLobbyID){
        List<string> memberNames = new List<string>();
        int memberCount = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(CurrentLobbyID));

        for (int i = 0; i < memberCount; i++)
        {
            CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(CurrentLobbyID), i);
            string memberName = SteamFriends.GetFriendPersonaName(memberID);
            memberNames.Add(memberName);
        }

        SetMembers(memberNames);
        instance.UpdateClientsList();
    }

    public void UpdateClientsList(){
        LSClient[] allClients = FindObjectsOfType<LSClient>();
        clients = new List<GameObject>();
        foreach(LSClient client in allClients)
        {
            clients.Add(client.gameObject);
        }
    }

    public static GameObject GetOwnedClient(){
        foreach(GameObject client in instance.clients)
        {
            if(client.GetComponent<LSClient>().isOwnedClient)
                return client;
        }
        return null;
    }
}
