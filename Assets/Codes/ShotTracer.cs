using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotTracer : MonoBehaviour
{
    Material newMaterial;
    // Start is called before the first frame update
    void Start()
    {
        GameObject capsule = transform.GetChild(0).gameObject;
        Renderer renderer = capsule.GetComponent<Renderer>();
        // 새 매터리얼 인스턴스 생성
        newMaterial = new Material(renderer.material);
        newMaterial.SetFloat("_Mode", 2);
        renderer.material = newMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        Color nowColor = newMaterial.color;

        Color endColor = Color.white;
        endColor.a = 0;
        nowColor = Color.Lerp(nowColor, endColor, 0.09f);
        if (nowColor.a <= 0.01f) Destroy(gameObject);
        
        newMaterial.color = nowColor;
    }
}
