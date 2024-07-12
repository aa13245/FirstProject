using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMapMark : MonoBehaviour
{
    public GameObject mapMarkPrefab;
    Transform axisTransform;
    Transform miniMapTransform;
    GameObject mapMark;

    // Start is called before the first frame update
    void Start()
    {
        axisTransform = GameObject.Find("Canvas/MiniMap/MiniMapMask/Axis").transform;
        miniMapTransform = axisTransform.Find("MiniMap");
        mapMark = Instantiate(mapMarkPrefab, miniMapTransform);
        
    }

    // Update is called once per frame
    void Update()
    {
        float scale = axisTransform.localScale.x;
        mapMark.transform.localPosition = new Vector3(transform.position.x, transform.position.z, 0);
        Vector3 scaleValue = Vector3.one * 1;
        if (mapMark.transform.lossyScale != scaleValue)
        {
            mapMark.transform.localScale = scaleValue / scale;
        }
    }
    private void OnDestroy()
    {
        Destroy(mapMark);
    }
}
