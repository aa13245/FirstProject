using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    Transform playerBodyTransform;
    Transform camAxisTransform;
    Transform axis;
    Transform frame;
    Transform playerMark;
    bool run = false;
    // Start is called before the first frame update
    void Start()
    {
        playerBodyTransform = GameObject.Find("Player/Body").transform;
        camAxisTransform = playerBodyTransform.parent.Find("CameraAxis");
        axis = transform.parent;
        frame = axis.parent.parent.Find("Frame");
        playerMark = transform.parent.parent.Find("PlayerMapMark");
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = new Vector3(-playerBodyTransform.position.x, - playerBodyTransform.position.z, 0);
        axis.localEulerAngles = new Vector3(0, 0, camAxisTransform.eulerAngles.y);
        frame.localEulerAngles = new Vector3(0, 0, camAxisTransform.eulerAngles.y);
        playerMark.localEulerAngles = new Vector3(0, 0, 180 + Mathf.DeltaAngle(playerBodyTransform.eulerAngles.y, camAxisTransform.eulerAngles.y));
        // ¹Ì´Ï¸Ê ÁÜ½ºÄÉÀÏ º¯°æ
        if (run)
        {
            if (axis.localScale.x > 2)
            {
                axis.localScale -= Vector3.one * 2 * Time.deltaTime;
            }
        }
        else
        {
            if (axis.localScale.x < 4)
            {
                axis.localScale += Vector3.one * 2 * Time.deltaTime;
            }
        }
    }
    public void RunScale(bool value)
    {
        if (run != value) run = value;
    }
}
