using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using static PlayerMove;
//using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.Rendering.DebugUI;

public class PlayerMove : MonoBehaviour
{
    // 걷기 최대속도
    public float walkMaxSpeed = 2;
    // 걷기 가속도
    public float walkAcceleration = 3;
    // 뛰기 최대속도
    public float runMaxSpeed = 4;
    // 줌 뛰기 최대속도
    float zoomRunMaxSpeed = 2.7f;
    // 뛰기 가속도
    public float runAcceleration = 4;
    // 회전 속도
    public float maxTurnSpeed = 250;
    public float turnSpeed = 0;
    // 줌 회전 속도
    float zoomTurnSpeed = 50;

    // 현재 스피드
    public float speed = 0;
    // 현재 스피드 벡터
    public Vector3 speedVector = Vector3.zero;

    // 캐릭터 컨트롤러 컴포넌트 가져오기
    CharacterController cc;

    // 중력
    public float gravity = -10;
    // 수직속력
    float yVelocity = 0;
    // 점프파워
    float jumpPower = 5;

    // HP Slider
    public Slider hpSlider;
    // HP Text
    public Text hpText;
    // transform
    public Transform bodyTransform;
    Transform cameraAxisTransform;
    CamMove camMove;
    MiniMap miniMap;

    PlayerStatus playerStatus;
    PlayerFire playerFire;
    public WaistAngle waistAngle;
    DetectWall detectWall;
    GameObject wall;
    Transform rayAxis;
    Transform rayLeftPos;
    Transform rayRightPos;

    // 발소리
    public AudioClip walkSound;
    float footSoundTimer;
    AudioSource audioSource;

    // Animator
    public Animator anim;

    public enum PlayerState
    {
        Stand,
        Crouch
    }
    public PlayerState state;

    // 엄폐
    public enum HideState
    {
        Off,
        Approaching,
        Arrived,
        HoldOut
    }
    public HideState hideState;


    //상태를 변경할 때 한번 실행되는 함수
    void ChangeState(PlayerState s)
    {
        if (s == state) return;
        //현재상태를 s값으로 셋팅한다
        state = s;
        //각 상태에 따른 초기화를 해주자
        if (state == PlayerState.Stand)
        {
            anim.SetTrigger("Stand");
            camMove.crouchCamSpeed = 0; 
        }
        else if (state == PlayerState.Crouch)
        {
            anim.SetTrigger("Crouch");
            camMove.crouchCamSpeed = 0;
        }
    }

    void ChangeHideState(HideState s)
    {
        if (s == hideState) return;
        hideState = s;
        if (hideState == HideState.Off)
        {
            AimDotUI.instance.IsHide = false;
            camMove.CamXPos(true);
            turnSpeed = 0;
        }
        else turnSpeed = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        // 캐릭터 컨트롤러 컴포넌트 가져오자
        cc = GetComponent<CharacterController>();
        // 자식 엔티티 가져오기
        bodyTransform = transform.Find("Body");
        cameraAxisTransform = transform.Find("CameraAxis");
        // 컴포넌트 가져오기
        camMove = cameraAxisTransform.GetComponent<CamMove>();
        playerStatus = GetComponent<PlayerStatus>();
        playerFire = GetComponent<PlayerFire>();
        detectWall = transform.Find("FindWallRange").GetComponent<DetectWall>();
        rayAxis = transform.Find("RayAxis");
        rayLeftPos = transform.Find("RayAxis/rayLeftPos");
        rayRightPos = transform.Find("RayAxis/rayRightPos");
        miniMap = GameObject.Find("MiniMap/MiniMapMask/Axis/MiniMap").GetComponent<MiniMap>();
        audioSource = GetComponent<AudioSource>();
    }

    // 최대 스피드
    float maxSpeed;
    // 가속도
    float acceleration;
    // 카메라의 y축 값
    float cameraY;
    // 캐릭터의 y축 값
    float playerY;

    bool run = false;
    float h;
    float v;
    // Update is called once per frame
    void Update()
    {
        if (!playerStatus.life) return;
        // 사용자 입력(w,s,a,d, shift)
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        maxSpeed = 0;
        acceleration = walkAcceleration;
        // 카메라의 y축 값
        cameraY = cameraAxisTransform.rotation.eulerAngles.y;
        // 캐릭터의 y축 값
        playerY = bodyTransform.rotation.eulerAngles.y;
        // 땅감지
        bool isGrounded = GetComponent<CharacterController>().isGrounded;

        // 엄폐 입력 감지
        bool hide = Input.GetKeyDown(KeyCode.Q);

        if (hideState == HideState.Off)
        {
            // 이동 방향
            // 현재 이동 방향(Local)
            float movingAngle = Mathf.Atan2(speedVector.x, speedVector.z) * Mathf.Rad2Deg;
            float movingDirection = Mathf.DeltaAngle(playerY, movingAngle);

            // 뛰기
            if (Input.GetButtonDown("Run"))
            {
                run = true;
                AimDotUI.instance.Run = true;
            }
            else if (Input.GetButtonUp("Run"))
            {
                run = false;
                AimDotUI.instance.Run = false;
            }
            if (run)
            {
                if (speed > 2) miniMap.RunScale(true);
                if (state == PlayerState.Crouch)
                {
                    //ChangeState(PlayerState.Stand);
                }
            }
            else
            {
                miniMap.RunScale(false);
            }

            // 줌 변경 시 플레이어 포지션 변경
            if (camMove.isZoomChanging)
            {
                ZoomChanging();
            }
            // 줌 Off시 (앞으로만 걸음)
            else if (!camMove.zoom)
            {
                ZoomOffCtrl(run);
            }
            // 줌 On시 or 변경중 (4방향으로 걸음)
            if (camMove.zoom || camMove.isZoomChanging)
            {
                ZoomOnCtrl(movingAngle, run);
            }

            // 점프
            //if (isGrounded && Input.GetButtonDown("Jump") && !camMove.zoom && !camMove.isZoomChanging)
            //{
            //    if (!camMove.zoom)
            //    {
            //        // yVelocity 를 jumpPower 한다.
            //        yVelocity = jumpPower;
            //    }
            //}
            if (hide) FindWall();
        }
        else if (hideState == HideState.Approaching)
        {
            miniMap.RunScale(false);
            Approaching();
            if (hide || Input.GetButtonDown("Run"))
            {
                ChangeHideState(HideState.Off);
            }
        }
        else if (hideState == HideState.Arrived)
        {
            HidedMoving();
            if (hide)
            {
                ChangeHideState(HideState.Off);
            }
        }
        else if (hideState == HideState.HoldOut)
        {
            HoldOut();
            if (hide)
            {
                ChangeHideState(HideState.Off);
            }
        }
        // 앉기
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {   
            if (state == PlayerState.Stand)
            {
                ChangeState(PlayerState.Crouch);
            }
            else if (state == PlayerState.Crouch)
            {
                ChangeState(PlayerState.Stand);
            }
        }
        AimDotUI.instance.Speed = speed;
        // yVelocity 값을 점점 줄여준다. (중력에 의해서)
        if (!isGrounded) yVelocity += gravity * Time.deltaTime;
        // 플레이어 벡터에 yVelocity 더함
        speedVector.y += yVelocity;

        // 플레이어 방향 기준 속도 벡터
        Vector3 localVector = Quaternion.Euler(0, -playerY, 0) * new Vector3(speedVector.x, 0, speedVector.z);
        // 애니메이션 파라미터 조정
        anim.SetFloat("velocityX", localVector.x);
        anim.SetFloat("velocityZ", localVector.z);

        cc.Move(speedVector * Time.deltaTime);
        Transform avatar = transform.GetChild(0).GetChild(0);

        // 발소리
        if (h != 0 || v != 0)
        {
            footSoundTimer += Time.deltaTime;
            if (footSoundTimer > 1 / (speed * 0.6f + 0.7f))
            {
                if (speed > walkMaxSpeed)
                {
                    audioSource.PlayOneShot(walkSound, 1.5f);
                }
                else
                {
                    audioSource.PlayOneShot(walkSound, 1.5f);
                }
                footSoundTimer = 0;
            }
        }
    }
    bool targetFound = false;
    Vector3 targetPos;
    void FindWall()
    {
        // 범위 내 가장 가까운 벽 탐지
        GameObject[] walls = detectWall.GetInRangeEntities();
        if (walls.Length != 0)
        {
            targetFound = false;
            wall = walls[0];
            ChangeHideState(HideState.Approaching);
            camMove.camSpeed = 0;
            rayAxis.eulerAngles = new Vector3(0, cameraAxisTransform.eulerAngles.y, 0);
            AimDotUI.instance.IsHide = true;
        }
        
    }
    // 몸 방향 F : 왼, T : 오
    bool bodyDir = false;
    bool sit = false;
    void Approaching()
    {   // 벽 감지
        if (!targetFound)
        {
            // Ray 생성
            Ray rayLeft = new Ray(rayLeftPos.position, rayLeftPos.forward);
            Ray rayRight = new Ray(rayRightPos.position, rayRightPos.forward);
            // Ray를 발사해서 어딘가에 맞았다면
            RaycastHit hitInfoLeft = new RaycastHit();
            RaycastHit hitInfoRight = new RaycastHit();
            // 벽 감지
            if (Physics.Raycast(rayLeft, out hitInfoLeft, 4, 1 << LayerMask.NameToLayer("Wall")) && Physics.Raycast(rayRight, out hitInfoRight, 4, 1 << LayerMask.NameToLayer("Wall"))
                && Mathf.Abs(hitInfoLeft.distance - hitInfoRight.distance) < 1)
            {   // 감지 성공
                targetFound = true;
                targetPos = (hitInfoLeft.point + hitInfoRight.point) / 2;
                // 앉아야 하는 엄폐물인지 판단
                HeightCheck();
            }
            else
            {
                // ray축 벽 방향으로 회전
                // 플레이어 -> 벽 각도
                Vector3 wallDir = wall.GetComponent<Collider>().ClosestPoint(transform.position) - transform.position;
                float wallAngle = Mathf.Atan2(wallDir.x, wallDir.z) * Mathf.Rad2Deg;
                // 현재 ray축 각도
                float rayAxisAngle = rayAxis.eulerAngles.y;
                // 남은 회전 각도
                float deltaAngle = Mathf.DeltaAngle(rayAxisAngle, wallAngle);
                // 남은 각도가 0보다 크면 시계 방향 회전
                if (deltaAngle > 0)
                {
                    rayAxis.eulerAngles = new Vector3(0, rayAxis.eulerAngles.y + 1000 * Time.deltaTime, 0);
                    bodyDir = false;
                    camMove.CamXPos(false);
                }
                // 남은 각도가 0보다 작으면 반시계 방향 회전
                else
                {
                    rayAxis.eulerAngles = new Vector3(0, rayAxis.eulerAngles.y - 1000 * Time.deltaTime, 0);
                    bodyDir = true;
                    camMove.CamXPos(true);
                }
            }
        }
        else// 벽으로 이동
        {
            if (sit) ChangeState(PlayerState.Crouch);
            // 타겟 방향으로 회전
            // 플레이어 -> 타겟 각도
            Vector3 targetDir = targetPos - transform.position;
            float targetAngle = Mathf.Atan2(targetDir.x, targetDir.z) * Mathf.Rad2Deg;
            // 현재 플레이어의 각도
            float playerAngle = bodyTransform.eulerAngles.y;
            // 남은 회전 각도
            float deltaAngle = Mathf.DeltaAngle(playerAngle, targetAngle);
            if (deltaAngle != 0)
            {
                float value = 600 * Time.deltaTime; // 이번 프레임에 회전할 각도
                if (Mathf.Abs(deltaAngle) <= Mathf.Abs(value))
                {   // 플레이어 각도 = 타겟 각도
                    bodyTransform.eulerAngles = new Vector3(0, targetAngle, 0);
                }
                else
                {
                    // 남은 각도가 0보다 크면 시계 방향 회전
                    if (deltaAngle > 0)
                    {
                        bodyTransform.eulerAngles = new Vector3(0, bodyTransform.eulerAngles.y + 600 * Time.deltaTime, 0);
                    }
                    // 남은 각도가 0보다 작으면 반시계 방향 회전
                    else
                    {
                        bodyTransform.eulerAngles = new Vector3(0, bodyTransform.eulerAngles.y - 600 * Time.deltaTime, 0);
                    }
                }
            }
            WalkFront(true, true, PlayerState.Stand);
            if (Vector3.Distance(new Vector3(bodyTransform.position.x, 0, bodyTransform.position.z), new Vector3(targetPos.x, 0, targetPos.z)) < 1.5f)
            {
                ChangeHideState(HideState.Arrived);
            }
        }
    }
    // 모서리 줌 하기 이전 위치
    Vector3 hidePos;
    // 모서리 줌 방향 F : 왼, T : 오
    bool zoomDir = false;
    public bool leftHit;
    public bool rightHit;
    void HidedMoving()
    {
        HeightCheck();
        Ray rayLeft = new Ray(rayLeftPos.position, rayLeftPos.forward);
        Ray rayRight = new Ray(rayRightPos.position, rayRightPos.forward);
        // Ray를 발사해서 어딘가에 맞았다면
        RaycastHit hitInfoLeft = new RaycastHit();
        RaycastHit hitInfoRight = new RaycastHit();
        // 0 : 왼쪽 끝, 1 : 중간, 2 : 오른쪽 끝
        int senseState = 1;
        // 벽 감지
        float playerWallDis;
        leftHit = Physics.Raycast(rayLeft, out hitInfoLeft, 2, 1 << LayerMask.NameToLayer("Wall"));
        rightHit = Physics.Raycast(rayRight, out hitInfoRight, 2, 1 << LayerMask.NameToLayer("Wall"));
        
        // 모서리 줌
        if ((leftHit ^ rightHit) && camMove.zoom)
        {
            if (leftHit) zoomDir = true;
            else
            {
                zoomDir = false;
            }
            hidePos = transform.position;
            ChangeHideState(HideState.HoldOut);
            return;
        }
        else if (camMove.zoom)
        {   // 줌할 때 일어서기
            ChangeState(PlayerState.Stand);
        }
        else
        {
            if (sit) ChangeState(PlayerState.Crouch);
        }
        if (leftHit)
        {   
            if (rightHit)
            {
                float leftDistance = Vector3.Distance(rayLeftPos.position, hitInfoLeft.point);
                float rightDistance = Vector3.Distance(rayRightPos.position, hitInfoRight.point);
                if (leftDistance != rightDistance)
                {
                    // 보정할 각도
                    float deltaDegree = Mathf.Atan2(leftDistance - rightDistance, 1) * Mathf.Rad2Deg;
                    rayAxis.transform.eulerAngles += new Vector3(0, deltaDegree, 0);
                }
                playerWallDis = Mathf.Min(leftDistance, rightDistance);
            }
            else
            {   // 오른쪽 없을 때
                senseState = 2;
                playerWallDis = Vector3.Distance(rayLeftPos.position, hitInfoLeft.point);
            }
        }
        else
        {   
            if (rightHit)
            {   // 왼쪽 없을 때
                senseState = 0;
                playerWallDis = Vector3.Distance(rayRightPos.position, hitInfoRight.point);
            }
            else
            {   // 양쪽 감지 안됐을 때
                ChangeHideState(HideState.Off);
                return;
            }
        }
        bool move = false;
        if ((h != 0 || v != 0) && !camMove.zoom)
        {
            maxSpeed = runMaxSpeed;
            acceleration = 10;
            // wasd입력을 y축 기준 각도로 변환
            float inputAngle = Mathf.Atan2(h, v) * Mathf.Rad2Deg;
            // 현재 플레이어 각도 -> 카메라 각도 기준 wasd 입력 각도
            float targetAngle = cameraY + inputAngle;
            // 벽 기준 이동 각도
            float deltaAngle = Mathf.DeltaAngle(rayAxis.eulerAngles.y, targetAngle);
            // 이동할 각도
            float moveAngle = rayAxis.eulerAngles.y;
            if (deltaAngle > 45 && deltaAngle < 135 && senseState <= 1)
            {   // 오른쪽 이동
                moveAngle += 90;
                speedVector += Quaternion.Euler(0, moveAngle, 0) * new Vector3(0, 0, acceleration) * Time.deltaTime;
                speed = new Vector3(speedVector.x, 0, speedVector.z).magnitude;
                move = true;
                bodyDir = true;
                camMove.CamXPos(true);
            }
            else if (deltaAngle > -135 && deltaAngle < -45 && senseState >= 1)
            {   // 왼쪽 이동
                moveAngle -= 90;
                speedVector += Quaternion.Euler(0, moveAngle, 0) * new Vector3(0, 0, acceleration) * Time.deltaTime;
                speed = new Vector3(speedVector.x, 0, speedVector.z).magnitude;
                move = true;
                bodyDir = false;
                camMove.CamXPos(false);
            }
            // 벽과 먼 방향 입력 / 달리기 입력 시 엄폐 해제
            else if (Mathf.Abs(deltaAngle) >= 135)
            {
                ChangeHideState(HideState.Off);
                return;
            }
            if (Input.GetButton("Run"))
            {
                ChangeHideState(HideState.Off);
                run = true;
                return;
            }
        }
        // 몸 방향 회전
        if (camMove.isZoomChanging && camMove.zoom)
        {
            ZoomChanging();
        }
        if (!camMove.zoom)
        {
            float target;
            float digree;
            if (bodyDir)
            {   // 오른쪽
                target = rayAxis.eulerAngles.y + 93;
                digree = Mathf.DeltaAngle(bodyTransform.eulerAngles.y, target);
            }
            else
            {   // 왼쪽
                target = rayAxis.eulerAngles.y - 93;
                digree = Mathf.DeltaAngle(bodyTransform.eulerAngles.y, target);
            }
            if (digree > 1)
            {   // 회전 가속
                if (Mathf.Abs(turnSpeed) < maxTurnSpeed) turnSpeed += Time.deltaTime * 1000;
                bodyTransform.eulerAngles += new Vector3(0, turnSpeed * Time.deltaTime, 0);
            }
            else if (digree < -1)
            {   // 회전 가속
                if (Mathf.Abs(turnSpeed) < maxTurnSpeed) turnSpeed -= Time.deltaTime * 1000;
                bodyTransform.eulerAngles += new Vector3(0, turnSpeed * Time.deltaTime, 0);
            }
            if (Mathf.Abs(turnSpeed) / 3 > Mathf.Abs(digree))
            {   // 회전 감속
                turnSpeed = digree * 3;
            }
        }
        // 줌 시
        // 플레이어 방향 카메라 방향으로 고정
        else if (!playerFire.deadEyeShooting && !camMove.isZoomChanging)
        {
            bodyTransform.eulerAngles = new Vector3(0, cameraY, 0);
        }


        // 감속 - ( 속도 > 최대속도 ) 일 때
        if ((speed != 0 && speed > maxSpeed) || !move)
        {
            // (뜀 속도 ~ 걷는 속도) : 현재 속도 비례 감속
            if (speed > walkAcceleration)
            {
                speedVector -= Quaternion.Euler(0, Mathf.Atan2(speedVector.x, speedVector.z) * Mathf.Rad2Deg, 0) * new Vector3(0, 0, 20) * Time.deltaTime;
                speed = new Vector3(speedVector.x, 0, speedVector.z).magnitude;
            }
            // (걷는 속도 ~ 멈춤) : 걷기 가속도로 감속
            else
            {
                // 감속할 값
                Vector3 value = Quaternion.Euler(0, Mathf.Atan2(speedVector.x, speedVector.z) * Mathf.Rad2Deg, 0) * new Vector3(0, 0, 20) * Time.deltaTime;
                float mag = value.magnitude;
                // 감속할 값이 남은 값보다 크면 0으로 변경
                if (mag > speed)
                {
                    speedVector = Vector3.zero;
                    speed = 0;
                }
                else // 아니라면 값만큼 뺌
                {
                    speedVector -= value;
                    speed = new Vector3(speedVector.x, 0, speedVector.z).magnitude;
                }
            }
        }

        // 벽과의 거리 조절
        if (playerWallDis > 0.5f)
        {   // 멀 때
            cc.Move(Quaternion.Euler(0, rayAxis.eulerAngles.y, 0) * Vector3.forward * (playerWallDis - 0.5f) * 4 * Time.deltaTime);
        }
        else if (playerWallDis < 0.4f)
        {   // 가까울 때
            cc.Move(Quaternion.Euler(0, rayAxis.eulerAngles.y, 0) * Vector3.back * (0.4f - playerWallDis) * 4 * Time.deltaTime);
        }

    }
    void HoldOut()
    {
        HeightCheck();
        maxSpeed = runMaxSpeed;
        acceleration = 20;
        if (camMove.isZoomChanging && camMove.zoom) ZoomChanging();
        else bodyTransform.eulerAngles = new Vector3(0, cameraY, 0);
        if (camMove.zoom)
        {
            //ChangeState(PlayerState.Stand);
            if (Vector3.Distance(hidePos, transform.position) < 0.3f)
            {
                if (!zoomDir)
                {   // 왼쪽 줌
                    speedVector += Quaternion.Euler(0, rayAxis.eulerAngles.y - 90, 0) * new Vector3(0, 0, acceleration) * Time.deltaTime;
                }
                else
                {   // 오른쪽 줌
                    speedVector += Quaternion.Euler(0, rayAxis.eulerAngles.y + 90, 0) * new Vector3(0, 0, acceleration) * Time.deltaTime;
                }
                speed = new Vector3(speedVector.x, 0, speedVector.z).magnitude;
            }
            // 감속 - ( 속도 > 최대속도 ) 일 때
            else if ((speed != 0))
            {
                // 감속할 값
                Vector3 value = Quaternion.Euler(0, Mathf.Atan2(speedVector.x, speedVector.z) * Mathf.Rad2Deg, 0) * new Vector3(0, 0, 20) * Time.deltaTime;
                float mag = value.magnitude;
                // 감속할 값이 남은 값보다 크면 0으로 변경
                if (mag > speed)
                {
                    speedVector = Vector3.zero;
                    speed = 0;
                }
                else // 아니라면 값만큼 뺌
                {
                    speedVector -= value;
                    speed = new Vector3(speedVector.x, 0, speedVector.z).magnitude;
                }
            }
        }
        else
        {
            //if (sit) ChangeState(PlayerState.Crouch);
            Vector3 targetDir = hidePos - transform.position;
            float targetAngle = Mathf.Atan2(targetDir.x, targetDir.z) * Mathf.Rad2Deg;
            speedVector += Quaternion.Euler(0, targetAngle, 0) * new Vector3(0, 0, acceleration) * Time.deltaTime;
            speed = new Vector3(speedVector.x, 0, speedVector.z).magnitude;
            // 오른쪽 회전
            if (zoomDir)
            {
                bodyTransform.eulerAngles = new Vector3(0, playerY + maxTurnSpeed * Time.deltaTime, 0);
            }
            // 왼쪽 회전
            else
            {
                bodyTransform.eulerAngles = new Vector3(0, playerY - maxTurnSpeed * Time.deltaTime, 0);
            }
            // 다 돌아왔는지 체크
            if (Vector3.Distance(hidePos, transform.position) < 0.34f)
            {
                ChangeHideState(HideState.Arrived);
            }
        }
    }
    void ZoomChanging()
    {
        float targetAngle; // 몸 회전 목표각도
        float deltaAngle; // 남은 회전 각도
        float movingAngle = Mathf.Atan2(speedVector.x, speedVector.z) * Mathf.Rad2Deg; // 현재 이동 방향

        if (camMove.zoom) // 줌 On
        {   // 카메라 방향
            targetAngle = cameraY;
        }
        else // 줌 Off
        {   // 이동 방향
            if (speed != 0)
            {
                targetAngle = movingAngle;
            }
            else
            {
                targetAngle = playerY;
            }
        }
        // 플레이어를 타겟 각도로 회전
        deltaAngle = Mathf.DeltaAngle(playerY, targetAngle);
        if (deltaAngle != 0)
        {
            // 회전속도 가속
            if (Mathf.Abs(deltaAngle) > 170) turnSpeed += Time.deltaTime / Time.timeScale * -1 * (2000 + 40 * Mathf.Abs(deltaAngle));
            else turnSpeed += Time.deltaTime / Time.timeScale * Mathf.Sign(deltaAngle) * (2000 + 40 * Mathf.Abs(deltaAngle));
            if (Mathf.Abs(turnSpeed) / 20 > Mathf.Abs(deltaAngle))
            {
                turnSpeed = deltaAngle * 20;
            }
            if (Mathf.Abs(turnSpeed) > 1000) turnSpeed = 1000 * Mathf.Sign(turnSpeed);
            // 회전할 각도가 남은 각도보다 크면
            if (Mathf.Abs(deltaAngle) <= Mathf.Abs(turnSpeed * Time.deltaTime / Time.timeScale))
            {   // 플레이어 각도 = 타겟 각도
                bodyTransform.eulerAngles = new Vector3(0, targetAngle, 0);
            }
            else
            {
                bodyTransform.eulerAngles = new Vector3(0, playerY + turnSpeed * Time.deltaTime / Time.timeScale, 0);
            }
        }
        // 플레이어 이동 벡터 변경
        speedVector = Quaternion.Euler(0, movingAngle, 0) * new Vector3(0, 0, speed);
    }
    void ZoomOffCtrl(bool run)
    {
        // wasd 입력이 있을 때
        bool accel = false;
        if (h != 0 || v != 0)
        {
            accel = true;
            // 플레이어를 카메라 기준 wasd입력 방향으로 회전
            // angle : wasd입력을 y축 기준 각도로 변환
            float angle = Mathf.Atan2(h, v) * Mathf.Rad2Deg;
            // (현재 플레이어 각도 -> 카메라 각도 기준 wasd 입력 각도) 로 남은 회전 각도
            float targetAngle = cameraY + angle;
            float deltaAngle = Mathf.DeltaAngle(playerY, targetAngle);
            if (deltaAngle != 0)
            {
                // 회전속도 가속
                turnSpeed += Time.deltaTime * Mathf.Sign(deltaAngle) * 400;
                // 남은 각도에 비례한 최대 회전속도 제한 (목표 방향 도달시 감속을 위함)
                if (Mathf.Abs(turnSpeed) / 3 > Mathf.Abs(deltaAngle))
                {
                    turnSpeed = deltaAngle * 3;
                }
                // 최대속도 제한
                if (Mathf.Abs(turnSpeed) > maxTurnSpeed) turnSpeed = maxTurnSpeed * Mathf.Sign(turnSpeed);
                // 회전할 각도가 남은 각도보다 크면
                if (Mathf.Abs(deltaAngle) <= Mathf.Abs(turnSpeed * Time.deltaTime))
                {   // 플레이어 각도 -> 타겟 각도 회전
                    bodyTransform.eulerAngles += new Vector3(0, Mathf.DeltaAngle(bodyTransform.eulerAngles.y, targetAngle) * 5 * Time.deltaTime, 0);
                }
                else
                {
                    bodyTransform.eulerAngles = new Vector3(0, playerY + turnSpeed * Time.deltaTime, 0);
                }
            }
        }
        else
        {   // 미입력시 회전속도 감속
            turnSpeed -= 10 * turnSpeed * Time.deltaTime;
        }
        WalkFront(run, accel, state);
    }
    void WalkFront(bool run, bool accel, PlayerState state)
    {
        if (accel)
        {
            if (run)
            {   // 뛸 때 최대 속력 / 가속도
                if (state == PlayerState.Stand)
                {   // 서있을 때
                    maxSpeed = runMaxSpeed;
                    acceleration = runAcceleration;
                }
                else
                {   // 앉아있을 때
                    maxSpeed = runMaxSpeed * 3/4;
                    acceleration = runAcceleration * 3/4;
                }
            }
            else
            {   // 걸을 때 최대 속력
                maxSpeed = walkMaxSpeed;
            }
            // 가속
            if (speed < maxSpeed)
            {
                speed += acceleration * Time.deltaTime;
            }
        }
        // 감속 - ( 속도 > 최대속도 ) 일 때
        if (speed != 0 && speed > maxSpeed)
        {   // (뜀 속도 ~ 걷는 속도) : 현재 속도 비례 감속
            if (speed > walkAcceleration)
            {
                speed -= speed * Time.deltaTime * 3;
            }
            // (걷는 속도 ~ 멈춤) : 걷기 가속도로 감속
            else
            {
                speed -= walkAcceleration * Time.deltaTime;
                if (speed < 0) speed = 0;
            }
        }
        // 플레이어에게 가해질 벡터 값 : speed값 -> 플레이어 전방 벡터로 변환
        speedVector = Quaternion.Euler(0, playerY, 0) * new Vector3(0, 0, speed);
    }
    void ZoomOnCtrl(float movingAngle, bool run)
    {
        // 플레이어 방향 카메라 방향으로 고정
        if (!camMove.isZoomChanging && !playerFire.deadEyeShooting)
        {
            bodyTransform.eulerAngles = new Vector3(0, cameraY, 0);
        }

        // angle : wasd입력을 y축 기준 각도로 변환
        float angle = Mathf.Atan2(h, v) * Mathf.Rad2Deg;
        // 현재 플레이어 각도 -> 카메라 각도 기준 wasd 입력 각도
        float targetAngle = cameraY + angle;

        // wasd 입력이 있을 때
        if (h != 0 || v != 0)
        {
            // 진행각과 입력각의 차이
            float accelAngle = Mathf.Abs(Mathf.DeltaAngle(movingAngle, targetAngle));
            

            if (run && state == PlayerState.Stand)
            {   // 뛸 때 최대 속력 / 가속도
                maxSpeed = zoomRunMaxSpeed;
            }
            else
            {   // 걸을 때 최대 속력 / 가속도
                maxSpeed = walkMaxSpeed;
            }
            if (accelAngle > 90)
            {
                acceleration = 15;
            }
            else
            {
                acceleration = runAcceleration;
            }
            // 가속
            if (speed < maxSpeed)
            {
                speedVector += Quaternion.Euler(0, targetAngle, 0) * new Vector3(0, 0, acceleration) * Time.deltaTime;
                speed = new Vector3(speedVector.x, 0, speedVector.z).magnitude;
            }
        }
        // 감속 - ( 속도 > 최대속도 ) 일 때
        if (speed != 0 && speed > maxSpeed)
        {
            // (뜀 속도 ~ 걷는 속도) : 현재 속도 비례 감속
            if (speed > walkAcceleration)
            {
                speedVector -= Quaternion.Euler(0, movingAngle, 0) * new Vector3(0, 0, speed) * Time.deltaTime;
                speed = new Vector3(speedVector.x, 0, speedVector.z).magnitude;
            }
            // (걷는 속도 ~ 멈춤) : 걷기 가속도로 감속
            else
            {
                // 감속할 값
                Vector3 value = Quaternion.Euler(0, movingAngle, 0) * new Vector3(0, 0, acceleration) * Time.deltaTime;
                float mag = value.magnitude;
                // 감속할 값이 남은 값보다 크면 0으로 변경
                if (mag > speed)
                {
                    speedVector = Vector3.zero;
                    speed = 0;
                }
                else // 아니라면 값만큼 뺌
                {
                    speedVector -= value;
                    speed = new Vector3(speedVector.x, 0, speedVector.z).magnitude;
                }
            }
        }
    }
    void HeightCheck()
    {
        // 앉아야 하는 엄폐물인지 판단
        Ray left = new Ray(rayLeftPos.position + Vector3.up * 1.2f, rayAxis.forward);
        Ray right = new Ray(rayRightPos.position + Vector3.up * 1.2f, rayAxis.forward);
        RaycastHit hitInfoLeft = new RaycastHit();
        RaycastHit hitInfoRight = new RaycastHit();
        bool isLeft = Physics.Raycast(left, out hitInfoLeft, 4, 1 << LayerMask.NameToLayer("Wall"));
        bool isRight = Physics.Raycast(right, out hitInfoRight, 4, 1 << LayerMask.NameToLayer("Wall"));
        if (isLeft || isRight) sit = false;
        else sit = true;
    }
}