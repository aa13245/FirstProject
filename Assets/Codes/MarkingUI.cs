using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkingUI : MonoBehaviour
{
    Transform dotTransform;
    Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        dotTransform = transform.Find("Canvas/Image");
        playerTransform = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        dotTransform.position = Camera.main.WorldToScreenPoint(transform.position);
    }
}
