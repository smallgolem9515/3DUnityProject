using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class StudyZombieAi : MonoBehaviour
{
    public enum ZombieState //������ ���¸� ����
    {
        Partrol, Chase, Attack, Idle, Damage, Die
    }
    public ZombieState currentState; //���� ����

    public enum ZombieType //������ ������ ����
    {
        ZombieType1,ZombieType2,ZombieType3
    }
    public ZombieType zombieType; //���� ����

    public Transform[] patrolPoints; //���� ������
    private Transform player; //�÷��̾��� Transform
    public float detectionRange = 10f; //�÷��̾ �����ϴ� ����
    public float attackRange = 2.0f; //���ݹ���
    public int hp = 100; //�⺻ü��(�� ZombieType���� �ٸ�)
    public int damage = 1;
    public Transform handTransform; //������ �� ��ġ
    public float sphereCastRadius = 0.5f; //Cast �ݰ�
    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private bool isAttacking; //���� �������� ��Ÿ���� ����
    private Animator animator;
    public LayerMask attackLayerMask; //���� ��� ���̾� ����
    private bool patrollingForward = true; //���� ���� �պ��� ���� ���� ����
    public float idleTime = 2.0f; //���� �������� ����ϴ� �ð�
    private float idleTimer = 0; //Idle �ִϸ��̼� ��� �ð�
    private float attackCooldown = 0f; //���� ��� �ð�

    private bool isJumping = false; //���� üũ
    private Rigidbody rb;
    public float jumpHeight = 2.0f; //���� ����
    public float jumpDuration = 1.0f; //���� ü�� �ð�
    private NavMeshLink[] navMeshLinks;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        
        if(zombieType == ZombieType.ZombieType1) //������ ������ ���� �������ͽ� ��ȭ
        {
            agent.speed = 1.0f;
            attackCooldown = 1.0f;
            damage = 1;
            hp = 100;
        } 
        else if (zombieType == ZombieType.ZombieType2)
        {
            agent.speed = 2.0f;
            attackCooldown = 1.5f;
            damage = 2;
            hp = 150;
        }
        else if (zombieType == ZombieType.ZombieType3)
        {
            agent.speed = 1.5f;
            attackCooldown = 2.0f;
            damage = 3;
            hp = 200;
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        player = StudyPlayerManager.Instance.transform;
        navMeshLinks = FindObjectsOfType<NavMeshLink>(); //��� NavMeshLinkã�´�.

        animator.SetLayerWeight(1, 0);
        currentState = ZombieState.Partrol;
    }
    void Update()
    {
        if (isJumping) return; //���� �߿��� �ٸ� ������ ���� ����

        //�÷��̾���� �Ÿ�
        float distanceToPlayer = Vector3.Distance(player.position,transform.position);

        //�÷��̾ ���� ���� �ȿ� �ְ� ���� ���� �ƴҶ� ����
        if(distanceToPlayer <= detectionRange && !isAttacking)
        {
            currentState = ZombieState.Chase; //�÷��̾� ����
        }
        //�÷��̾ ���� ���� �ȿ� �ְ� ���� ���� �ƴ� �� ����
        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            //�����̸� ����ؼ� ������ �� �ִ� ���°� �ƴϸ� �Լ��� ������� �ʵ��� ������ �߰��� ��
            if(!isAttacking)
            {
                currentState = ZombieState.Attack;
            }
        }
        switch (currentState)
        {
            case ZombieState.Partrol:
                Patrol();
                break;
            case ZombieState.Chase:
                ChasePlayer();
                break;
            case ZombieState.Attack:
                AttackPlayer();
                break;
            case ZombieState.Idle:
                Idle();
                break;
        }
    }
    void Idle()
    {
        Debug.Log("Zombie Idle ����");
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", true);

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            idleTimer = 0;
            MoveToNextPatrolPoint();
        }
    }
    void MoveToNextPatrolPoint()
    {
        if(patrollingForward) //���� ���� �������� �ε����� �̵�
        {
            currentPatrolIndex++;
            if(currentPatrolIndex >= patrolPoints.Length)
            {
                currentPatrolIndex = patrolPoints.Length - 2; //������ �������� ���ư�
                patrollingForward = false;
            }
        }
        else
        {
            currentPatrolIndex--;
            if(currentPatrolIndex < 0)
            {
                currentPatrolIndex = 1; //ù �������� �ٽ� ����
                patrollingForward = true;
            }
        }
        currentState = ZombieState.Partrol; //�ٽ� ���� ���� ��ȯ
    }
    void Patrol()
    {
        if (patrolPoints.Length == 0) //���� ó��
        {
            return;
        }
        Debug.Log("Zombie Patrol ����");

        agent.isStopped = false;
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", true);

        agent.destination = patrolPoints[currentPatrolIndex].position; //���� ��ǥ �������� �̵�
        //destination = �������� �����ϴ� �Ӽ�, AI�� �� ��ǥ�� ��ǥ�� ��θ� ����ؼ� �̵�

        //��ֹ��� �ְų� NavMeshLink�� ��������� ����
        if(agent.isOnOffMeshLink)
        {
            StartCoroutine(JumpAcrossLink());
        }

        //���� ������ ���� ���� �� Idle ���·� ��ȯ, remainingDistance : AI ��ǥ ���������� ���� �Ÿ�(�ǽð� ������Ʈ)
        if(agent.remainingDistance < 0.5f && !agent.pathPending) //pathPending : ��ΰ� ��������� ���θ� Ȯ��
        {
            currentState = ZombieState.Idle;
        }
    }
    void ChasePlayer()
    {
        Debug.Log("Zombie Chase ����");
        agent.isStopped = false; //isStopped : ������Ʈ�� �̵��� �Ͻ������� �̵� ����
        //true : ��θ� ���󰡴°� ���߰� false : �ٽ� ��� �̵�
        agent.destination = player.position; //��ǥ ��ġ�� �÷��̾�� ����
        animator.SetBool("isWalking", true);
        animator.SetBool("isIdle", false);
    }
    void AttackPlayer()
    {
        if (isAttacking) return; //���� ���̶�� �ٽ� �������� ����

        isAttacking = true;
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
        animator.SetTrigger("Attack");

        //�÷��̾� �������� ȸ��
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x,0,direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        //Quaternion.Slerp() : �ΰ��� ���ʹϾ��� �ε巴�� �����ϴ� �Լ�

        PerformAttack();
        Debug.Log("Zombie Attack ����");

        StartCoroutine(AttackCooldown());
    }
    public void PerformAttack()
    {
        Vector3 attackDirection = player.position - handTransform.position;
        attackDirection.Normalize();

        RaycastHit hit;
        float shpereRadius = 1.0f; //�ݰ�
        float castDistance = attackRange;

        if (Physics.SphereCast(handTransform.position, shpereRadius, attackDirection, out hit, castDistance, attackLayerMask))
        {
            Debug.Log("SphereCast hit : " + hit.collider.name);

            if(hit.collider.tag == "Player")
            {
                StudyPlayerManager.Instance.TakeDamage(damage);
                Debug.Log("Player Hit");
            }
        }

        Debug.DrawRay(handTransform.position, attackDirection * castDistance, Color.red, 1.0f);
    }
    //���� ���� �� ȣ��Ǵ� �Լ�(���� �ִϸ��̼��� ���� �� �ִϸ��̼� �̺�Ʈ�� ȣ��Ǵ� �Լ�)
    public void EndAttack()
    {
        Debug.Log("End Attack");
        isAttacking = false; //���� ���� ����
        agent.isStopped = false;

        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if (distanceToPlayer <= attackRange) //���� ���� ���� �ִٸ� ���� ����
        {
            AttackPlayer();
        }
        else
        {
            if(distanceToPlayer <= detectionRange) //���ݹ����� ����� ���� �Ǵ� ��������
            {
                currentState = ZombieState.Chase;
            }
            else
            {
                currentState = ZombieState.Idle;
            }
            animator.SetBool("isWalking", true);
        }
    }
    IEnumerator AttackCooldown() //���� ��Ÿ�� �Լ�
    {
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }
    IEnumerator JumpAcrossLink()
    {
        animator.SetLayerWeight(1, 1);
        Debug.Log("Zombie Jump");

        isJumping = true;
        agent.isStopped = true;

        //NavMeshLink ���۰� �� ��ǥ ��������
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = linkData.startPos;
        Vector3 endPos = linkData.endPos;

        float elapsedTime = 0;
        while(elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            Vector3 currentPosition = Vector3.Lerp(startPos, endPos, t);
            currentPosition.y += Mathf.Sin(t * Mathf.PI) * jumpHeight; //������ ���
            transform.position = currentPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        //�������� ��ġ
        transform.position = endPos;

        //NavMeshAgent ��� �簳
        agent.CompleteOffMeshLink();
        agent.isStopped = false;

        isJumping = false;
        animator.SetLayerWeight(1, 0);
    }
    void DamagedZombie()
    {

    }
}
