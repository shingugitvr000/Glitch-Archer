using UnityEngine;
using System;

public class EnemyStateMachine
{
    // 상태 관련 변수들
    public EnemyState CurrentState { get; private set; }

    // AI 컨트롤에 필요한 참조들
    private Transform enemyTransform;
    private Rigidbody2D rb;
    private Animator animator;
    private WeaponController weaponController;

    // AI 행동 관련 변수
    private Transform player;
    private float detectionRange;
    private float attackRange;
    private float moveSpeed;
    private BaseEnemy baseEnemy;

    // 생성자를 통해 필요한 정보 주입
    public EnemyStateMachine(
        Transform enemyTransform,
        Rigidbody2D rb,
        Animator animator,
        WeaponController weaponController,
        float detectionRange,
        float attackRange,
        float moveSpeed)
    {
        this.enemyTransform = enemyTransform;
        this.rb = rb;
        this.animator = animator;
        this.weaponController = weaponController;
        this.detectionRange = detectionRange;
        this.attackRange = attackRange;
        this.moveSpeed = moveSpeed;

        // 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // 초기 상태 설정
        CurrentState = EnemyState.Idle;
    }

    public EnemyStateMachine(BaseEnemy baseEnemy)
    {
        this.baseEnemy = baseEnemy;
    }

    // 상태 변경 메서드
    public void ChangeState(EnemyState newState)
    {
        CurrentState = newState;

        // 상태별 초기화 로직
        switch (newState)
        {
            case EnemyState.Idle:
                // 속도 멈춤
                rb.velocity = Vector2.zero;

                // 애니메이션 속도 0으로
                if (animator != null)
                    animator.SetFloat("Speed", 0);
                break;

            case EnemyState.Patrol:
                // 순찰 시 약간의 무작위 움직임 추가 가능
                break;

            case EnemyState.Chase:
                if (animator != null)
                    animator.SetFloat("Speed", 1);
                break;

            case EnemyState.Attack:
                // 공격 상태일 때 멈춤
                rb.velocity = Vector2.zero;
                break;
        }
    }

    // 상태 업데이트 메서드
    public void Update()
    {
        if (player == null) return;

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector2.Distance(enemyTransform.position, player.position);

        switch (CurrentState)
        {
            case EnemyState.Idle:
                // 플레이어 감지 시 추격 상태로 전환
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Patrol:
                // 플레이어 감지 시 추격 상태로 전환
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                // 플레이어 감지 범위를 벗어나면 대기 상태로
                if (distanceToPlayer > detectionRange)
                {
                    ChangeState(EnemyState.Idle);
                    return;
                }

                // 공격 범위 내에 들어오면 공격 상태로
                if (distanceToPlayer <= attackRange)
                {
                    ChangeState(EnemyState.Attack);
                    return;
                }

                // 플레이어 추격
                Vector2 direction = (player.position - enemyTransform.position).normalized;
                rb.velocity = direction * moveSpeed;
                break;

            case EnemyState.Attack:
                // 공격 범위를 벗어나면 추격 상태로
                if (distanceToPlayer > attackRange)
                {
                    ChangeState(EnemyState.Chase);
                    return;
                }

                // 공격 로직
                weaponController.Fire();
                break;
        }
    }
}