using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using static PlayerMove;
//using static UnityEditor.Experimental.GraphView.GraphView;


// 마우스의 움직임에 따라
// 카메라, 캐릭터(Player)를 회전하고 싶다.
public class CamMove : MonoBehaviour
{
    // 회전값(마우스의 움직임을 누적하는 값)
    float rotX = 0;
    float rotY = 0;

    // cameraTransform
    Transform cameraTransform;
    // 카메라 최대/최소 거리
    public float camMax = -3.7f;
    public float camMin = -2;
    // 줌 카메라 거리
    public float camZoom = -1.5f;
    // 카메라 속도
    public float camSpeed = 0;
    public float crouchCamSpeed = 0;
    // 앉기 카메라 오프셋
    Vector3 crouchOffset = Vector3.zero;
    // 캠 거리
    Vector3 camDistance = new Vector3(0, 0, -2);

    // 회전 스피드
    public float rotSpeed = 200;

    // 회전 가능 여부
    public bool useVertical = false;
    public bool useHorizontal = false;

    // 줌 활성화
    public bool zoom = false;
    // 줌 변경 중
    public bool isZoomChanging = false;
    // 줌 걸리는 시간
    float zoomOnTime = 0.2f;
    // 줌 해제 걸리는 시간
    float zoomOffTime = 0.4f;
    // 초당 줌 거리
    Vector3 zoomPerSec;
    // 에임 색
    bool onEnemy = false;

    PlayerMove playerMove;
    Transform bodyTransform;
    PlayerFire playerFire;
    PlayerStatus playerStatus;
    // 에임
    Image aimDot;

    public Vector3 lookPos;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        playerMove = transform.GetComponentInParent<PlayerMove>();
        playerFire = transform.GetComponentInParent<PlayerFire>();
        playerStatus = transform.GetComponentInParent<PlayerStatus>();
        bodyTransform = transform.parent.Find("Body");
        cameraTransform = transform.Find("Main Camera");
        aimDot = GameObject.Find("AimDot").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        CursorSet();
        // 1. 마우스의 움직임값을 받아오자
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        // 2. 마우스의 움직임값을 누적시키자
        if (useHorizontal == true)
        {
            rotX += mx * Time.deltaTime / Time.timeScale * rotSpeed;
        }
        

        if(useVertical == true)
        {
            rotY += my * Time.deltaTime / Time.timeScale * rotSpeed;
            rotY = Mathf.Clamp(rotY, -80f, 50f);
        }

        // 3. 누적된 값을 물체의 회전값으로 셋팅하자
        transform.localEulerAngles = new Vector3(-rotY, rotX);

        // 줌
        // 마우스 클릭
        bool mouse = Input.GetMouseButton(1);
        // 줌 활성화
        if (!playerFire.deadEyeOn)
        {
            if (mouse && playerStatus.weaponState != PlayerStatus.WeaponState.Hand && !zoom)
            {
                Zoom(true);
            }

            // 줌 완료 상태 and 우클릭 해제 시 줌 해제
            else if (zoom && (!mouse || playerStatus.weaponState == PlayerStatus.WeaponState.Hand) && !isZoomChanging)
            {   // 공격 불가
                Zoom(false);
                AimDotUI.instance.IsZoomed = AimDotUI.ZoomState.Off;
            }
        }

        CamPos();
        Recoil();

        // 에임이 적 위에 있으면 에임 빨간색
        // 카메라 위치, 카메라 앞방향 Ray를 만든다.
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        // Ray를 발사해서 어딘가에 맞았다면
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(ray, out hitInfo) && hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (!onEnemy)
            {
                AimDotUI.instance.EnemyOnAim = true;
                onEnemy = true;
            }
        }
        else
        {
            if (onEnemy)
            {
                AimDotUI.instance.EnemyOnAim = false;
                onEnemy = false;
            }
        }
    }
    Vector3 zoomOffset;

    Vector3 crouchZoom = new Vector3(0, 0, 0.6f);
    Vector3 hideZoom = new Vector3(0, 0, 0.8f);
    Vector3 hideStandingZoom = new Vector3(0, 0.4f, 0.8f);
    Vector3 hideCrouchZoom = new Vector3(0, 0, 0.8f);
    public void Zoom(bool on)
    {
        zoomOffset = Vector3.zero;
        playerStatus.ChangeAiming(on);
        zoom = on;
        isZoomChanging = true;
        playerMove.turnSpeed = 0;
        AimDotUI.instance.IsZoomed = AimDotUI.ZoomState.OnChanging;
        if (on)
        {
            if (playerMove.hideState == PlayerMove.HideState.Off)
            {   // 엄폐 상태 아닐 때 줌 거리 설정
                if (playerMove.state == PlayerMove.PlayerState.Crouch)
                {   // 앉았을 때
                    zoomPerSec = (new Vector3(0, 0, camZoom - camDistance.z) + crouchZoom) / zoomOnTime;
                    zoomOffset += crouchZoom;
                }
                else
                {   // 서있을 때
                    zoomPerSec = new Vector3(0, 0, camZoom - camDistance.z) / zoomOnTime;
                }
            }
            else
            {   // 엄폐 상태일 때 줌 거리 설정
                if (playerMove.state == PlayerMove.PlayerState.Crouch)
                {   
                    if ((playerMove.leftHit ^ playerMove.rightHit))
                    {   // 모서리
                        zoomPerSec = (new Vector3(0, 0, camZoom - camDistance.z) + hideCrouchZoom) / zoomOnTime;
                        zoomOffset += hideCrouchZoom;
                    }
                    else
                    {   // 일어설 때
                        zoomPerSec = (new Vector3(0, - camDistance.y, camZoom - camDistance.z) + hideStandingZoom) / zoomOnTime;
                        zoomOffset += hideStandingZoom;
                    }
                }
                else
                {   // 서있을 때
                    zoomPerSec = (new Vector3(0, 0, camZoom - camDistance.z) + hideZoom) / zoomOnTime;
                    zoomOffset += hideZoom;
                }
            }
        }
        else
        {
            camSpeed = 0;
            //zoomPerSec = Vector3.right * (camMin - camDistance.z) / zoomOffTime;
        }
    }
    // 플레이어 기준 카메라 위치 - false : 왼쪽, true : 오른쪽
    bool camPos = true;
    float wallDis = -10;
    void CamPos()
    {
        // X
        float dis = 0.5f;
        if (camPos)
        {
            if (cameraTransform.localPosition.x < dis)
            {
                Vector3 value = Vector3.right * (dis - cameraTransform.localPosition.x) * 3 * Time.deltaTime / Time.timeScale;
                if (0.5f - cameraTransform.localPosition.x < value.magnitude)
                {
                    cameraTransform.localPosition = new Vector3(0.5f, cameraTransform.localPosition.y, cameraTransform.localPosition.z);
                }
                else cameraTransform.Translate(value);
            }
        }
        else
        {
            if (cameraTransform.localPosition.x > -dis)
            {
                Vector3 value = Vector3.left * (dis + cameraTransform.localPosition.x) * 3 * Time.deltaTime / Time.timeScale;
                if (0.5f + cameraTransform.localPosition.x < value.magnitude)
                {
                    cameraTransform.localPosition = new Vector3(-0.5f, cameraTransform.localPosition.y, cameraTransform.localPosition.z);
                }
                else cameraTransform.Translate(value);
            }
        }
        // Z
        if (!zoom)
        {
            if ((playerMove.hideState == PlayerMove.HideState.Off || playerMove.hideState == PlayerMove.HideState.Approaching) && playerMove.state != PlayerMove.PlayerState.Crouch)
            {   // 일반 움직임 시 앞 뒤 움직임
                float player2CamDelta = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, bodyTransform.rotation.eulerAngles.y);
                float speedFromCam = playerMove.speed * Mathf.Cos(player2CamDelta * Mathf.Deg2Rad);
                float value = -speedFromCam * Time.deltaTime;
                if ((value < 0 && camDistance.z > camMax) || (value > 0 && camDistance.z < camMin))
                {
                    camDistance.z += -speedFromCam * Time.deltaTime * 0.7f;
                }
            }
            if (playerMove.hideState != PlayerMove.HideState.Off)
            {   // 엄폐 시
                if (camDistance.z < camMin - 0.5f)
                {
                    if (camSpeed < 3) camSpeed += 3 * Time.deltaTime;
                    camDistance.z += (camMin - 0.5f - camDistance.z) * camSpeed * Time.deltaTime;
                }
            }
            
        }
        // 줌 On/Off 시 카메라 이동
        if (isZoomChanging)
        {
            if (zoom)
            {   // 줌 On
                if (camZoom + zoomOffset.z > camDistance.z)
                {
                    camDistance += zoomPerSec * Time.deltaTime / Time.timeScale; // 카메라 이동
                }
                else
                {   // 공격 가능
                    isZoomChanging = false; // 완료
                    AimDotUI.instance.IsZoomed = AimDotUI.ZoomState.On;
                }
            }
            else
            {   // 줌 Off
                if (camDistance.z > camMin + 0.05f)
                {
                    if (camSpeed < 6) camSpeed += 8 * Time.deltaTime;
                    camDistance.z += (camMin - camDistance.z) * camSpeed * Time.deltaTime;
                    camDistance.y += (-camDistance.y) * camSpeed * 2 * Time.deltaTime;
                }
                else
                {
                    isZoomChanging = false; // 완료
                }
            }
        }

        // 앉기 카메라 줌
        if (crouchCamSpeed < 3) crouchCamSpeed += 3 * Time.deltaTime;
        if (playerMove.state == PlayerMove.PlayerState.Crouch)
        {
            if (crouchOffset.z < 0)
            {
                crouchOffset.z += (0 - crouchOffset.z) * crouchCamSpeed * Time.deltaTime;
            }
            if (crouchOffset.y > -0.4f)
            {
                crouchOffset.y += (-0.4f -crouchOffset.y) * crouchCamSpeed * Time.deltaTime;
            }
            if (camDistance.z < camMin)
            {
                camDistance.z += (camMin - camDistance.z) * crouchCamSpeed * Time.deltaTime;
            }
        }
        else if (!zoom && !isZoomChanging)
        {
            if (crouchOffset.z > 0)
            {
                crouchOffset.z += (- crouchOffset.z) * crouchCamSpeed * Time.deltaTime;
            }
            if (crouchOffset.y < 0)
            {
                crouchOffset.y += (- crouchOffset.y) * crouchCamSpeed * Time.deltaTime;
            }
        }

        // 카메라 뒤에 벽 뚫림 방지
        Ray ray = new Ray(cameraTransform.parent.position, -cameraTransform.forward);
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(ray, out hitInfo) && Vector3.Distance(cameraTransform.parent.position, hitInfo.point) < -(camDistance.z + crouchOffset.z) && Vector3.Distance(cameraTransform.parent.position, hitInfo.point) < -wallDis)
        {
            wallDis = -Vector3.Distance(cameraTransform.parent.position, hitInfo.point);
            if (wallDis < -(camDistance.z + crouchOffset.z) && !zoom && !isZoomChanging)
            {
                camDistance.z = Mathf.Clamp(camDistance.z -(wallDis + camDistance.z + crouchOffset.z), camMax, camMin);
            }
        }
        else
        {
            if (wallDis > -10) wallDis -= Time.deltaTime;
        }
        cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, crouchOffset.y + camDistance.y + 0.6f, Mathf.Max(wallDis, camDistance.z + crouchOffset.z));
    }
    public void CamXPos(bool pos)
    {
        camPos = pos;
    }
    public void CamZPos(float z)
    {
        camDistance.z = z;
    }
    enum RecoilLevel
    {
        Off,
        Up,
        Down
    }
    RecoilLevel recoilLevel = RecoilLevel.Off;
    public void RecoilSet()
    {
        recoilLevel = RecoilLevel.Up;
    }
    float recoverySpeed = 0;
    void Recoil()
    {
        if (recoilLevel == RecoilLevel.Off) return;
        else if (recoilLevel == RecoilLevel.Up)
        {
            cameraTransform.Rotate(Vector3.right * 20 * Mathf.DeltaAngle(cameraTransform.localEulerAngles.x, -2) * Time.deltaTime);
            if (cameraTransform.localEulerAngles.x <= 359 )
            {
                recoverySpeed = 0;
                recoilLevel = RecoilLevel.Down;
            }
        }
        else
        {
            recoverySpeed += Time.deltaTime * 10;
            cameraTransform.Rotate(Vector3.right * recoverySpeed * Mathf.DeltaAngle(cameraTransform.localEulerAngles.x, 0) * Time.deltaTime);
        }
        if (cameraTransform.localEulerAngles.x == 0) recoilLevel = RecoilLevel.Off;
    }

    void CursorSet()
    {
        // ESC를 누르면 커서를 다시 보이게 하고 잠금을 해제합니다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // 마우스 왼쪽 버튼을 누르면 커서를 다시 숨기고 잠급니다.
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
