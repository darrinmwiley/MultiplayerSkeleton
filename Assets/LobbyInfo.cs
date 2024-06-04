using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyInfo : MonoBehaviour
{
    public static LobbyInfo instance;
    public List<string> members;

    public void Awake() => instance = this;

    public static void SetMembers(List<string> m)
    {
        instance.members = m;
    }
}
