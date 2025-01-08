using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.UI;

public class EnemyBehavior : MonoBehaviour
{
    public GameManager gameManager;

    // 정찰 루트 위치
    public Transform PatrolRoute;
    // 적이 이동할 위치 리스트
    public List<Transform> Locations;
    private int _locationIndex = 0;
    // NavMesh 
    private NavMeshAgent _agent;

    public float separationRadius = 2.0f;
    public float separationForce = 10.0f;

    Transform player;
    public GameObject firePos;
    public GameObject bulletFactory;
    public GameObject shotTracerPrefab;
    AudioSource audioSource;
    EnemyMapMark enemyMapMark;

    public enum EnemyState
    {
        Idle,
        Attack,
        TakeDamage,
        Die
    }

    public EnemyState state;

    public float currHP;
    public float maxHP;

    public float findDistance = 8f;
    public float attackDistance = 5f;
    public float attackDelayTIme = 2f;
    float attackTimer = 0;

    private Vector3 lastKnownPosition;
    Vector3 pos;

    public Animator animator;
    AudioManager audioManager;

    void Start()
    {
        player = GameObject.Find("Player").transform;

        currHP = maxHP;

        lastKnownPosition = Vector3.zero;

        _agent = GetComponent<NavMeshAgent>();

        audioSource = GetComponent<AudioSource>();

        animator = GetComponentInChildren<Animator>();

        // 정찰 루트를 배열에 추가해준다
        InitializePatrolRoute();
        // 다음 정찰 루트로 이동하는 배열 함수
        MoveToNextPatrolLocation();

        gameManager = GameManager.FindObjectOfType<GameManager>();

        enemyMapMark = gameObject.GetComponent<EnemyMapMark>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    void Update()
    {
        // 서로 붙지 않게 분리 시켜주는 메소드
        ApplySeparation();

        if (_agent.enabled && _agent.remainingDistance < 0.2f && !_agent.pathPending)
        {
            MoveToNextPatrolLocation();
        }

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
            //TakeDamage(player.transform.position);
        }
        else if (state == EnemyState.Die)
        {
            //UpdateDie();
        }

    }

    public void UpdateIdle()
    {
        float dist = Vector3.Distance(player.transform.position, transform.position);
        // 플레이어와의 거리가 가까워지면
        if (dist < findDistance)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f);

            // 공격함수
            UpdateAttack();

            animator.SetFloat("speed", 1.0f);
        }
    }

    public void UpdateAttack()
    {
        float dist = Vector3.Distance(player.transform.position, transform.position);
        // 플레이어가 공격 가능 범위 안에 들어오면
        if (dist < attackDistance)
        {
            // 시간 누적
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackDelayTIme)
            {
                // 공격 타이머 초기화 후
                attackTimer = 0;
                // 이동을 멈춘다
                _agent.isStopped = true;
                // 플레이어의 위치에 공격 (데미지 함수)
                TakeDamage(player.transform.position);
            }
        }
        // 공격 가능 범위 벗어나면
        else
        {
            // 인지 가능 범위 안에 들어온다면
            if (dist < findDistance)
            {
                // 플레이어에게 이동
                lastKnownPosition = player.transform.position;
            }
            // 이동을 재개한다
            _agent.isStopped = false;
            _agent.destination = lastKnownPosition;
        }
    }

    public void TakeDamage(Vector3 aimPos)
    {
        // 랜덤으로 애니메이터 출력 (둘 중 하나)
        if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Zombie Stand Up" ||
            animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Falling Back Death")
            return;

        // 레이캐스트
        Ray ray = new Ray(firePos.transform.position, aimPos - firePos.transform.position);
        RaycastHit hitInfo = new RaycastHit();

        if (Physics.Raycast(ray, out hitInfo))
        {
            GameObject bullet = Instantiate(bulletFactory);
            bullet.transform.position = hitInfo.point;
            bullet.transform.forward = hitInfo.normal;
            Destroy(bullet, 2);
            // Tracer
            ShotTracer(hitInfo.point - firePos.transform.position, Vector3.Distance(transform.position, hitInfo.point));
            audioSource.PlayOneShot(audioSource.clip);

            if (hitInfo.transform.gameObject.name.Contains("Player"))
            {

                PlayerStatus ph = hitInfo.transform.GetComponent<PlayerStatus>();
                if (ph != null)
                {
                    ph.Damaged(2);
                }
            }
            enemyMapMark.Fire();
            animator.SetTrigger("Attack");
        }
    }

    public bool OnDamaged(float damage)
    {
        if (state == EnemyState.Die)
        {
            return false;
        }

        // 현재 체력에서 데미지만큼 마이너스 처리
        currHP -= damage;

        if (currHP > 0)
        {        
            int randomAnim = Random.Range(0, 2);
            // Audio (총 소리)
            audioSource.PlayOneShot(audioManager.painSounds[Random.Range(0, audioManager.painSounds.Length)]);
            if (randomAnim == 0)
            {
                animator.SetTrigger("Damage1");
            }
            else
            {
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Damage2");
            }

            return false;
        }
        else
        {
            audioSource.PlayOneShot(audioManager.dyingSounds[Random.Range(0, audioManager.dyingSounds.Length)]);
            state = EnemyState.Die;
            Die();
            return true;
        }
    }

    public void Die()
    {
        // 이동을 멈춘다
        _agent.enabled = false;
        // 콜라이더 비활성화
        GetComponent<CapsuleCollider>().enabled = false;

        // 죽음 애니메이션 재생
        animator.SetTrigger("Die");

        // 게임 매니저에 죽은 에너미수 전달 (Count)
        gameManager.EnemyKilled();

    }

    public void InitializePatrolRoute()
    {
        foreach (Transform child in PatrolRoute)
        {
            // 리스트에 추가
            Locations.Add(child);
        }
    }

    public void MoveToNextPatrolLocation()
    {
        // 순찰 경로 설정 X시 return
        if (Locations.Count == 0)
        {
            return;
        }

        // 적 캐릭터는 해당 위치로 이동한다
        _agent.destination = Locations[_locationIndex].position;
        // 현재 이동 위치에서 다음으로 넘어감 , 마지막 위치에 도달 시, 다시 처음으로 가랏 (순환구조)       
        _locationIndex = (_locationIndex + 1) % Locations.Count;

        // Idle 함수 호출에 대한 개선필요
        UpdateIdle();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            _agent.destination = player.position;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Bullet(clone)")
        {
            currHP -= 1;
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

    void ApplySeparation()
    {
        Vector3 separation = Vector3.zero;
        int neighborCount = 0;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, separationRadius);
        foreach (var hitCollider in hitColliders)
        {
            if(hitCollider != this.GetComponent<Collider>() && hitCollider.CompareTag("Enemy"))
            {
                Vector3 direction = transform.position - hitCollider.transform.position;
                separation += direction.normalized / direction.magnitude;
                // 인접 카운트 증가
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            separation /= neighborCount;
            Vector3 separationMove = separation * separationForce;
            _agent.Move(separationMove * Time.deltaTime);
        }

    }
}
