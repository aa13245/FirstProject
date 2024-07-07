using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UI;
using UnityEngine.UI;
using TMPro;

public class PlayerStatus : MonoBehaviour
{
    public float maxHP = 100;
    public float currHP = 100;

    // hp slider
    public Slider hpSlider;
    // hp text
    public TextMeshProUGUI hpText;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Damaged(float damage)
    {
        if (damage > currHP)
        {
            currHP = 0;
        }
        else
        {
            currHP -= damage;
        }
        // text 갱신
        hpText.text = currHP + " / " + maxHP;
        // 슬라이더 갱신
        float ratio = currHP / maxHP;
        hpSlider.value = ratio;
    }
}
