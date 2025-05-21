using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 적 타입 열거형
public enum EnemyType
{
    Minion,      // 쫄병
    Elite,       // 정예병
    MidBoss,     // 중간 보스
    FinalBoss    // 최종 보스
}

// 적 특성 인터페이스
public interface IEnemyBehavior
{
    void SpecialAbility();     // 특수 능력
    void TriggerEnrageMode();  // 분노 모드
}

// 추상 기본 적 클래스
public abstract class BaseEnemy : MonoBehaviour
{
    [Header("기본 적 속성")]
    [SerializeField] protected EnemyType enemyType;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float detectionRange;

    // 공통 상태 머신 컴포넌트
    protected EnemyStateMachine stateMachine;

    // 컴포넌트 참조
    protected Rigidbody2D rb;
    protected Animator animator;
    protected WeaponController weaponController;

    // 현재 상태 관련 변수
    protected int currentHealth;
    protected Transform player;

    // 초기화 메서드
    protected virtual void Awake()
    {
        // 기본 컴포넌트 설정
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        weaponController = GetComponent<WeaponController>();

        // 상태 머신 초기화
        stateMachine = new EnemyStateMachine(this);

        // 초기 체력 설정
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        // 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected virtual void Update()
    {
        // 상태 머신 업데이트
        stateMachine.Update();
    }

    // 공통 피해 처리 메서드
    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // 피격 이펙트 등 공통 로직
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

    // 분노 모드 임계값 체크
    protected virtual bool IsEnrageThresholdReached()
    {
        // 체력이 30% 이하일 때 분노 모드 트리거
        return currentHealth <= maxHealth * 0.3f;
    }

    // 공통 사망 처리
    protected virtual void Die()
    {
        // 사망 애니메이션
        animator.SetTrigger("Die");

        // 드랍 아이템 생성
        SpawnDropItems();

        // 오브젝트 제거
        Destroy(gameObject, 1f);
    }

    // 아이템 드랍
    protected virtual void SpawnDropItems()
    {
        // 적 타입에 따른 드랍 아이템 로직
        switch (enemyType)
        {
            case EnemyType.Minion:
                // 낮은 확률, 작은 아이템
                break;
            case EnemyType.Elite:
                // 중간 확률, 중간 등급 아이템
                break;
            case EnemyType.MidBoss:
            case EnemyType.FinalBoss:
                // 높은 확률, 고급 아이템
                break;
        }
    }

    // 피격 이펙트
    protected virtual void PlayHitEffect()
    {
        // 공통 피격 이펙트
        animator.SetTrigger("Hit");
    }

    // 분노 모드 트리거
    protected virtual void TriggerEnrageMode()
    {
        // 이동 속도 증가
        moveSpeed *= 1.5f;

        // 공격력 증가
        //weaponController.IncreaseDamage(1.5f);

        // 분노 애니메이션
        animator.SetTrigger("Enrage");
    }
}