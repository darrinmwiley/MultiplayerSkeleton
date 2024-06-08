using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet.Managing;
using FishNet.Connection;
using FishNet.Object;
using Steamworks;

public class BootstrapManager : MonoBehaviour
{
    private static BootstrapManager instance;
    
    private void Awake() => instance = this;

    [SerializeField] private string menuName = "MainMenu";
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks _fishySteamworks;

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;
    protected Callback<LobbyChatUpdate_t> LobbyChatUpdate;

    public static ulong CurrentLobbyID;

    public static void CreateLobby(){
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
    }

    private void Start()
    {
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(menuName, LoadSceneMode.Additive);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("starting lobby creation" + callback.m_eResult.ToString());
        if(callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.Log("lobby creation failed");
            return;
        }
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "name", SteamFriends.GetPersonaName().ToString()+"'s lobby");
        _fishySteamworks.SetClientAddress(SteamUser.GetSteamID().ToString());
        _fishySteamworks.StartConnection(/*server = */ true);
        Debug.Log("Lobby creation was successful");
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        CurrentLobbyID = callback.m_ulSteamIDLobby;

        MainMenuManager.LobbyEntered(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name"), _networkManager.IsServer);

        _fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress"));

        _fishySteamworks.StartConnection(/*server = */false);

        LobbyInfo.UpdateLobbyInfo(CurrentLobbyID);
    }

    public static void SpawnClientOnServer(GameObject clientObj)
    {
        //Debug.Log("request recieved to spawn "+clientObj.name+" on server");
        instance._networkManager.ServerManager.Spawn(clientObj);
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        LobbyInfo.UpdateLobbyInfo(CurrentLobbyID);
    }

    public static void JoinByID(CSteamID steamID)
    {
        Debug.Log("attempting to join lobby id "+steamID.m_SteamID);
        if(SteamMatchmaking.RequestLobbyData(steamID))
            SteamMatchmaking.JoinLobby(steamID);
        else{
            Debug.Log("failed to join lobby id "+steamID.m_SteamID);
        }
    }

    //not currently called, but we don't need very rich lobby settings. can be scoped out later. For now, it's just friends can join
    public static void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
        CurrentLobbyID = 0;
        instance._fishySteamworks.StopConnection(/*server =*/ false);
        if(instance._networkManager.IsServer)
            instance._fishySteamworks.StopConnection(/*server = */ true);
        LobbyInfo.UpdateLobbyInfo(CurrentLobbyID);
    }
}
