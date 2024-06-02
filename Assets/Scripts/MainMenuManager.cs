using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Steamworks;

public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager instance;

    [SerializeField] private GameObject menuScreen, lobbyScreen;
    [SerializeField] private TMP_InputField lobbyInput;

    [SerializeField] private TextMeshProUGUI lobbyIDText, lobbyNameText;
    [SerializeField] private Button startGameButton;

    private void Awake(){
        instance = this;
    } 

    public void CreateLobby()
    {
        BootstrapManager.CreateLobby();
    }

    public void OpenMainMenu()
    {
        CloseAllScreens();
        menuScreen.SetActive(true);
    }

    public void OpenLobby()
    {
        CloseAllScreens();
        lobbyScreen.SetActive(true);
    }

    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        instance.startGameButton.gameObject.SetActive(isHost);
        instance.lobbyNameText.text = lobbyName;
        instance.lobbyIDText.text = BootstrapManager.CurrentLobbyID.ToString();
        instance.OpenLobby();
    }

    void CloseAllScreens()
    {
        menuScreen.SetActive(false);
        lobbyScreen.SetActive(false);
    }

    public void JoinLobby()
    {
        CSteamID steamID = new CSteamID(System.Convert.ToUInt64(lobbyInput.text));
        BootstrapManager.JoinByID(steamID);
    }

    public void StartGame() 
    {
        string[] scenesToClose = new string[] {"MainMenu"};
        BootstrapNetworkManager.ChangeNetworkScene("GameScene", scenesToClose);
    }

    public void LeaveLobby()
    {
        BootstrapManager.LeaveLobby();
    }
}
