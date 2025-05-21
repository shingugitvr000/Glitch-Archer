using UnityEngine;
using System;

// AI 상태 열거형
public enum EnemyState
{
    Idle,           // 대기 상태
    Patrol,         // 순찰 상태
    Chase,          // 추격 상태
    Attack,         // 공격 상태
    Hurt,           // 피격 상태
    Die             // 사망 상태
}

public class EnemyAI : MonoBehaviour
{
    // 상태 관련 변수
    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    // 이동 및 전투 관련 변수
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float patrolRadius = 10f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int maxHealth = 100;

    // 컴포넌트 참조
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private WeaponController weaponController;

    // 상태 관련 변수
    private float lastAttackTime;
    private Vector2 patrolStartPosition;
    private int currentHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        weaponController = GetComponent<WeaponController>();

        // 초기 체력 설정
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // 초기 순찰 시작 위치 저장
        patrolStartPosition = transform.position;
    }

    private void Update()
    {
        // 현재 상태에 따른 행동 업데이트
        switch (CurrentState)
        {
            case EnemyState.Idle:
                UpdateIdleState();
                break;
            case EnemyState.Patrol:
                UpdatePatrolState();
                break;
            case EnemyState.Chase:
                UpdateChaseState();
                break;
            case EnemyState.Attack:
                UpdateAttackState();
                break;
            case EnemyState.Hurt:
                UpdateHurtState();
                break;
            case EnemyState.Die:
                UpdateDieState();
                break;
        }
    }

    // 상태 전환 메서드
    private void ChangeState(EnemyState newState)
    {
        // 현재 상태 종료 로직
        ExitCurrentState();

        // 상태 변경
        CurrentState = newState;

        // 새 상태 진입 로직
        EnterNewState();
    }

    // 대기 상태 업데이트
    private void UpdateIdleState()
    {
        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 플레이어 감지 범위 내에 들어오면 추격 상태로 전환
        if (distanceToPlayer <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
        }
        else
        {
            // 일정 시간 후 순찰 상태로 전환
            ChangeState(EnemyState.Patrol);
        }
    }

    // 순찰 상태 업데이트
    private void UpdatePatrolState()
    {
        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 플레이어 감지 범위 내에 들어오면 추격 상태로 전환
        if (distanceToPlayer <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // 순찰 로직
        // 초기 위치를 중심으로 랜덤하게 이동
        Vector2 patrolTarget = patrolStartPosition +
            new Vector2(
                UnityEngine.Random.Range(-patrolRadius, patrolRadius),
                UnityEngine.Random.Range(-patrolRadius, patrolRadius)
            );

        // 목표 지점으로 이동
        MoveTowards(patrolTarget, moveSpeed);
    }

    // 추격 상태 업데이트
    private void UpdateChaseState()
    {
        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 플레이어 감지 범위를 벗어나면 대기 상태로 전환
        if (distanceToPlayer > detectionRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 공격 범위 내에 들어오면 공격 상태로 전환
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        // 플레이어 추격
        MoveTowards(player.position, chaseSpeed);
    }

    // 공격 상태 업데이트
    private void UpdateAttackState()
    {
        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 공격 범위를 벗어나면 추격 상태로 전환
        if (distanceToPlayer > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // 공격 쿨다운 체크
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // 공격 수행
            Attack();
            lastAttackTime = Time.time;
        }
    }

    // 피격 상태 업데이트
    private void UpdateHurtState()
    {
        // 피격 애니메이션 재생 등의 로직
        // 일정 시간 후 이전 상태로 복귀하거나 다른 상태로 전환
    }

    // 사망 상태 업데이트
    private void UpdateDieState()
    {
        // 사망 애니메이션 재생
        // 오브젝트 제거 등의 로직
        Destroy(gameObject);
    }

    // 목표 지점으로 이동
    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        rb.velocity = direction * speed;

        // 애니메이터에 이동 상태 전달
        animator.SetFloat("Speed", rb.velocity.magnitude);
    }

    // 공격 메서드
    private void Attack()
    {
        // 무기 컨트롤러를 통해 총 발사
        weaponController.Fire();

        // 공격 애니메이션 트리거
        animator.SetTrigger("Attack");
    }

    // 피해 받을 때 호출되는 메서드
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            // 체력이 0 이하면 사망 상태로 전환
            ChangeState(EnemyState.Die);
        }
        else
        {
            // 피격 상태로 전환
            ChangeState(EnemyState.Hurt);
        }
    }

    // 현재 상태 종료 로직
    private void ExitCurrentState()
    {
        switch (CurrentState)
        {
            case EnemyState.Patrol:
                // 순찰 상태 종료 시 속도 초기화 등
                rb.velocity = Vector2.zero;
                break;
            case EnemyState.Chase:
                // 추격 상태 종료 시 속도 초기화 등
                rb.velocity = Vector2.zero;
                break;
        }
    }

    // 새 상태 진입 로직
    private void EnterNewState()
    {
        switch (CurrentState)
        {
            case EnemyState.Idle:
                // 대기 상태 진입 시 애니메이션 등
                animator.SetFloat("Speed", 0);
                break;
            case EnemyState.Patrol:
                // 순찰 상태 진입 시 초기 설정
                break;
        }
    }
}

