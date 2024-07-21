using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CheckPoint : MonoBehaviour
{
    public GameObject[] points;
    public GameObject markPrefab;
    GameObject mark;

    Transform axisTransform;
    Transform miniMapTransform;
    MiniMap miniMap;
    Transform playerBodyTransform;

    int pointNum;
    // Start is called before the first frame update
    void Start()
    {
        if (points.Length != 0)
        {
            axisTransform = GameObject.Find("Canvas/MiniMap/MiniMapMask/Axis").transform;
            miniMapTransform = axisTransform.Find("MiniMap");
            miniMap = miniMapTransform.GetComponent<MiniMap>();
            playerBodyTransform = GameObject.Find("Player/Body").transform;
            mark = Instantiate(markPrefab, miniMapTransform);
            mark.GetComponent<Image>().color = Color.yellow;
        }
    }
    private void Update()
    {
        if (pointNum > points.Length - 1)
        {
            Destroy(mark);
            return;
        }
        float dis = Vector3.Distance(new Vector3(points[pointNum].transform.position.x, 0, points[pointNum].transform.position.z), new Vector3(playerBodyTransform.position.x, 0, playerBodyTransform.position.z));
        if (dis < 5)
        {
            points[pointNum].SetActive(false);
            pointNum++;
        }
    }
    private void LateUpdate()
    {
        if (pointNum > points.Length - 1) return;
        float scale = axisTransform.localScale.x;
        mark.transform.localPosition = new Vector3(points[pointNum].transform.position.x, points[pointNum].transform.position.z, 0);
        mark.transform.localEulerAngles = Vector3.back * axisTransform.eulerAngles.z;
        Vector3 scaleValue = Vector3.one * 1;
        if (mark.transform.lossyScale != scaleValue)
        {
            mark.transform.localScale = scaleValue / scale;
        }
        float distance = Vector3.Distance(points[pointNum].transform.position, new Vector3(playerBodyTransform.position.x, 0, playerBodyTransform.position.z)) * (axisTransform.localScale.x + 1);
        if (distance > 144)
        {
            float ratio = 1 / axisTransform.localScale.x;
            Vector3 normal = new Vector3(points[pointNum].transform.position.x - playerBodyTransform.position.x, points[pointNum].transform.position.z - playerBodyTransform.position.z, 0).normalized;
            mark.transform.localPosition = new Vector3(playerBodyTransform.position.x, playerBodyTransform.position.z, 0) + normal * ratio * 90;
        }
    }
}
