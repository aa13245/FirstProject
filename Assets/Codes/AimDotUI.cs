using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AimDotUI : MonoBehaviour
{
    static public AimDotUI instance;

    bool fireReady = true;
    public bool FireReady
    {
        set 
        {
            fireReady = value;
            AlphaChange();
        }
    }
    public enum ZoomState
    {
        OnChanging,
        On,
        Off
    }
    ZoomState isZoomed = ZoomState.Off;
    public ZoomState IsZoomed
    {
        set
        {
            isZoomed = value;
            AlphaChange();
        }
    }
    bool enemyOnAim = false;
    public bool EnemyOnAim
    {
        set
        {
            enemyOnAim = value;
            ChangedEnemyOnAim();
        }
    }
    bool isHide = false;
    public bool IsHide
    {
        set
        {
            isHide = value;
            AlphaChange();
        }
    }
    bool run = false;
    public bool Run
    {
        set
        {
            run = value;
            AlphaChange();
        }
    }
    float speed = 0;
    public float Speed
    {
        set
        {
            speed = value;
            if (run) AlphaChange();
        }
    }
    bool deadEye = false;
    public bool DeadEye
    {
        set
        {
            deadEye = value;
            AlphaChange();
        }
    }

    Image aimDot;
    Image killFilter;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    Image x;
    // Start is called before the first frame update
    void Start()
    {
        aimDot = gameObject.GetComponent<Image>();
        x = aimDot.transform.GetChild(0).GetComponent<Image>();
        killFilter = aimDot.transform.GetChild(1).GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        KillFilter();
    }
    bool killFilterOn = false;
    void KillFilter()
    {
        if (!killFilterOn) return;

        killFilter.color = new Color(1, 1, 1, killFilter.color.a - Time.deltaTime * 0.3f);
        if (killFilter.color.a <= 0) killFilterOn = false;
    }
    void AlphaChange()
    {
        if (deadEye)
        {   // X
            aimDot.enabled = false;
            aimDot.transform.GetChild(0).gameObject.SetActive(false);
        }
        else if (isZoomed == ZoomState.On)
        {   // »Ú
            if (fireReady)
            {
                aimDot.enabled = true;
                aimDot.color = new Color(aimDot.color.r, aimDot.color.g, aimDot.color.b);
                aimDot.transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {   // ≈ı∏Ì
                aimDot.enabled = true;
                aimDot.color = new Color(aimDot.color.r, aimDot.color.g, aimDot.color.b, 0.5f);
                aimDot.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        else if ((run && speed > 2.5f) || (isHide && isZoomed != ZoomState.OnChanging))
        {   // X
            aimDot.enabled = false;
            aimDot.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {   // ≈ı∏Ì
            aimDot.enabled = true;
            aimDot.color = new Color(aimDot.color.r, aimDot.color.g, aimDot.color.b, 0.5f);
            aimDot.transform.GetChild(0).gameObject.SetActive(true);
        }
    }
    void ChangedEnemyOnAim()
    {
        if (enemyOnAim)
        {
            aimDot.color = new Color(1, 0, 0, aimDot.color.a);
        }
        else
        {
            aimDot.color = new Color(1, 1, 1, aimDot.color.a);
        }
    }
    public void Hit(bool isDead)
    {
        if (isDead)
        {
            x.color = Color.red;
            if (!deadEye)
            {
            killFilter.color = new Color(1, 1, 1, 0.1f);
            killFilterOn = true;
            }
        }
        else x.color = new Color(1, 1, 1, 0.5f);
        StartCoroutine(Timer());
    }
    IEnumerator Timer()
    {
        x.enabled = true;
        yield return new WaitForSeconds(0.4f);
        x.enabled = false;
    }
}
