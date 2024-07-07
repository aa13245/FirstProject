using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.UI;

public class EnemyBehavior : MonoBehaviour
{
    // 타겟 설정 (목표 지점)
    public Transform PatrolRoute;
    // 순찰 위치 목록
    public List<Transform> Locations;
    private int _locationIndex = 0;
    private NavMeshAgent _agent;

    GameObject enemy;

    // player 의 Transform 값 저장
    Transform player;
    public GameObject firePos;
    public GameObject bulletFactory;

    public Slider hpUI;

    public GameObject shotTracerPrefab;

    AudioSource audioSource;

    public enum EnemyState
    {
        Idle,
        Attack,
        TakeDamage,
        Die
    }

    public EnemyState state;

    // 스피드
    public float speed = 1;
    // 현재 체력
    public float currHP;
    // Max HP
    public float maxHP;

    // 발견 범위 거리
    public float findDistance = 8f;
    // 공격 범위
    public float attackDistance = 5f;
    // 정지 거리
    public float stopDistance = 2f;
    // 공격 딜레이 시간
    public float attackDelayTIme = 2f;
    // 공격 타이머
    float attackTimer = 0;
    // 피격 시간
    public float damagedDelayTime = 2f;

    // 현재 시간
    public float currTime;
    // 죽는 시간
    public float dieDelayTIme = 2f;
    // 플레이어의 마지막 알려진 위치
    private Vector3 lastKnownPosition;
    // 에너미 목적지 설정
    Vector3 pos;

    void Start()
    {
        player = GameObject.Find("Player").transform;
        // 현재 HP를 최대 HP로 설정
        currHP = maxHP;

        lastKnownPosition = Vector3.zero;

        _agent = GetComponent<NavMeshAgent>();

        InitializePatrolRoute();

        MoveToNextPatrolLocation();

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // NavMeshComp 설정된 대상에서 얼마나 떨어져 있고, 이 값이 0.2보다 작은지
        // pathPending -> bool값을 반환
        // 에이전트가 목표 지점에 도착했는지 확인 후 -> 다음 순찰 위치로 이동
        if (_agent.enabled && _agent.remainingDistance < 0.2f && !_agent.pathPending)
        {
            MoveToNextPatrolLocation();
        }

        // 에너미의 상태에 따른 조건
        if (state == EnemyState.Attack)
        {
            UpdateAttack();
        }
        else if (state == EnemyState.Idle)
        {
            UpdateIdle();
        }
        else if (state == EnemyState.TakeDamage)
        {
            TakeDamage(player.transform.position);
        }
        else if (state == EnemyState.Die)
        {
            UpdateDie();
        }
    }

    // 대기 함수
    public void UpdateIdle()
    {
        //플레이어와의 거리가 발견 범위 안에 들어 왔다면 플레이어를 바라보며 멈추기
        float dist = Vector3.Distance(player.transform.position, transform.position);
        if (dist < findDistance)
        {
            // 플레이어를 바라보기
            Vector3 directionToPlayer = player.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            UpdateAttack();
        }
    }

    // 공격 함수
    public void UpdateAttack()
    {
        float dist = Vector3.Distance(player.transform.position, transform.position);
        // 플레이어와의 거리가 공격 가능 범위 안에 들어왔다면
        // 플레이어를 바라보며 정지한 상태로 변환 후 공격 -> 플레이어가 멀어지면 추격
        if (dist < attackDistance)
        {
            attackTimer += Time.deltaTime;

            // 공격 지연 시간이 경과했을 때
            if (attackTimer >= attackDelayTIme)
            {
                attackTimer = 0;

                // NavMesh 멈추고 공격하라.
                _agent.isStopped = true;
                print("공격!");
                TakeDamage(player.transform.position);
            }
        }
        else
        {
            // 플레이어와의 거리가 발견 범위보다 작다면
            if (dist < findDistance)
            {
                lastKnownPosition = player.transform.position;
            }
            _agent.isStopped = false;
            _agent.destination = lastKnownPosition;
            //_agent.SetDestination(lastKnownPosition);

            /*
            // 플레이어가 공격 범위를 벗어났을 때
            if (lastKnownPosition != Vector3.zero)
            {
                // NavMesh 다시 활성화 하고 플레이어 따라가기 // 장애물에 숨으면 찾아오면서 공격하기까지
                _agent.isStopped = false;
                _agent.SetDestination(lastKnownPosition);
            }
            attackTimer = 0;
            */


            //if (_agent.destination != player.transform.position)
            //{
            //    _agent.destination = lastKnownPosition;
            //}
        }
    }

    // 데미지 함수
    public void TakeDamage(Vector3 aimPos)
    {
        // 레이캐스트를 사용하여 플레이어 공격
        Ray ray = new Ray(firePos.transform.position, aimPos - firePos.transform.position);
        RaycastHit hitInfo = new RaycastHit();

        // Ray를 발사해서 맞았다면
        if (Physics.Raycast(ray, out hitInfo))
        {
            // 파편효과 공장에서 파편 효과를 만든다.
            GameObject bullet = Instantiate(bulletFactory);
            // 맞은 위치에 두기 
            bullet.transform.position = hitInfo.point;
            // 만든 파편효과의 앞방향을 맞은 위치의 normal 값으로 셋팅한다.
            bullet.transform.forward = hitInfo.normal;
            // 만든 파편효과를 3초뒤에 파괴하자
            Destroy(bullet, 2);
            // 총 궤적 효과
            ShotTracer(hitInfo.point - firePos.transform.position, Vector3.Distance(transform.position, hitInfo.point));
            // 총 사운드
            audioSource.PlayOneShot(audioSource.clip);

            // 맞은 대상이 Player 라면
            if (hitInfo.transform.gameObject.name.Contains("Player"))
            {
                // Player 에게 데미지를 주자
                // Player 에서 Enemy 컴포넌트를 가져오자
                PlayerStatus ph = hitInfo.transform.GetComponent<PlayerStatus>();
                // 가져온 컴포넌트에서 Damaged 함수를 호출
                if (ph != null)
                {
                    ph.Damaged(2);
                }
            }
        }
    }

    // 피격 함수
    public void OnDamaged(float damage)
    {
        if (state == EnemyState.Die)
        {
            return;
        }

        currHP -= damage;

        // HP바를 갱신하자.
        float ratio = currHP / maxHP;
        hpUI.value = ratio;

        // 체력이 0 이상일 때 데미지 상태
        if (currHP > 0)
        {
            //state = EnemyState.TakeDamage;
        }
        // 에너미의 체력이 0 이하로 떨어지면 Die 상태로 전환
        else
        {
            state = EnemyState.Die;
        }
    }

    // 사망 함수
    public void UpdateDie()
    {
        currTime += Time.deltaTime;
        // 현재 시간이 죽는 시간보다 커진다면
        if (currTime > dieDelayTIme)
        {
            // 네비게이션 시스템 비활성화
            _agent.enabled = false;
            // 캡슐 콜라이더 비활성화
            GetComponent<CapsuleCollider>().enabled = false;
            // 아래 방향으로 움직이게 한다.
            transform.position += Vector3.down * speed * Time.deltaTime;
            // y축 위치가 -2보다 작아질 경우
            if (transform.position.y < -2)
            {
                Destroy(gameObject);
            }
        }
    }

    // 정찰 시작
    public void InitializePatrolRoute()
    {
        foreach (Transform child in PatrolRoute)
        {
            Locations.Add(child);
        }
    }

    // 다음 정찰지로 움직이기
    public void MoveToNextPatrolLocation()
    {
        // 순찰 위치가 없는 경우 반환
        if (Locations.Count == 0)
        {
            return;
        }

        _agent.destination = Locations[_locationIndex].position;
        // 문법 부가
        _locationIndex = (_locationIndex + 1) % Locations.Count;

        UpdateIdle();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            _agent.destination = player.position;
            print("Attack!");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        // dd
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 에너미가 총알에 맞았을 때 
        if (collision.gameObject.name == "Bullet(clone)")
        {
            // 체력을 1 감소시킨다.
            currHP -= 1;
            print("Critical hit");
        }
    }

    public void SetDestination()
    {
        _agent.destination = pos;
    }

    void ShotTracer(Vector3 pos, float distance)
    {
        GameObject shotTracer = Instantiate(shotTracerPrefab);
        shotTracer.transform.position = firePos.transform.position;
        shotTracer.transform.forward = pos;
        shotTracer.transform.Translate(0, 0, distance / 2);
        shotTracer.transform.localScale = new Vector3(1, 1, distance);
    }
}
