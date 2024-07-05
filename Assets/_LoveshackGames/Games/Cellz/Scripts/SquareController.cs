using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using Steamworks;

public class SquareController : NetworkBehaviour
{
    [SyncVar]
    public string playerID;
    [SyncVar(OnChange = nameof(OnPlayerNameChanged))]
    public string playerName;
    [SyncVar(OnChange = nameof(OnColorChanged))]
    public Color color;

    private float x, y;

    public SquareController instance;

    public float maxSize = 3;
    public float currentSize;
    private float startSize = .2f;
    public float growthRate = .1f;

    void Awake() => instance = this;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            InitClient();
        }
        else
        {
            gameObject.GetComponent<SquareController>().enabled = false;
        }
    }

    private void InitClient()
    {
        string pid = SteamUser.GetSteamID().ToString();
        string pname = SteamFriends.GetPersonaName().ToString();
        Color color = new Color(Random.value, Random.value, Random.value);
        InitClientServer(gameObject, pid, pname, color);

        // Set location to be random -8 to 8 in x and -4 to 4 in y
        x = Random.Range(-8, 8);
        y = Random.Range(-4, 4);
        transform.position = new Vector3(x, y, transform.position.z);


        gameObject.transform.localScale = new Vector3(startSize, startSize, startSize);

        // Now that initialization is complete, call the server RPC
        OnPlayerJoinedServer(playerName);
    }

    [ServerRpc]
    public void InitClientServer(GameObject go, string playerID, string playerName, Color color)
    {
        SquareController ctrl = go.GetComponent<SquareController>();
        ctrl.playerID = playerID;
        ctrl.playerName = playerName;
        ctrl.color = color;
    }


    public void OnColorChanged(Color oldValue, Color newValue, bool asServer)
    {
        gameObject.GetComponent<SpriteRenderer>().color = newValue;
    }

    public void OnPlayerNameChanged(string oldValue, string newValue, bool asServer)
    {
        gameObject.name = newValue;
    }

    [ServerRpc]
    public void OnPlayerJoinedServer(string playerName)
    {
        OnPlayerJoined(playerName);
    }

    [ObserversRpc]
    public void OnPlayerJoined(string playerName)
    {
        Debug.Log(playerName + " joined!");
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if(currentSize < maxSize){
            currentSize += growthRate * Time.deltaTime;
            gameObject.transform.localScale = new Vector3(currentSize, currentSize, currentSize);
        }

        // If arrow keys are pressed, move player location accordingly
        float moveX = 0;
        float moveY = 0;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            moveY = 1;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            moveY = -1;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            moveX = 1;
        }

        // Move the player
        transform.position += new Vector3(moveX, moveY, 0) * Time.deltaTime * 5f; // Adjust the speed multiplier as needed
    }
}
