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
    GameObject fireDirMark;
    float fireTimer;

    // Start is called before the first frame update
    void Start()
    {
        axisTransform = GameObject.Find("Canvas/MiniMap/MiniMapMask/Axis").transform;
        miniMapTransform = axisTransform.Find("MiniMap");
        miniMap = miniMapTransform.GetComponent<MiniMap>();
        mapMark = Instantiate(miniMap.mapMarkPrefab, miniMapTransform);
        enemyBehavior = gameObject.GetComponent<EnemyBehavior>();
        fireDirMark = Instantiate(miniMap.enemyFireDirMarkPrefab, axisTransform.parent.parent);
        fireDirMark.SetActive(false);
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
        if (fireTimer < 5)
        {
            fireTimer += Time.deltaTime / Time.timeScale;
            if (fireTimer >= 5) fireDirMark.SetActive(false);
        }
    }
    public void Fire()
    {
        fireDirMark.SetActive(true);
        fireTimer = 0;
        Vector3 pos = mapMark.transform.position - axisTransform.transform.position;
        float angle = Mathf.Atan2(pos.x, pos.y) * Mathf.Rad2Deg;
        fireDirMark.transform.eulerAngles = Vector3.back * angle;
    }
    private void OnDestroy()
    {
        Destroy(mapMark);
    }
}
