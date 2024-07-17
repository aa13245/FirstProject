using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscController : MonoBehaviour
{
    public GameObject square;
    public Mask maskComponent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            maskComponent.showMaskGraphic = !maskComponent.showMaskGraphic;
        }
    }
}
