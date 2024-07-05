using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyBehavior : MonoBehaviour
{
    // Ÿ�� ���� (��ǥ ����)
    public Transform PatrolRoute;
    // ���� ��ġ ���
    public List<Transform> Locations;
    private int _locationIndex = 0;
    private NavMeshAgent _agent;

    // player �� Transform �� ����
    Transform player;

    public Slider hpUI;

    private int _lives = 3;

    // ���ʹ� �����
    //public int EnemyLives
    //{
    //    get { return _lives; }
    //    set {
    //        _lives = value;

    //        // ���� �׾������� Ȯ��
    //        if (_lives <= 0)
    //        {
    //            Destroy(gameObject);
    //            print("Enemy down");
    //        }
    //    }
    //}

    public enum EnemyState
    {
        Idle,
        Attack,
        TakeDamage,
        Die
    }

    public EnemyState state;

    // ���ʹ� ������ ����
    Vector3 pos;

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

    void Start()
    {
        player = GameObject.Find("Player").transform;
        // ���� HP�� �ִ� HP�� ����
        currHP = maxHP;

        player = GameObject.Find("Player").transform;
        _agent = GetComponent<NavMeshAgent>();

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
        if(state == EnemyState.Attack)
        {
            UpdateAttack();
        }
        else if(state == EnemyState.Idle)
        {
            UpdateIdle();
        }
        else if(state == EnemyState.TakeDamage)
        {
            TakeDamage();
        }
        else if(state == EnemyState.Die)
        {
            UpdateDie();
        }
    }

    // ��� �Լ�
    public void UpdateIdle()
    {
        //�÷��̾���� �Ÿ��� �߰� ���� �ȿ� ��� �Դٸ� �÷��̾ �ٶ󺸸� ���߱�
        float dist = Vector3.Distance(player.transform.position, transform.position);
        if(dist < findDistance)
        {
            // �÷��̾ �ٶ󺸱�
            Vector3 directionToPlayer = player.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            UpdateAttack();
        }
    }

    // ���� �Լ�
    public void UpdateAttack()
    {

        float dist = Vector3.Distance(player.transform.position, transform.position);
        // �÷��̾���� �Ÿ��� ���� ���� ���� �ȿ� ���Դٸ�
        // �÷��̾ �ٶ󺸸� ������ ���·� ��ȯ �� ���� -> �÷��̾ �־����� �߰�
        if(dist < attackDistance)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackDelayTIme)
            {
                attackTimer = 0;
                // NavMesh ���߰� �����϶�.
                _agent.isStopped = true;
                print("Attack!");
                // PlayerStatus ������Ʈ�� ��������
                PlayerStatus pm = player.GetComponent<PlayerStatus>();
                // ������ ������Ʈ�� Damaged �Լ��� ����
                pm.Damaged(10);

                if (dist > findDistance)
                {
                    lastKnownPosition = player.transform.position;
                }
            }
        }
        else
        {
            // NavMesh �ٽ� Ȱ��ȭ �ϰ� �÷��̾� ���󰡱� // ��ֹ��� ������ ã�ƿ��鼭 �����ϱ����
            _agent.isStopped = false;
            if(_agent.destination != player.transform.position)
            {
                _agent.SetDestination(player.transform.position);
            }
        }

        TakeDamage();
    }

    public void TakeDamage()
    {
        
    }

    // �ǰ� �Լ�
    public void OnDamaged(float damage)
    {
        if (state == EnemyState.Die)
        {
            return;
        }

        currHP -= damage;

        // HP�ٸ� ��������.
        float ratio = currHP / maxHP;
        hpUI.value = ratio;

        // ü���� 0 �̻��� �� ������ ����
        if (currHP > 0)
        {
            //state = EnemyState.TakeDamage;
        }
        // ���ʹ��� ü���� 0 ���Ϸ� �������� Die ���·� ��ȯ
        else
        {
            state = EnemyState.Die;
        }
    }

    // ��� �Լ�
    public void UpdateDie()
    {
        currTime += Time.deltaTime;
        // ���� �ð��� �״� �ð����� Ŀ���ٸ�
        if(currTime > dieDelayTIme)
        {
            // �׺���̼� �ý��� ��Ȱ��ȭ
            _agent.enabled = false;
            // ĸ�� �ݶ��̴� ��Ȱ��ȭ
            GetComponent<CapsuleCollider>().enabled = false;
            // �Ʒ� �������� �����̰� �Ѵ�.
            transform.position += Vector3.down * speed * Time.deltaTime;
            // y�� ��ġ�� -2���� �۾��� ���
            if(transform.position.y < -2)
            {
                Destroy(gameObject);
            }
        }
    }

    // ���� ����
    public void InitializePatrolRoute()
    {
        foreach (Transform child in PatrolRoute)
        {
            Locations.Add(child);
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
        if(other.name == "Player")
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
}
