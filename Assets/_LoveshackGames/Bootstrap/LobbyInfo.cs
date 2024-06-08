using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyInfo : MonoBehaviour
{
    public static LobbyInfo instance;
    public List<string> members = new List<string>();
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
}
