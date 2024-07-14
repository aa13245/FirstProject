using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInven : MonoBehaviour
{
    public GameObject Inventory;
    public Mask maskComponent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            maskComponent.showMaskGraphic = !maskComponent.showMaskGraphic;
            //Inventory.SetActive(!Inventory.activeSelf);
        }
    }
}
