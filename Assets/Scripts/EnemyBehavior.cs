using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.UI;

public class EnemyBehavior : MonoBehaviour
{
    public GameManager gameManager;

    public Transform PatrolRoute;
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

    public float speed = 1;
    public float currHP;
    public float maxHP;

    public float findDistance = 8f;
    public float attackDistance = 5f;
    public float stopDistance = 2f;
    public float attackDelayTIme = 2f;
    float attackTimer = 0;
    public float damagedDelayTime = 2f;

    public float currTime;
    public float dieDelayTIme = 2f;
    private Vector3 lastKnownPosition;
    Vector3 pos;

    public Animator animator;
    public Transform target;
    AudioManager audioManager;

    void Start()
    {
        player = GameObject.Find("Player").transform;

        currHP = maxHP;

        lastKnownPosition = Vector3.zero;

        _agent = GetComponent<NavMeshAgent>();

        audioSource = GetComponent<AudioSource>();

        animator = GetComponentInChildren<Animator>();

        InitializePatrolRoute();

        MoveToNextPatrolLocation();

        gameManager = GameManager.FindObjectOfType<GameManager>();

        enemyMapMark = gameObject.GetComponent<EnemyMapMark>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    void Update()
    {
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
        if (dist < findDistance)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f);

            UpdateAttack();

            animator.SetFloat("speed", 1.0f);
        }
    }

    public void UpdateAttack()
    {
        float dist = Vector3.Distance(player.transform.position, transform.position);

        if (dist < attackDistance)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= attackDelayTIme)
            {
                attackTimer = 0;

                _agent.isStopped = true;
                print("����!");
                TakeDamage(player.transform.position);
            }
        }
        else
        {
            if (dist < findDistance)
            {
                lastKnownPosition = player.transform.position;
            }
            _agent.isStopped = false;
            _agent.destination = lastKnownPosition;
            //_agent.SetDestination(lastKnownPosition);

            /*
            if (lastKnownPosition != Vector3.zero)
            {
                // NavMesh �ٽ� Ȱ��ȭ �ϰ� �÷��̾� ���󰡱� 
                _agent.isStopped = false;
                _agent.SetDestination(lastKnownPosition);
            }
            attackTimer = 0;
            */
        }
    }

    public void TakeDamage(Vector3 aimPos)
    {
        if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Zombie Stand Up" ||
            animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Falling Back Death")
            return;

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

        currHP -= damage;

        if (currHP > 0)
        {
            //state = EnemyState.TakeDamage;
           
            int randomAnim = Random.Range(0, 2);
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
        _agent.enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;

        animator.SetTrigger("Die");

        gameManager.EnemyKilled();

    }

    public void InitializePatrolRoute()
    {
        foreach (Transform child in PatrolRoute)
        {
            Locations.Add(child);

            //animator.GetBool("isRunning");
        }
    }

    public void MoveToNextPatrolLocation()
    {
        // ���� ��ġ�� ���� ��� ��ȯ
        if (Locations.Count == 0)
        {
            return;
        }

        _agent.destination = Locations[_locationIndex].position;
        
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
        if (collision.gameObject.name == "Bullet(clone)")
        {
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

    void ApplySeparation()
    {
        //return;

        Vector3 separation = Vector3.zero;
        int neighborCount = 0;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, separationRadius);
        foreach (var hitCollider in hitColliders)
        {
            if(hitCollider != this.GetComponent<Collider>() && hitCollider.CompareTag("Enemy"))
            {
                Vector3 direction = transform.position - hitCollider.transform.position;
                separation += direction.normalized / direction.magnitude;
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
