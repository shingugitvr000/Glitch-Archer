using UnityEngine;
using System;

// AI ���� ������
public enum EnemyState
{
    Idle,           // ��� ����
    Patrol,         // ���� ����
    Chase,          // �߰� ����
    Attack,         // ���� ����
    Hurt,           // �ǰ� ����
    Die             // ��� ����
}

public class EnemyAI : MonoBehaviour
{
    // ���� ���� ����
    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    // �̵� �� ���� ���� ����
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float patrolRadius = 10f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int maxHealth = 100;

    // ������Ʈ ����
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private WeaponController weaponController;

    // ���� ���� ����
    private float lastAttackTime;
    private Vector2 patrolStartPosition;
    private int currentHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        weaponController = GetComponent<WeaponController>();

        // �ʱ� ü�� ����
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // �÷��̾� ã��
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // �ʱ� ���� ���� ��ġ ����
        patrolStartPosition = transform.position;
    }

    private void Update()
    {
        // ���� ���¿� ���� �ൿ ������Ʈ
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

    // ���� ��ȯ �޼���
    private void ChangeState(EnemyState newState)
    {
        // ���� ���� ���� ����
        ExitCurrentState();

        // ���� ����
        CurrentState = newState;

        // �� ���� ���� ����
        EnterNewState();
    }

    // ��� ���� ������Ʈ
    private void UpdateIdleState()
    {
        // �÷��̾���� �Ÿ� ���
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // �÷��̾� ���� ���� ���� ������ �߰� ���·� ��ȯ
        if (distanceToPlayer <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
        }
        else
        {
            // ���� �ð� �� ���� ���·� ��ȯ
            ChangeState(EnemyState.Patrol);
        }
    }

    // ���� ���� ������Ʈ
    private void UpdatePatrolState()
    {
        // �÷��̾���� �Ÿ� ���
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // �÷��̾� ���� ���� ���� ������ �߰� ���·� ��ȯ
        if (distanceToPlayer <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // ���� ����
        // �ʱ� ��ġ�� �߽����� �����ϰ� �̵�
        Vector2 patrolTarget = patrolStartPosition +
            new Vector2(
                UnityEngine.Random.Range(-patrolRadius, patrolRadius),
                UnityEngine.Random.Range(-patrolRadius, patrolRadius)
            );

        // ��ǥ �������� �̵�
        MoveTowards(patrolTarget, moveSpeed);
    }

    // �߰� ���� ������Ʈ
    private void UpdateChaseState()
    {
        // �÷��̾���� �Ÿ� ���
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // �÷��̾� ���� ������ ����� ��� ���·� ��ȯ
        if (distanceToPlayer > detectionRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // ���� ���� ���� ������ ���� ���·� ��ȯ
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        // �÷��̾� �߰�
        MoveTowards(player.position, chaseSpeed);
    }

    // ���� ���� ������Ʈ
    private void UpdateAttackState()
    {
        // �÷��̾���� �Ÿ� ���
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // ���� ������ ����� �߰� ���·� ��ȯ
        if (distanceToPlayer > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // ���� ��ٿ� üũ
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // ���� ����
            Attack();
            lastAttackTime = Time.time;
        }
    }

    // �ǰ� ���� ������Ʈ
    private void UpdateHurtState()
    {
        // �ǰ� �ִϸ��̼� ��� ���� ����
        // ���� �ð� �� ���� ���·� �����ϰų� �ٸ� ���·� ��ȯ
    }

    // ��� ���� ������Ʈ
    private void UpdateDieState()
    {
        // ��� �ִϸ��̼� ���
        // ������Ʈ ���� ���� ����
        Destroy(gameObject);
    }

    // ��ǥ �������� �̵�
    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        rb.velocity = direction * speed;

        // �ִϸ����Ϳ� �̵� ���� ����
        animator.SetFloat("Speed", rb.velocity.magnitude);
    }

    // ���� �޼���
    private void Attack()
    {
        // ���� ��Ʈ�ѷ��� ���� �� �߻�
        weaponController.Fire();

        // ���� �ִϸ��̼� Ʈ����
        animator.SetTrigger("Attack");
    }

    // ���� ���� �� ȣ��Ǵ� �޼���
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            // ü���� 0 ���ϸ� ��� ���·� ��ȯ
            ChangeState(EnemyState.Die);
        }
        else
        {
            // �ǰ� ���·� ��ȯ
            ChangeState(EnemyState.Hurt);
        }
    }

    // ���� ���� ���� ����
    private void ExitCurrentState()
    {
        switch (CurrentState)
        {
            case EnemyState.Patrol:
                // ���� ���� ���� �� �ӵ� �ʱ�ȭ ��
                rb.velocity = Vector2.zero;
                break;
            case EnemyState.Chase:
                // �߰� ���� ���� �� �ӵ� �ʱ�ȭ ��
                rb.velocity = Vector2.zero;
                break;
        }
    }

    // �� ���� ���� ����
    private void EnterNewState()
    {
        switch (CurrentState)
        {
            case EnemyState.Idle:
                // ��� ���� ���� �� �ִϸ��̼� ��
                animator.SetFloat("Speed", 0);
                break;
            case EnemyState.Patrol:
                // ���� ���� ���� �� �ʱ� ����
                break;
        }
    }
}

