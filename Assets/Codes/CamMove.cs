using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;


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
    public float camMax = -4;
    public float camMin = -2;
    // 줌 카메라 거리
    public float camZoom = -1.5f;

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
    float zoomPerSec;

    // playerMove
    PlayerMove playerMove;
    // Transform
    Transform bodyTransform;
    // PlayerFIre
    PlayerFire playerFire;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        playerMove = transform.GetComponentInParent<PlayerMove>();
        playerFire = transform.GetComponentInParent<PlayerFire>();
        bodyTransform = transform.parent.Find("Body");
        cameraTransform = transform.Find("Main Camera");
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
        }

        // 3. 누적된 값을 물체의 회전값으로 셋팅하자
        transform.localEulerAngles = new Vector3(-rotY, rotX);

        if (!zoom)
        {   // 카메라 거리 조절
            if (playerMove.hideState != PlayerMove.HideState.Off)
            {   // 엄폐 시
                if (cameraTransform.localPosition.z < camMin - 0.5f)
                {
                    cameraTransform.Translate(Vector3.forward * 4 * Time.deltaTime);
                }
            }
            else
            {   // 일반 움직임 시
                float player2CamDelta = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, bodyTransform.rotation.eulerAngles.y);
                float speedFromCam = playerMove.speed * Mathf.Cos(player2CamDelta * Mathf.Deg2Rad);
                Vector3 camVecor = new Vector3(0, 0, -speedFromCam * Time.deltaTime);
                if ((camVecor.z < 0 && cameraTransform.localPosition.z > camMax) || (camVecor.z > 0 && cameraTransform.localPosition.z < camMin))
                {
                    cameraTransform.Translate(camVecor);
                }
            }
            
        }

        // 줌
        // 마우스 클릭
        bool mouse = Input.GetMouseButton(1);
        // 줌 활성화
        if (!playerFire.deadEyeOn)
        {
            if (mouse && !zoom)
            {
                Zoom(true);
            }

            // 줌 완료 상태 and 우클릭 해제 시 줌 해제
            else if (zoom && !mouse && !isZoomChanging)
            {
                Zoom(false);
            }
        }

        // 줌 On/Off 시 카메라 이동
        if (isZoomChanging)
        {
            if (zoom)
            {   // 줌 On
                if (camZoom > cameraTransform.localPosition.z)
                {
                    cameraTransform.Translate(new Vector3(0, 0, zoomPerSec * Time.deltaTime / Time.timeScale)); // 카메라 이동
                }
                else isZoomChanging = false; // 완료
            }
            else
            {   // 줌 Off
                if (cameraTransform.localPosition.z > camMin)
                {
                    cameraTransform.Translate(new Vector3(0, 0, zoomPerSec * Time.deltaTime)); // 카메라 이동
                }
                else isZoomChanging = false; // 완료
            }
            
        }
        CamXPos();
    }
    public void Zoom(bool on)
    {
        zoom = on;
        isZoomChanging = true;
        if (on) zoomPerSec = (camZoom - cameraTransform.localPosition.z) / zoomOnTime;
        else zoomPerSec = (camMin - cameraTransform.localPosition.z) / zoomOffTime;
    }
    bool camPos = true;
    void CamXPos()
    {
        if (camPos)
        {
            if (cameraTransform.localPosition.x < 0.5f)
            {
                Vector3 value = Vector3.right * 3 * Time.deltaTime / Time.timeScale;
                if (0.5f - cameraTransform.localPosition.x < value.magnitude)
                {
                    cameraTransform.localPosition = new Vector3(0.5f, cameraTransform.localPosition.y, cameraTransform.localPosition.z);
                }
                else cameraTransform.Translate(value);
            }
        }
        else
        {
            if (cameraTransform.localPosition.x > -0.5f)
            {
                Vector3 value = Vector3.left * 3 * Time.deltaTime / Time.timeScale;
                if (0.5f + cameraTransform.localPosition.x < value.magnitude)
                {
                    cameraTransform.localPosition = new Vector3(-0.5f, cameraTransform.localPosition.y, cameraTransform.localPosition.z);
                }
                else cameraTransform.Translate(value);
            }
        }
    }
    public void CamXPos(bool pos)
    {
        camPos = pos;
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
