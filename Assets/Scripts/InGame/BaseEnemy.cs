using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// �� Ÿ�� ������
public enum EnemyType
{
    Minion,      // �̺�
    Elite,       // ������
    MidBoss,     // �߰� ����
    FinalBoss    // ���� ����
}

// �� Ư�� �������̽�
public interface IEnemyBehavior
{
    void SpecialAbility();     // Ư�� �ɷ�
    void TriggerEnrageMode();  // �г� ���
}

// �߻� �⺻ �� Ŭ����
public abstract class BaseEnemy : MonoBehaviour
{
    [Header("�⺻ �� �Ӽ�")]
    [SerializeField] protected EnemyType enemyType;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float detectionRange;

    // ���� ���� �ӽ� ������Ʈ
    protected EnemyStateMachine stateMachine;

    // ������Ʈ ����
    protected Rigidbody2D rb;
    protected Animator animator;
    protected WeaponController weaponController;

    // ���� ���� ���� ����
    protected int currentHealth;
    protected Transform player;

    // �ʱ�ȭ �޼���
    protected virtual void Awake()
    {
        // �⺻ ������Ʈ ����
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        weaponController = GetComponent<WeaponController>();

        // ���� �ӽ� �ʱ�ȭ
        stateMachine = new EnemyStateMachine(this);

        // �ʱ� ü�� ����
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        // �÷��̾� ã��
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected virtual void Update()
    {
        // ���� �ӽ� ������Ʈ
        stateMachine.Update();
    }

    // ���� ���� ó�� �޼���
    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // �ǰ� ����Ʈ �� ���� ����
        PlayHitEffect();

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (IsEnrageThresholdReached())
        {
            TriggerEnrageMode();
        }
    }

    // �г� ��� �Ӱ谪 üũ
    protected virtual bool IsEnrageThresholdReached()
    {
        // ü���� 30% ������ �� �г� ��� Ʈ����
        return currentHealth <= maxHealth * 0.3f;
    }

    // ���� ��� ó��
    protected virtual void Die()
    {
        // ��� �ִϸ��̼�
        animator.SetTrigger("Die");

        // ��� ������ ����
        SpawnDropItems();

        // ������Ʈ ����
        Destroy(gameObject, 1f);
    }

    // ������ ���
    protected virtual void SpawnDropItems()
    {
        // �� Ÿ�Կ� ���� ��� ������ ����
        switch (enemyType)
        {
            case EnemyType.Minion:
                // ���� Ȯ��, ���� ������
                break;
            case EnemyType.Elite:
                // �߰� Ȯ��, �߰� ��� ������
                break;
            case EnemyType.MidBoss:
            case EnemyType.FinalBoss:
                // ���� Ȯ��, ��� ������
                break;
        }
    }

    // �ǰ� ����Ʈ
    protected virtual void PlayHitEffect()
    {
        // ���� �ǰ� ����Ʈ
        animator.SetTrigger("Hit");
    }

    // �г� ��� Ʈ����
    protected virtual void TriggerEnrageMode()
    {
        // �̵� �ӵ� ����
        moveSpeed *= 1.5f;

        // ���ݷ� ����
        //weaponController.IncreaseDamage(1.5f);

        // �г� �ִϸ��̼�
        animator.SetTrigger("Enrage");
    }
}