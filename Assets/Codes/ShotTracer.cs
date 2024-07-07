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
        newMaterial.SetFloat("_Mode", 3);
        renderer.material = newMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        Color nowColor = newMaterial.color;
        float h, s, v, a;
        Color.RGBToHSV(nowColor, out h, out s, out v);
        a = nowColor.a;
        s -= 6f * Time.deltaTime;
        a = Mathf.Lerp(0, a, 0.91f);
        Color newColor = Color.HSVToRGB(h, s, v);
        if (a <= 0.0001f) Destroy(gameObject);
        newColor.a = a;
        newMaterial.color = newColor;
    }
}
