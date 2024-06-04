using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Steamworks;

public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager instance;

    private bool inLobby;
    private string lobbyName;
    private CSteamID lobbyID;

    [SerializeField] private TextMeshProUGUI lobbyButtonText;
    [SerializeField] private TextMeshProUGUI lobbyInfoText;
    [SerializeField] private TextMeshProUGUI playButtonText;
    

    private void Awake(){
        instance = this;
    } 

    //todo, there is a "server already running exception if you create, leave, create again"
    public void CreateLobby()
    {
        Debug.Log("invoking createLobby");
        if(inLobby){
            LeaveLobby();
            inLobby = false;
        }else{
            BootstrapManager.CreateLobby();
            inLobby = true;
        }
    }

    /*public void OpenMainMenu()
    {
        menuScreen.SetActive(true);
    }*/

    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        instance.lobbyName = lobbyName;
        instance.lobbyButtonText.text = "Leave Lobby";
        Debug.Log("lobby entered: "+lobbyName);
        instance.lobbyID = new CSteamID(System.Convert.ToUInt64(BootstrapManager.CurrentLobbyID.ToString()));
        instance.PrintLobbyMembers();
        //instance.lobbyIDText.text = BootstrapManager.CurrentLobbyID.ToString();
    }

    public void PrintLobbyMembers()
    {
        Debug.Log(lobbyID);
        int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
        Debug.Log("member num: "+memberCount);
        List<string> memberNames = new List<string>();

        for (int i = 0; i < memberCount; i++)
        {
            CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
            string memberName = SteamFriends.GetFriendPersonaName(memberID);
            memberNames.Add(memberName);
        }

        foreach (var name in memberNames)
        {
            Debug.Log("Lobby member: " + name);
        }
    }

    /*
    this was for the old join by lobby ID button
    public void JoinLobby()
    {
        lobbyID = new CSteamID(System.Convert.ToUInt64(BootstrapManager.CurrentLobbyID.ToString()));
        PrintLobbyMembers();
        BootstrapManager.JoinByID(lobbyID);
    }*/

    public void StartGame() 
    {
        string[] scenesToClose = new string[] {"MainMenu"};
        BootstrapNetworkManager.ChangeNetworkScene("GameScene", scenesToClose);
    }

    public void LeaveLobby()
    {
        instance.lobbyInfoText.text = "No Lobby";
        instance.lobbyButtonText.text = "Create Lobby";
        BootstrapManager.LeaveLobby();
    }

    public void UpdateLobbyInfo(){
        string lobbyText = lobbyName+"\n\n";
        foreach(string member in LobbyInfo.GetMembers())
        {
            lobbyText += member+"\n";
        }
        lobbyInfoText.text = lobbyText;
    }

    void Update()
    {
        if(inLobby)
        {
            UpdateLobbyInfo();
        }
    }
}
