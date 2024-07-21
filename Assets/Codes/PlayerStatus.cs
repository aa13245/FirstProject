using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor.UI;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerStatus : MonoBehaviour
{
    public float maxHP = 100;
    public float currHP;
    public float maxDeadEye = 100;
    public float currDeadEye;
    // 목숨
    public bool life = true;

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
    GameObject deadEyeFilter;
    // 비네트 필터
    Image vignetteFilter;
    public WaistAngle waistAngle;
    // Animator
    public Animator anim;
    // 무기
    public GameObject rifle;
    // 총알 수 UI
    GameObject bulletUI;
    // 장전된 총알 수
    int bulletNum = 7;

    // 재장전 소리
    AudioSource audioSource;
    public AudioClip reloadSound;
    public AudioClip cliplnSound;

    // 피격 사운드
    AudioManager audioManager;
    public int BulletNum
    {
        get
        {
            return bulletNum;
        }
        set
        {
            bulletNum = value;
            bulletUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = bulletNum.ToString();
        }
    }
    // 전체 총알 수
    public int wholeBulletNum = 100;
    public bool isReloading = false;
    public void Reload()
    {
        if (wholeBulletNum == 0 || bulletNum == 7) return;
        isReloading = true;
        audioSource.PlayOneShot(reloadSound);
        anim.SetTrigger("Reloading");
        StartCoroutine(ReloadWait());
    }
    IEnumerator ReloadWait()
    {
        yield return new WaitForSeconds(1.5f);
        ReloadComplete();
        audioSource.PlayOneShot(cliplnSound);
    }
    public void ReloadComplete()
    {
        int num = 7 - BulletNum;
        if (wholeBulletNum < num)
        {
            BulletNum += wholeBulletNum;
            wholeBulletNum = 0;
        }
        else
        {
            wholeBulletNum -= num;
            BulletNum = 7;
        }
        bulletUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = wholeBulletNum.ToString();
        isReloading = false;
    }
    // 손 상태
    public enum WeaponState
    {
        Hand,
        Rifle,
    }
    public WeaponState weaponState;
    void ChangeHand(WeaponState s)
    {
        if (s == weaponState) return;
        weaponState = s;
        if (weaponState == WeaponState.Hand)
        {
            anim.SetTrigger("Hand");
            rifle.SetActive(false);
            bulletUI.SetActive(false);
        }
        else if (weaponState == WeaponState.Rifle)
        {
            anim.SetTrigger("Rifle");
            rifle.SetActive(true);
            bulletUI.SetActive(true);
        }
    }
    public bool aimingState = false;
    public void ChangeAiming(bool s)
    {
        if (s == aimingState) return;
        aimingState = s;
        if (aimingState)
        {
            anim.SetTrigger("AimingOn");
        }
        else
        {
            anim.SetTrigger("AimingOff");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rifle.SetActive(false);
        currHP = maxHP;
        currDeadEye = maxDeadEye;
        hpUI = GameObject.Find("HP");
        hpValue = hpUI.transform.Find("Value").GetComponent<Image>();
        hpEffect = hpUI.transform.Find("Effect").GetComponent<Image>();
        deadEyeUI = GameObject.Find("DeadEye");
        deadEyeValue = deadEyeUI.transform.Find("Value").GetComponent<Image>();
        deadEyeEffect = deadEyeUI.transform.Find("Effect").GetComponent<Image>();
        vignetteFilter = GameObject.Find("VignetteFilter").GetComponent<Image>();
        deadEyeFilter = GameObject.Find("DeadEyeFilter");
        bulletUI = GameObject.Find("Canvas/Bullet");
        audioSource = GetComponent<AudioSource>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        bulletUI.SetActive(false);
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
        if (vignetteFilter.color.a > 0 && life)
        {
            vignetteFilter.color = new Color(1, 1, 1, vignetteFilter.color.a - Time.deltaTime / Time.timeScale);
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

        if (!life)
        {
            DieEffect();
            return;
        }
        // 무기 변경
        bool num1 = Input.GetKeyDown(KeyCode.Alpha1);
        bool num2 = Input.GetKeyDown(KeyCode.Alpha2);
        if (num1) ChangeHand(WeaponState.Hand);
        if (num2) ChangeHand(WeaponState.Rifle);
        // 장전
        if (Input.GetKeyDown(KeyCode.R) && !aimingState && !isReloading && weaponState == WeaponState.Rifle) Reload();
    }

    public void Damaged(float damage)
    {
        if (damage > currHP)
        {
            if (life) Die();
        }
        else
        {
            currHP -= damage;
            if (damage > 0)
            {   // 효과
                hpEffect.color = new Color(hpEffect.color.r, hpEffect.color.g, hpEffect.color.b, 0.3f);
                vignetteFilter.color = new Color(1, 1, 1, 0.5f);
                anim.SetTrigger("Hit");
                waistAngle.RecoilSet(10);
                audioSource.PlayOneShot(audioManager.painSounds[Random.Range(0, audioManager.painSounds.Length)]);
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
    void Die()
    {
        currHP = 0;
        life = false;
        anim.SetTrigger("Die");
        deadEyeFilter.SetActive(false);
        vignetteFilter.color = new Color(1, 1, 1, 1);
    }
    float sceneTimer = 0;
    void DieEffect()
    {
        Time.timeScale += (0.3f - Time.timeScale) * Time.deltaTime / Time.timeScale;
        sceneTimer += Time.deltaTime / Time.timeScale;
        if (sceneTimer > 5) SceneManager.LoadScene("GameOverScene");
    }
}
