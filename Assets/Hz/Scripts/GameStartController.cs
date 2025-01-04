using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartController : MonoBehaviour
{
    public GameObject square;
    public Mask maskComponent;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            maskComponent.showMaskGraphic = !maskComponent.showMaskGraphic;

            Invoke("LoadScene", 0.1f);
        }
    }

    void LoadScene()
    {

        SceneManager.LoadScene("NewMapScene");
    }
}
