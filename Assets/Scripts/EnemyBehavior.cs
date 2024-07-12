using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.UI;

public class EnemyBehavior : MonoBehaviour
{
    // Ÿ�� ���� (��ǥ ����)
    public Transform PatrolRoute;
    // ���� ��ġ ���
    public List<Transform> Locations;
    private int _locationIndex = 0;
    private NavMeshAgent _agent;

    GameObject enemy;

    // player �� Transform �� ����
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

    // ���ǵ�
    public float speed = 1;
    // ���� ü��
    public float currHP;
    // Max HP
    public float maxHP;

    // �߰� ���� �Ÿ�
    public float findDistance = 8f;
    // ���� ����
    public float attackDistance = 5f;
    // ���� �Ÿ�
    public float stopDistance = 2f;
    // ���� ������ �ð�
    public float attackDelayTIme = 2f;
    // ���� Ÿ�̸�
    float attackTimer = 0;
    // �ǰ� �ð�
    public float damagedDelayTime = 2f;

    // ���� �ð�
    public float currTime;
    // �״� �ð�
    public float dieDelayTIme = 2f;
    // �÷��̾��� ������ �˷��� ��ġ
    private Vector3 lastKnownPosition;
    // ���ʹ� ������ ����
    Vector3 pos;

    // �ִϸ�����
    public Animator animator;
    // ���� ��ġ
    public Transform target;

    void Start()
    {
        player = GameObject.Find("Player").transform;
        // ���� HP�� �ִ� HP�� ����
        currHP = maxHP;

        lastKnownPosition = Vector3.zero;

        _agent = GetComponent<NavMeshAgent>();

        audioSource = GetComponent<AudioSource>();

        animator = GetComponentInChildren<Animator>();
        // �� �±׸� ����� Ÿ���� ã�´�.
        target = GameObject.Find("Player").transform;

        InitializePatrolRoute();

        MoveToNextPatrolLocation();

    }

    void Update()
    {
        // NavMeshComp ������ ��󿡼� �󸶳� ������ �ְ�, �� ���� 0.2���� ������
        // pathPending -> bool���� ��ȯ
        // ������Ʈ�� ��ǥ ������ �����ߴ��� Ȯ�� �� -> ���� ���� ��ġ�� �̵�
        if (_agent.enabled && _agent.remainingDistance < 0.2f && !_agent.pathPending)
        {
            MoveToNextPatrolLocation();
        }

        // ���ʹ��� ���¿� ���� ����
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
            UpdateDie();
        }
    }

    // ��� �Լ�
    public void UpdateIdle()
    {
        //�÷��̾���� �Ÿ��� �߰� ���� �ȿ� ��� �Դٸ� �÷��̾ �ٶ󺸸� ���߱�
        float dist = Vector3.Distance(player.transform.position, transform.position);
        if (dist < findDistance)
        {
            // �÷��̾ �ٶ󺸱�
            Vector3 directionToPlayer = player.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            UpdateAttack();

            animator.SetFloat("speed", 2.0f);
        }
    }

    // ���� �Լ�
    public void UpdateAttack()
    {
        float dist = Vector3.Distance(player.transform.position, transform.position);
        // �÷��̾���� �Ÿ��� ���� ���� ���� �ȿ� ���Դٸ�
        // �÷��̾ �ٶ󺸸� ������ ���·� ��ȯ �� ���� -> �÷��̾ �־����� �߰�
        if (dist < attackDistance)
        {
            attackTimer += Time.deltaTime;

            // ���� ���� �ð��� ������� ��
            if (attackTimer >= attackDelayTIme)
            {
                attackTimer = 0;

                // NavMesh ���߰� �����϶�.
                _agent.isStopped = true;
                print("����!");
                TakeDamage(player.transform.position);
            }
        }
        else
        {
            // �÷��̾���� �Ÿ��� �߰� �������� �۴ٸ�
            if (dist < findDistance)
            {
                lastKnownPosition = player.transform.position;
            }
            _agent.isStopped = false;
            _agent.destination = lastKnownPosition;
            //_agent.SetDestination(lastKnownPosition);

            /*
            // �÷��̾ ���� ������ ����� ��
            if (lastKnownPosition != Vector3.zero)
            {
                // NavMesh �ٽ� Ȱ��ȭ �ϰ� �÷��̾� ���󰡱� // ��ֹ��� ������ ã�ƿ��鼭 �����ϱ����
                _agent.isStopped = false;
                _agent.SetDestination(lastKnownPosition);
            }
            attackTimer = 0;
            */
        }
    }

    // ������ �Լ�
    public void TakeDamage(Vector3 aimPos)
    {
        // ����ĳ��Ʈ�� ����Ͽ� �÷��̾� ����
        Ray ray = new Ray(firePos.transform.position, aimPos - firePos.transform.position);
        RaycastHit hitInfo = new RaycastHit();

        // Ray�� �߻��ؼ� �¾Ҵٸ�
        if (Physics.Raycast(ray, out hitInfo))
        {
            // ����ȿ�� ���忡�� ���� ȿ���� �����.
            GameObject bullet = Instantiate(bulletFactory);
            // ���� ��ġ�� �α� 
            bullet.transform.position = hitInfo.point;
            // ���� ����ȿ���� �չ����� ���� ��ġ�� normal ������ �����Ѵ�.
            bullet.transform.forward = hitInfo.normal;
            // ���� ����ȿ���� 3�ʵڿ� �ı�����
            Destroy(bullet, 2);
            // �� ���� ȿ��
            ShotTracer(hitInfo.point - firePos.transform.position, Vector3.Distance(transform.position, hitInfo.point));
            // �� ����
            audioSource.PlayOneShot(audioSource.clip);

            // ���� ����� Player ���
            if (hitInfo.transform.gameObject.name.Contains("Player"))
            {
                // Player ���� �������� ����
                // Player ���� Enemy ������Ʈ�� ��������
                PlayerStatus ph = hitInfo.transform.GetComponent<PlayerStatus>();
                // ������ ������Ʈ���� Damaged �Լ��� ȣ��
                if (ph != null)
                {
                    ph.Damaged(2);
                }
            }

            // �ִϸ����� �߰� - ���� 2��
            animator.SetTrigger("Attack");
            print("????????????");
        }
    }

    // �ǰ� �Լ�
    public bool OnDamaged(float damage)
    {
        if (state == EnemyState.Die)
        {
            return false;
        }

        currHP -= damage;

        // HP�ٸ� ��������.
        float ratio = currHP / maxHP;
        hpUI.value = ratio;

        // ü���� 0 �̻��� �� ������ ����
        if (currHP > 0)
        {
            //state = EnemyState.TakeDamage;
            animator.SetTrigger("Damage");
            return false;
        }
        // ���ʹ��� ü���� 0 ���Ϸ� �������� Die ���·� ��ȯ
        else
        {
            state = EnemyState.Die;
            animator.SetTrigger("Die");
            return true;
        }
    }

    // ��� �Լ�
    public void UpdateDie()
    {
        currTime += Time.deltaTime;
        // ���� �ð��� �״� �ð����� Ŀ���ٸ�
        if (currTime > dieDelayTIme)
        {
            // �׺���̼� �ý��� ��Ȱ��ȭ
            _agent.enabled = false;
            // ĸ�� �ݶ��̴� ��Ȱ��ȭ
            GetComponent<CapsuleCollider>().enabled = false;
            // �Ʒ� �������� �����̰� �Ѵ�.
            //transform.position += Vector3.down * speed * Time.deltaTime;

            Destroy(gameObject);
            // y�� ��ġ�� -2���� �۾��� ���
            //if (transform.position.y < -2)
            //{
            //    Destroy(gameObject);
            //}

            // �ִϸ�����
            animator.SetTrigger("Die");
        }
    }

    // ���� ����
    public void InitializePatrolRoute()
    {
        foreach (Transform child in PatrolRoute)
        {
            Locations.Add(child);

            //animator.GetBool("isRunning");
        }
    }

    // ���� �������� �����̱�
    public void MoveToNextPatrolLocation()
    {
        // ���� ��ġ�� ���� ��� ��ȯ
        if (Locations.Count == 0)
        {
            return;
        }

        _agent.destination = Locations[_locationIndex].position;
        // ���� �ΰ�
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
        // ���ʹ̰� �Ѿ˿� �¾��� �� 
        if (collision.gameObject.name == "Bullet(clone)")
        {
            // ü���� 1 ���ҽ�Ų��.
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
