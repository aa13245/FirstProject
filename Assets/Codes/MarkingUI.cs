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
        // main ī�޶� �չ��� ī�޶� -> ���� ���� �� ������ ���̰��� 60 ���� ������ ���̰� ����.
        // �׷��� ������ �Ⱥ��̰� 
        Vector3 dir = transform.position - Camera.main.transform.position;
        dotTransform.gameObject.SetActive(Vector3.Angle(dir, Camera.main.transform.forward) < 90);
        dotTransform.position = Camera.main.WorldToScreenPoint(transform.position);
    }
}