using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersController : MonoBehaviour
{
    private static PlayersController instance;

    private void Awake() => instance = this;

    void Start(){

    }

    public GameObject FindPlayerByID(string id)
    {
        foreach(Transform child in transform){
            SquareController square = child.gameObject.GetComponent<SquareController>();
            if(square.playerID == id)
                return child.gameObject;
        }
        return null;
    }



}
