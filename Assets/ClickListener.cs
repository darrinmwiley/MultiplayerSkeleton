using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickListener : MonoBehaviour
{
    public void OnClick(){
        RPCService.DispatchEventToAllClients("hello networking");
    }
}
