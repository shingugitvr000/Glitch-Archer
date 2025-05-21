using UnityEngine;
using System;

public class EnemyStateMachine
{
    // ���� ���� ������
    public EnemyState CurrentState { get; private set; }

    // AI ��Ʈ�ѿ� �ʿ��� ������
    private Transform enemyTransform;
    private Rigidbody2D rb;
    private Animator animator;
    private WeaponController weaponController;

    // AI �ൿ ���� ����
    private Transform player;
    private float detectionRange;
    private float attackRange;
    private float moveSpeed;
    private BaseEnemy baseEnemy;

    // �����ڸ� ���� �ʿ��� ���� ����
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

        // �÷��̾� ã��
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // �ʱ� ���� ����
        CurrentState = EnemyState.Idle;
    }

    public EnemyStateMachine(BaseEnemy baseEnemy)
    {
        this.baseEnemy = baseEnemy;
    }

    // ���� ���� �޼���
    public void ChangeState(EnemyState newState)
    {
        CurrentState = newState;

        // ���º� �ʱ�ȭ ����
        switch (newState)
        {
            case EnemyState.Idle:
                // �ӵ� ����
                rb.velocity = Vector2.zero;

                // �ִϸ��̼� �ӵ� 0����
                if (animator != null)
                    animator.SetFloat("Speed", 0);
                break;

            case EnemyState.Patrol:
                // ���� �� �ణ�� ������ ������ �߰� ����
                break;

            case EnemyState.Chase:
                if (animator != null)
                    animator.SetFloat("Speed", 1);
                break;

            case EnemyState.Attack:
                // ���� ������ �� ����
                rb.velocity = Vector2.zero;
                break;
        }
    }

    // ���� ������Ʈ �޼���
    public void Update()
    {
        if (player == null) return;

        // �÷��̾���� �Ÿ� ���
        float distanceToPlayer = Vector2.Distance(enemyTransform.position, player.position);

        switch (CurrentState)
        {
            case EnemyState.Idle:
                // �÷��̾� ���� �� �߰� ���·� ��ȯ
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Patrol:
                // �÷��̾� ���� �� �߰� ���·� ��ȯ
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                // �÷��̾� ���� ������ ����� ��� ���·�
                if (distanceToPlayer > detectionRange)
                {
                    ChangeState(EnemyState.Idle);
                    return;
                }

                // ���� ���� ���� ������ ���� ���·�
                if (distanceToPlayer <= attackRange)
                {
                    ChangeState(EnemyState.Attack);
                    return;
                }

                // �÷��̾� �߰�
                Vector2 direction = (player.position - enemyTransform.position).normalized;
                rb.velocity = direction * moveSpeed;
                break;

            case EnemyState.Attack:
                // ���� ������ ����� �߰� ���·�
                if (distanceToPlayer > attackRange)
                {
                    ChangeState(EnemyState.Chase);
                    return;
                }

                // ���� ����
                weaponController.Fire();
                break;
        }
    }
}