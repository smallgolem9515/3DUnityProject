using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class StudyZombieAi : MonoBehaviour
{
    public enum ZombieState //좀비의 상태를 관리
    {
        Partrol, Chase, Attack, Idle, Damage, Die
    }
    public ZombieState currentState; //현재 상태

    public enum ZombieType //좀비의 종류를 관리
    {
        ZombieType1,ZombieType2,ZombieType3
    }
    public ZombieType zombieType; //좀비 종류

    public Transform[] patrolPoints; //순찰 지점들
    private Transform player; //플레이어의 Transform
    public float detectionRange = 10f; //플레이어를 감지하는 범위
    public float attackRange = 2.0f; //공격범위
    public int hp = 100; //기본체력(단 ZombieType마다 다름)
    public int damage = 1;
    public Transform handTransform; //좀비의 손 위치
    public float sphereCastRadius = 0.5f; //Cast 반경
    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private bool isAttacking; //공격 중인지를 나타내는 변수
    private Animator animator;
    public LayerMask attackLayerMask; //공격 대상 레이어 설정
    private bool patrollingForward = true; //순찰 지점 왕복을 위한 방향 관리
    public float idleTime = 2.0f; //순찰 지점에서 대기하는 시간
    private float idleTimer = 0; //Idle 애니메이션 대기 시간
    private float attackCooldown = 0f; //공격 대기 시간

    private bool isJumping = false; //점프 체크
    private Rigidbody rb;
    public float jumpHeight = 2.0f; //점프 높이
    public float jumpDuration = 1.0f; //점프 체공 시간
    private NavMeshLink[] navMeshLinks;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        
        if(zombieType == ZombieType.ZombieType1) //좀비의 종류에 따라 스테이터스 변화
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
        navMeshLinks = FindObjectsOfType<NavMeshLink>(); //모든 NavMeshLink찾는다.

        animator.SetLayerWeight(1, 0);
        currentState = ZombieState.Partrol;
    }
    void Update()
    {
        if (isJumping) return; //점프 중에는 다른 동작을 하지 않음

        //플레이어와의 거리
        float distanceToPlayer = Vector3.Distance(player.position,transform.position);

        //플레이어가 감지 범위 안에 있고 공격 중이 아닐때 추적
        if(distanceToPlayer <= detectionRange && !isAttacking)
        {
            currentState = ZombieState.Chase; //플레이어 추적
        }
        //플레이어가 공격 범위 안에 있고 공격 중이 아닐 때 공격
        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            //딜레이를 고려해서 공격할 수 있는 상태가 아니면 함수가 실행되지 않도록 조건을 추가한 것
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
        Debug.Log("Zombie Idle 상태");
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
        if(patrollingForward) //다음 순찰 지점으로 인덱스를 이동
        {
            currentPatrolIndex++;
            if(currentPatrolIndex >= patrolPoints.Length)
            {
                currentPatrolIndex = patrolPoints.Length - 2; //마지막 지점에서 돌아감
                patrollingForward = false;
            }
        }
        else
        {
            currentPatrolIndex--;
            if(currentPatrolIndex < 0)
            {
                currentPatrolIndex = 1; //첫 지점에서 다시 전진
                patrollingForward = true;
            }
        }
        currentState = ZombieState.Partrol; //다시 순찰 상태 전환
    }
    void Patrol()
    {
        if (patrolPoints.Length == 0) //예외 처리
        {
            return;
        }
        Debug.Log("Zombie Patrol 상태");

        agent.isStopped = false;
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", true);

        agent.destination = patrolPoints[currentPatrolIndex].position; //현재 목표 지점으로 이동
        //destination = 목적지를 설정하는 속성, AI가 그 좌표를 목표로 경로를 계산해서 이동

        //장애물이 있거나 NavMeshLink에 가까워지면 점프
        if(agent.isOnOffMeshLink)
        {
            StartCoroutine(JumpAcrossLink());
        }

        //순찰 지점에 도착 했을 때 Idle 상태로 전환, remainingDistance : AI 목표 목적지까지 남은 거리(실시간 업데이트)
        if(agent.remainingDistance < 0.5f && !agent.pathPending) //pathPending : 경로가 계산중인지 여부를 확인
        {
            currentState = ZombieState.Idle;
        }
    }
    void ChasePlayer()
    {
        Debug.Log("Zombie Chase 상태");
        agent.isStopped = false; //isStopped : 에이전트의 이동을 일시적으로 이동 제어
        //true : 경로를 따라가는걸 멈추고 false : 다시 경로 이동
        agent.destination = player.position; //목표 위치를 플레이어로 지정
        animator.SetBool("isWalking", true);
        animator.SetBool("isIdle", false);
    }
    void AttackPlayer()
    {
        if (isAttacking) return; //공격 중이라면 다시 공격하지 않음

        isAttacking = true;
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
        animator.SetTrigger("Attack");

        //플레이어 방향으로 회전
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x,0,direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        //Quaternion.Slerp() : 두개의 쿼터니언을 부드럽게 보간하는 함수

        PerformAttack();
        Debug.Log("Zombie Attack 상태");

        StartCoroutine(AttackCooldown());
    }
    public void PerformAttack()
    {
        Vector3 attackDirection = player.position - handTransform.position;
        attackDirection.Normalize();

        RaycastHit hit;
        float shpereRadius = 1.0f; //반경
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
    //공격 종료 시 호출되는 함수(공격 애니메이션이 끝날 때 애니메이션 이벤트로 호출되는 함수)
    public void EndAttack()
    {
        Debug.Log("End Attack");
        isAttacking = false; //공격 상태 해제
        agent.isStopped = false;

        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if (distanceToPlayer <= attackRange) //공격 범위 내에 있다면 공격 시작
        {
            AttackPlayer();
        }
        else
        {
            if(distanceToPlayer <= detectionRange) //공격범위에 벗어났고 추적 또는 순찰모드로
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
    IEnumerator AttackCooldown() //공격 쿨타임 함수
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

        //NavMeshLink 시작과 끝 좌표 가져오기
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = linkData.startPos;
        Vector3 endPos = linkData.endPos;

        float elapsedTime = 0;
        while(elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            Vector3 currentPosition = Vector3.Lerp(startPos, endPos, t);
            currentPosition.y += Mathf.Sin(t * Mathf.PI) * jumpHeight; //포물선 경로
            transform.position = currentPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        //도착점에 위치
        transform.position = endPos;

        //NavMeshAgent 경로 재개
        agent.CompleteOffMeshLink();
        agent.isStopped = false;

        isJumping = false;
        animator.SetLayerWeight(1, 0);
    }
    void DamagedZombie()
    {

    }
}
