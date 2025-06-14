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

    // 공유 데이터 (각 적마다 독립적)
    public float currentHealth;
    public float lastAttackTime;
    public float lastSeenTime;
    public int currentPatrolIndex;
    public bool isWaiting;
    public float waitTimer;
    public Vector3 coverPosition;
    public float coverStartTime;

    // 히트 상태 추가
    private bool isHit = false;
    private float hitTimer = 0f;
    private const float HIT_DURATION = 0.5f; // 히트 애니메이션 시간


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

        // 히트 중이면 아무것도 안 함
        if (isHit)
        {
            hitTimer -= Time.deltaTime;
            if (hitTimer <= 0)
            {
                isHit = false;
                // 히트 끝나면 이동 재개
                if (Agent != null && currentState != null)
                {
                    Agent.isStopped = false;
                }
            }
            return;
        }

        currentState?.Update();
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (Anim == null) return;

        // 죽었으면 애니메이션 업데이트 안 함
        if (currentHealth <= 0) return;

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
        Debug.Log($"[{name}] 현재 상태: {currentState?.GetType().Name}");


        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // 히트 애니메이션 추가
        if (Anim != null)
        {
            if (damage >= 50f)
                Anim.SetTrigger("TakeDamage04");
            else if (damage >= 35f)
                Anim.SetTrigger("TakeDamage03");
            else if (damage >= 20f)
                Anim.SetTrigger("TakeDamage02");
            else
                Anim.SetTrigger("TakeDamage01");

            // 히트 상태로 전환 (이동 정지)
            isHit = true;
            hitTimer = HIT_DURATION;

            if (Agent != null)
            {
                Agent.isStopped = true; // 이동 정지
            }
        }



        // 플레이어 참조가 없으면 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log($"[{name}] 플레이어 참조 설정됨");
            }
            else
            {
                Debug.LogWarning($"[{name}] 플레이어를 찾을 수 없음!");
                return;
            }
        }

        // 맞으면 무조건 플레이어를 찾아서 추적 시작
        if (currentState is PatrolState)
        {
            Debug.Log($"[{name}] PatrolState에서 추적 상태로 전환 시도");

            // 타입별로 다른 추적 상태로 전환
            if (this is AssaultController)
                ChangeState<AssaultChaseState>();
            else if (this is MidRangeController)
                ChangeState<TacticalChaseState>();
            else if (this is SniperController)
                ChangeState<CautiousChaseState>();
            else
                ChangeState<ChaseState>(); // 기본 ChaseState (혹시 모를 경우)

            Debug.Log($"[{name}] 상태 전환 완료 - 새 상태: {currentState?.GetType().Name}");
            Debug.Log($"[{name}] 플레이어 위치: {player.position}");
        }
        else
        {
            Debug.Log($"[{name}] PatrolState가 아님 - 현재: {currentState?.GetType().Name}");
        }

        // 타입별 특수 반응
        Debug.Log($"[{name}] OnTakeDamage 호출 전");
        OnTakeDamage(damage);
        Debug.Log($"[{name}] OnTakeDamage 호출 후 - 상태: {currentState?.GetType().Name}");

        AlertNearbyEnemies();
    }


    // 원거리 공격에 대한 반응 (타입별로 다르게 처리)
    protected virtual void HandleLongRangeResponse()
    {
        // 기본 반응: 마지막으로 본 위치로 이동하여 수색
        if (currentState is PatrolState)
        {
            ChangeState<ChaseState>();
            Debug.Log($"[{name}] 플레이어 수색 모드로 전환");
        }
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

        // 죽음 애니메이션
        if (Anim != null)
        {
            Anim.SetTrigger("Die");
            Anim.SetBool("IsDead", true);
            Anim.SetBool("InCombat", false); // InCombat 강제로 끄기
        }

        // AI 정지
        if (Agent != null) Agent.enabled = false;

        // 상태 정리
        currentState?.Exit();
        currentState = null;

        // 스크립트 비활성화
        this.enabled = false;

        // 3초 후 삭제
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