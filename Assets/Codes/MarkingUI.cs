using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkingUI : MonoBehaviour
{
    Transform dotTransform;

    // Start is called before the first frame update
    void Start()
    {
        dotTransform = transform.Find("Canvas/Image");
    }

    // Update is called once per frame
    void Update()
    {
        // main 카메라 앞방향 카메라 -> 나의 방향 두 벡터의 사이각이 60 보다 작으면 보이게 하자.
        // 그렇지 않으면 안보이게 
        Vector3 dir = transform.position - Camera.main.transform.position;
        dotTransform.gameObject.SetActive(Vector3.Angle(dir, Camera.main.transform.forward) < 90);
        dotTransform.position = Camera.main.WorldToScreenPoint(transform.position);
    }
}