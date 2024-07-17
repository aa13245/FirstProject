using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMapMark : MonoBehaviour
{
    Transform axisTransform;
    Transform miniMapTransform;
    MiniMap miniMap;
    GameObject mapMark;
    EnemyBehavior enemyBehavior;
    bool live = true;

    // Start is called before the first frame update
    void Start()
    {
        axisTransform = GameObject.Find("Canvas/MiniMap/MiniMapMask/Axis").transform;
        miniMapTransform = axisTransform.Find("MiniMap");
        miniMap = miniMapTransform.GetComponent<MiniMap>();
        mapMark = Instantiate(miniMap.mapMarkPrefab, miniMapTransform);
        enemyBehavior = gameObject.GetComponent<EnemyBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        float scale = axisTransform.localScale.x;
        mapMark.transform.localPosition = new Vector3(transform.position.x, transform.position.z, 0);
        mapMark.transform.localEulerAngles = Vector3.back * axisTransform.eulerAngles.z;
        Vector3 scaleValue = Vector3.one * 1;
        if (mapMark.transform.lossyScale != scaleValue)
        {
            mapMark.transform.localScale = scaleValue / scale;
        }
        if (live && enemyBehavior.currHP <= 0)
        {
            Destroy(mapMark);
            mapMark = Instantiate(miniMap.deadMarkPrefab, miniMapTransform);
            live = false;
        }
    }
    private void OnDestroy()
    {
        Destroy(mapMark);
    }
}
