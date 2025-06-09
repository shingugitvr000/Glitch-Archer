using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public abstract class EnemyController : MonoBehaviour
{
    [Header("기본 설정")]
    public Transform player;
    public Transform[] patrolPoints;
    public float maxHealth = 100f;

    [Header("공격 설정")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float damage = 25f;

    [Header("디버그")]
    public bool showDebugInfo = true;
    public bool showGizmos = true;

    // 컴포넌트
    public NavMeshAgent Agent { get; private set; }
    public Animator Anim { get; private set; }

    // 상태 시스템
    private Dictionary<System.Type, EnemyStateBase> states = new Dictionary<System.Type, EnemyStateBase>();
    private EnemyStateBase currentState;

    // 현재 상태 접근용 프로퍼티 추가
    public EnemyStateBase CurrentState => currentState;

    // 공용 데이터 (각 적마다 독립적)
    public float currentHealth;
    public float lastAttackTime;
    public float lastSeenTime;
    public int currentPatrolIndex;
    public bool isWaiting;
    public float waitTimer;
    public Vector3 coverPosition;
    public float coverStartTime;

    // 각 타입별로 오버라이드할 속성들
    public abstract float DetectionRange { get; }
    public abstract float AttackRange { get; }
    public abstract float LoseTargetRange { get; }
    public abstract float PatrolSpeed { get; }
    public abstract float ChaseSpeed { get; }
    public abstract float AttackCooldown { get; }
    public abstract float BulletSpeed { get; }

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;

        // 플레이어 자동 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // FirePoint 자동 설정
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0, 1.5f, 1f);
            firePoint = fp.transform;
        }

        // 상태들 등록
        RegisterStates();
    }

    protected abstract void RegisterStates();

    protected void AddState(EnemyStateBase state)
    {
        states[state.GetType()] = state;
    }

    public void ChangeState<T>() where T : EnemyStateBase
    {
        if (states.TryGetValue(typeof(T), out EnemyStateBase newState))
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter(this);
        }
    }

    void Update()
    {
        if (currentHealth <= 0) return;

        currentState?.Update();
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (Anim == null) return;

        float speed = Agent.velocity.magnitude;
        Anim.SetFloat("Speed", speed);

        bool inCombat = !(currentState is PatrolState);
        Anim.SetBool("InCombat", inCombat);
    }

    public virtual void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"[{name}] 데미지 {damage} 받음! 체력: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // 패트롤 중이면 추적으로 전환
        if (currentState is PatrolState)
        {
            ChangeState<ChaseState>();
        }

        // 타입별 특수 반응
        OnTakeDamage(damage);

        AlertNearbyEnemies();
    }

    protected virtual void OnTakeDamage(float damage) { }

    void AlertNearbyEnemies()
    {
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, 15f);
        foreach (var col in nearbyEnemies)
        {
            if (col.CompareTag("Enemy") && col.gameObject != gameObject)
            {
                var enemy = col.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.player = player;
                    if (enemy.CurrentState is PatrolState)
                    {
                        enemy.ChangeState<ChaseState>();
                    }
                }
            }
        }
    }

    void Die()
    {
        Debug.Log($"[{name}] 사망!");

        if (Agent != null) Agent.enabled = false;
        if (Anim != null)
        {
            Anim.SetTrigger("Die");
            Anim.SetBool("IsDead", true);
        }

        this.enabled = false;
        Destroy(gameObject, 5f);
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        currentState?.DrawGizmos();

        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);

        // 타입 표시
        Gizmos.color = GetTypeColor();
        Gizmos.DrawSphere(transform.position + Vector3.up * 3f, 0.3f);
    }

    protected abstract Color GetTypeColor();
}