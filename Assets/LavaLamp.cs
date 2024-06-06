using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaLamp : MonoBehaviour
{
    public Material materialPrefab;
    public Color bgColor;
    public Color bubbleColor;
    public float timeOffset;

    private Material mat;

    void Start()
    {
        mat = GetComponent<MeshRenderer>().material = Instantiate(materialPrefab);
        mat.SetColor("_BGColor", bgColor);
        mat.SetColor("_Color", bubbleColor);
        mat.SetFloat("_TimeOffset", timeOffset);
    }

    void Update(){
        mat.SetColor("_BGColor", bgColor);
        mat.SetColor("_Color", bubbleColor);
        mat.SetFloat("_TimeOffset", timeOffset);
        mat.SetFloat("_TimeS", Time.time);
    }
}
