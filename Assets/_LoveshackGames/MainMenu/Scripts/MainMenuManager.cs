using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Steamworks;

public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager instance;

    private bool inLobby;
    private bool lobbyOwner;
    private bool lobbyPublic = true;
    private string lobbyName;
    private CSteamID lobbyID;

    public int selectedGameIndex;
    public List<GameObject> games;

    [SerializeField] private TextMeshProUGUI lobbyButtonText;
    [SerializeField] private TextMeshProUGUI lobbyInfoText;
    [SerializeField] private TextMeshProUGUI playButtonText;
    
    public void ChangeSelectedGameIndex(int delta)
    {
        selectedGameIndex += delta;
        selectedGameIndex += games.Count;
        selectedGameIndex %= games.Count;
    }

    private void Awake(){
        instance = this;
    } 

    private void Start(){
        BootstrapManager.CreateLobby();
        lobbyOwner = true;
    }

    public void OnPlayButtonClicked(){
        BootstrapNetworkManager.StartGame(games[selectedGameIndex]);
    }

    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        instance.inLobby = true;
        instance.lobbyName = lobbyName;
        Debug.Log("lobby entered: "+lobbyName);
        instance.lobbyID = new CSteamID(System.Convert.ToUInt64(BootstrapManager.CurrentLobbyID.ToString()));
        instance.PrintLobbyMembers();

        //BootstrapManager.SpawnClientOnServer(LobbyInfo.GetOwnedClient());
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

    public void StartGame() 
    {
        string[] scenesToClose = new string[] {"MainMenu"};
        BootstrapNetworkManager.ChangeNetworkScene("GameScene", scenesToClose);
    }

    public void LeaveLobby()
    {
        instance.lobbyInfoText.text = "No Lobby";
        BootstrapManager.LeaveLobby();
        BootstrapManager.CreateLobby();
        lobbyOwner = true;
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
        playButtonText.text = "Play "+games[selectedGameIndex].GetComponent<LSGame>().gameTitle;
    }
}
