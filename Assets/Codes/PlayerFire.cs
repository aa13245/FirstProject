using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerFire : MonoBehaviour
{
    // 발사위치
    public GameObject firePos;

    // 발사 힘
    public float firePower = 100;

    // 연사 속도 (쿨타임)
    public float fireDelay = 1f;
    // 공격 쿨 돌았는지
    bool fireEnable = true;

    // 스킬 - 데드아이
    public bool deadEyeOn = false;
    public bool deadEyeShooting = false;
    float deadEyeFireDelay = 0.1f; // 데드아이 연사력
    float deadEyeFireTimer = 0;
    Image filterImageComp;
    // 데드아이 마킹
    List<GameObject> deadEyeMarkings;

    // 마킹 프리팹
    public GameObject markingFactory;
    // 파편효과 공장(프리팹)
    public GameObject bulletImpactFactory;
    // 발사
    public GameObject fireEffect;
    // 발사 소리
    public AudioClip fireSound;
    // 재장전 소리
    public AudioClip reloadSound;
    public AudioClip cliplnSound;
    public AudioClip clickSound;
    // 총알 궤적
    public GameObject shotTracerprefab;
    public WaistAngle waistAngle;

    // 카메라 컴포넌트
    Transform cameraAxisTransform;
    CamMove camMove;
    Transform bodyTransform;

    AudioSource audioSource;
    PlayerStatus playerStatus;
    // 에임
    Image aimDot;
    // Animator
    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        cameraAxisTransform = transform.Find("CameraAxis");
        camMove = cameraAxisTransform.GetComponent<CamMove>();
        bodyTransform = transform.Find("Body");
        audioSource = GetComponent<AudioSource>();
        playerStatus = gameObject.GetComponent<PlayerStatus>();
        aimDot = GameObject.Find("AimDot").GetComponent<Image>();
        filterImageComp = GameObject.Find("DeadEyeFilter").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        // 만약에 왼쪽 마우스 버튼을 누르면
        if(Input.GetMouseButtonDown(0))
        {
            // 줌 했을 때 and 쿨 돌았을 때
            if (camMove.zoom && !camMove.isZoomChanging && fireEnable)
            {
                if (deadEyeOn)
                {
                    if (!deadEyeShooting) DeadEyeMarking();
                }
                else
                {
                    // 카메라 위치, 카메라 앞방향 Ray를 만든다.
                    Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                    // Ray를 발사해서 어딘가에 맞았다면
                    RaycastHit hitInfo = new RaycastHit();
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                        Fire(hitInfo.point, false);
                    }
                    else Fire(Camera.main.transform.forward, true);
                } 
            }
        }

        // 데드아이 ON/Off
        if (Input.GetMouseButtonDown(2) && !camMove.isZoomChanging)
        {
            if (deadEyeOn)
            { // Off
                if (deadEyeMarkings.Count == 0)
                {   // 마킹한게 없을 때
                    DeadEyeOnOff(false);
                }
                else
                {   // 발사 시작
                    deadEyeShooting = true;
                    AimDotUI.instance.DeadEye = true;
                }
                
            }
            else if (playerStatus.DeadEyeCheck())
            { // On
                DeadEyeOnOff(true);
            }
        }
        // 필터
        if (deadEyeOn)
        {
            if (filterImageComp.color.a < 0.05f)
            {
                filterImageComp.color += new Color(0, 0, 0, 0.3f * Time.deltaTime);
            }
            // 게이지
            if (!deadEyeShooting)
            {
                if (!playerStatus.DeadEye())
                {   // 게이지 모두 소모 시 자동 꺼짐
                    if (deadEyeMarkings.Count == 0)
                    {   // 마킹한게 없을 때
                        DeadEyeOnOff(false);
                    }
                    else
                    {   // 발사 시작
                        deadEyeShooting = true;
                        AimDotUI.instance.DeadEye = true;
                    }
                }
            }
        }
        else
        {
            if (filterImageComp.color.a > 0)
            {
                filterImageComp.color -= new Color(0, 0, 0, 0.1f * Time.deltaTime);
            }
        }

        // 데드아이 발사
        if (deadEyeShooting)
        {
            if (deadEyeMarkings.Count != 0)
            {
                deadEyeFireTimer += Time.deltaTime;
                // 플레이어 -> 마킹 각도
                Vector3 markingDir = deadEyeMarkings[0].transform.position - transform.position;
                float markingAngle = Mathf.Atan2(markingDir.x, markingDir.z) * Mathf.Rad2Deg;
                // 현재 플레이어의 각도
                float playerAngle = bodyTransform.eulerAngles.y;
                // 남은 회전 각도
                float deltaAngle = Mathf.DeltaAngle(playerAngle, markingAngle);
                // 이번 프레임에 회전할 각도
                float value = (10 + Mathf.Abs(deltaAngle) * 18) * Time.deltaTime;
                // 회전할 각도가 남은 각도보다 크면
                if (Mathf.Abs(deltaAngle) <= Mathf.Abs(value))
                {   // 플레이어 각도 = 타겟 각도
                    bodyTransform.eulerAngles = new Vector3(0, markingAngle, 0);

                }
                else
                {
                    // 남은 각도가 0보다 크면 시계 방향 회전
                    if (deltaAngle > 0)
                    {
                        bodyTransform.eulerAngles = new Vector3(0, playerAngle + value, 0);
                    }
                    // 남은 각도가 0보다 작으면 반시계 방향 회전
                    else
                    {
                        bodyTransform.eulerAngles = new Vector3(0, playerAngle - value, 0);
                    }
                }
                if (deadEyeFireTimer >= deadEyeFireDelay)
                {
                    deadEyeFireTimer = 0;
                    Fire(deadEyeMarkings[0].transform.position, false);
                    Destroy(deadEyeMarkings[0]);
                    deadEyeMarkings.RemoveAt(0);
                }
            }
            else
            {
                deadEyeFireTimer += Time.deltaTime;
                if (deadEyeFireTimer >= deadEyeFireDelay)
                {   // 끝
                    deadEyeFireTimer = 0;
                    DeadEyeOnOff(false);
                    deadEyeShooting = false;
                    AimDotUI.instance.DeadEye = false;
                }
            }
        }
    }

    void DeadEyeOnOff(bool enable)
    {
        deadEyeOn = enable;
        camMove.Zoom(enable);
        if (enable)
        {
            deadEyeMarkings = new List<GameObject>();
            Time.timeScale = 0.3f;
            playerStatus.ReloadComplete();
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    void DeadEyeMarking()
    {
        if (deadEyeMarkings.Count >= playerStatus.BulletNum) return;
        // 카메라 위치, 카메라 앞방향 Ray를 만든다.
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        // Ray를 발사해서 어딘가에 맞았다면
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(ray, out hitInfo) && hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            GameObject marking = Instantiate(markingFactory, hitInfo.transform);
            marking.transform.position = hitInfo.point;
            deadEyeMarkings.Add(marking);
        }
    }
    void ShotTracer(Vector3 pos, float distance)
    {
        GameObject shotTracer = Instantiate(shotTracerprefab);
        shotTracer.transform.position = firePos.transform.position;
        shotTracer.transform.forward = pos;
        shotTracer.transform.Translate(0, 0, distance / 2);
        shotTracer.transform.localScale = new Vector3(1, 1, distance);
    }
    void Fire(Vector3 aimPos, bool air)
    {
        if (playerStatus.BulletNum == 0)
        {
            IEnumerator Delay()
            {
                yield return new WaitForSeconds(0.15f);
                audioSource.Stop();
            }
            audioSource.resource = clickSound;
            audioSource.Play();
            StartCoroutine(Delay());
            return;
        }
        playerStatus.BulletNum -= 1;
        if (!air)
        {   // 허공에 안쐈을 때
            // 총구 위치, 총구 앞방향 Ray를 만든다.
            Ray ray = new Ray(firePos.transform.position, aimPos - firePos.transform.position);
            // Ray를 발사해서 어딘가에 맞았다면
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(ray, out hitInfo))
            {
                // 맞은 위치에 파편효과를 보여준다.
                // Debug.Log("맞은 오브젝트 : " + hitInfo.transform.name);

                // 파편효과 공장에서 파편효과를 만든다.
                GameObject bulletImpact = Instantiate(bulletImpactFactory);
                // 만든 파편효과를 맞은 위치에 놓는다.
                bulletImpact.transform.position = hitInfo.point;
                // 만든 파편효과의 앞방향을 맞은 위치의 normal 값으로 셋팅한다.
                bulletImpact.transform.forward = hitInfo.normal;
                // 만든 파편효과를 2초뒤에 파괴하자
                Destroy(bulletImpact, 2);
                ShotTracer(aimPos - firePos.transform.position, Vector3.Distance(firePos.transform.position, aimPos));
                // 맞은 대상이 Enemy 라면
                if (hitInfo.transform.gameObject.name.Contains("Enemy"))
                {
                    // Enemy 에게 데미지를 주자
                    // Enemy 에서 EnemyBehavior 컴포넌트를 가져오자
                    EnemyBehavior enemy = hitInfo.transform.GetComponent<EnemyBehavior>();
                    // 가져온 컴포넌트에서 OnDamaged 함수를 호출
                    if (enemy.OnDamaged(40)) AimDotUI.instance.Hit(true);
                    else if (enemy.GetComponent<EnemyBehavior>().currHP > 0) AimDotUI.instance.Hit(false);
                }
                else if (hitInfo.transform.gameObject.name.Contains("Head"))
                {
                    EnemyBehavior enemy = hitInfo.transform.GetComponentInParent<EnemyBehavior>();
                    if (enemy.OnDamaged(100)) AimDotUI.instance.Hit(true);
                }
            }
        }
        else
        {   // 허공에 쐈을 때
            ShotTracer(aimPos, 100);
        }
        // 총 사운드
        audioSource.PlayOneShot(fireSound);
        audioSource.resource = cliplnSound;
        audioSource.PlayDelayed(0.5f);
        // 화염 효과
        GameObject fire = Instantiate(fireEffect, firePos.transform.position, Quaternion.Euler(Vector3.zero));
        fire.transform.localScale = Vector3.one * 0.1f;
        // 반동
        anim.SetTrigger("Fire");
        camMove.RecoilSet();
        waistAngle.RecoilSet(10);

        // 쿨 돌리기
        StartCoroutine(WaitCooltime());
    }
    IEnumerator WaitCooltime()
    {
        fireEnable = false;
        AimDotUI.instance.FireReady = false;
        yield return new WaitForSeconds(fireDelay);
        fireEnable = true;
        AimDotUI.instance.FireReady = true;
    }
}
