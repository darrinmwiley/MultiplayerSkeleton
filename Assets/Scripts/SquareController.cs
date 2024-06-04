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
    [SyncVar]
    public string playerName;
    [SyncVar(OnChange = nameof(OnColorChanged))]
    public Color color;

    private float x, y;

    public SquareController instance;

    void Awake() => instance = this;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if(base.IsOwner)
        {
        }else{
            gameObject.GetComponent<SquareController>().enabled = false;
        }
    }

    public void OnColorChanged(Color oldValue, Color newValue, bool asServer)
    {
        gameObject.GetComponent<SpriteRenderer>().color = newValue;
    }

    [ServerRpc]
    public void OnPlayerJoinedServer()
    {
        OnPlayerJoined(playerName);

    }

    [ObserversRpc]
    public void OnPlayerJoined(string playerName)
    {
        Debug.Log(playerName+" joined!");
    }

    // Start is called before the first frame update
    void Start()
    {
        playerID = SteamUser.GetSteamID().ToString();
        playerName = SteamFriends.GetPersonaName().ToString();
        color = new Color(Random.value, Random.value, Random.value);
        gameObject.name = playerName;
        // Set location to be random -500 to 500 in x and y
        x = Random.Range(-8, 8);
        y = Random.Range(-4, 4);
        transform.position = new Vector3(x, y, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
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

        if (Input.GetKey(KeyCode.Space))
        {
            OnPlayerJoinedServer();
        }

        // Move the player
        transform.position += new Vector3(moveX, moveY, 0) * Time.deltaTime * 5f; // Adjust the speed multiplier as needed
    }
}
