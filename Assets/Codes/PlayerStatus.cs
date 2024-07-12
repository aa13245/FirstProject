using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UI;
using UnityEngine.UI;
using TMPro;

public class PlayerStatus : MonoBehaviour
{
    public float maxHP = 100;
    public float currHP;
    public float maxDeadEye = 100;
    public float currDeadEye;

    // hp
    GameObject hpUI;
    // hp value
    Image hpValue;
    // hp effect
    Image hpEffect;
    // dead eye
    GameObject deadEyeUI;
    // dead eye value
    Image deadEyeValue;
    // dead eye effect
    Image deadEyeEffect;

    // Start is called before the first frame update
    void Start()
    {
        currHP = maxHP;
        currDeadEye = maxDeadEye;
        hpUI = GameObject.Find("HP");
        hpValue = hpUI.transform.Find("Value").GetComponent<Image>();
        hpEffect = hpUI.transform.Find("Effect").GetComponent<Image>();
        deadEyeUI = GameObject.Find("DeadEye");
        deadEyeValue = deadEyeUI.transform.Find("Value").GetComponent<Image>();
        deadEyeEffect = deadEyeUI.transform.Find("Effect").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hpEffect.color.a > 0)
        {
            hpEffect.color = new Color(hpEffect.color.r, hpEffect.color.g, hpEffect.color.b, hpEffect.color.a - Time.deltaTime / Time.timeScale);
        }
        if (deadEyeEffect.color.a > 0)
        {
            deadEyeEffect.color = new Color(deadEyeEffect.color.r, deadEyeEffect.color.g, deadEyeEffect.color.b, deadEyeEffect.color.a - Time.deltaTime / Time.timeScale);
        }
        if (currDeadEye != maxDeadEye)
        {
            currDeadEye += 0.5f * Time.deltaTime;
            if (currDeadEye > maxDeadEye)
            {
                currDeadEye = maxDeadEye;
            }
        }
        if (deadEyeValue.fillAmount != 0) deadEyeValue.fillAmount = currDeadEye / maxDeadEye;
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
            if (damage > 0)
            {   // 효과
                hpEffect.color = new Color(hpEffect.color.r, hpEffect.color.g, hpEffect.color.b, 0.3f);
            }
        }
        // 슬라이더 갱신
        float ratio = currHP / maxHP;
        hpValue.fillAmount = ratio;
    }
    public bool DeadEyeCheck()
    {
        if (currDeadEye >= valuePerTick) return true;
        else return false;
    }
    float tickTimer = 0;
    float valuePerTick = 1.5f;
    public bool DeadEye()
    {
        tickTimer += Time.deltaTime / Time.timeScale;
        if (tickTimer >= 0.3f)
        {
            tickTimer = 0;
            if (currDeadEye >= valuePerTick)
            {
                currDeadEye -= valuePerTick;
                deadEyeValue.fillAmount = currDeadEye / maxDeadEye;
                deadEyeEffect.color = new Color(deadEyeEffect.color.r, deadEyeEffect.color.g, deadEyeEffect.color.b, 0.2f);
                return true;
            }
            else return false;
        }
        else return true;
    }
}
