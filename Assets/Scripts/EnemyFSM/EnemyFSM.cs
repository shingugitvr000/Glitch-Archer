using UnityEngine;
using UnityEngine.AI;

namespace EnemyAI
{
    public class EnemyFSM : MonoBehaviour
    {
        [Header("상태 에셋들")]
        public PatrolState patrolState;
        public ChaseState chaseState;
        public AttackState attackState;
        public CoverState coverState;

        [Header("타겟 및 패트롤")]
        public Transform player;
        public Transform[] patrolPoints;
        public Transform[] coverPoints;  // 엄폐 포인트 추가

        [Header("컴포넌트 참조")]
        public NavMeshAgent agent;
        public Animator animator;

        [Header("어그로 시스템")]
        public bool enableAggroSystem = true;       // 어그로 시스템 사용 여부
        public float aggroActivationRange = 30f;    // 어그로 활성화 범위
        public bool instantAggroOnDamage = true;    // 피해시 즉시 어그로

        [Header("체력 시스템")]
        public float maxHealth = 100f;
        public float currentHealth = 100f;

        [Header("디버그")]
        public bool showDebugInfo = false;
        public bool showGizmos = true;

        // 매니저들
        public EnemyStateManager StateManager { get; private set; }
        public EnemyAnimationController AnimationController { get; private set; }

        // 컴포넌트 속성들
        public NavMeshAgent Agent => agent;
        public Animator Anim => animator;

        // 이벤트들
        public System.Action<float> OnHealthChanged;
        public System.Action OnDeath;
        public System.Action OnAggroActivated;

        void Awake()
        {
            // 컴포넌트 자동 할당
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            // 플레이어 자동 찾기
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.transform;
            }

            // 매니저들 초기화
            StateManager = gameObject.AddComponent<EnemyStateManager>();
            AnimationController = gameObject.AddComponent<EnemyAnimationController>();

            // 체력 초기화
            currentHealth = maxHealth;
        }

        void Start()
        {
            // 상태 매니저 초기화
            StateManager.Initialize(this);
            AnimationController.Initialize(this);

            // 초기 상태 설정 (패트롤)
            if (patrolState != null)
            {
                StateManager.ChangeState(patrolState);
            }
            else
            {
                Debug.LogError($"[{name}] PatrolState가 할당되지 않았습니다!");
            }
        }

        void Update()
        {
            // 현재 상태 업데이트
            StateManager?.UpdateCurrentState();

            // 디버그 정보 표시
            if (showDebugInfo)
            {
                DisplayDebugInfo();
            }
        }

        void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // 현재 상태의 기즈모 그리기
            StateManager?.DrawCurrentStateGizmos();

            // 어그로 활성화 범위
            if (enableAggroSystem)
            {
                Gizmos.color = new Color(1f, 0f, 1f, 0.2f); // 반투명 마젠타
                Gizmos.DrawSphere(transform.position, aggroActivationRange);
            }
        }

        // === 피해 및 어그로 시스템 ===

        /// <summary>
        /// 적이 피해를 받았을 때 호출되는 메서드
        /// </summary>
        /// <param name="damage">받은 피해량</param>
        /// <param name="damageSource">피해를 준 오브젝트 (선택사항)</param>
        public void TakeDamage(float damage, GameObject damageSource = null)
        {
            // 체력 감소
            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnHealthChanged?.Invoke(currentHealth);

            Debug.Log($"[{name}] 피해 {damage} 받음! 현재 체력: {currentHealth}/{maxHealth}");

            // 죽음 처리
            if (currentHealth <= 0)
            {
                HandleDeath();
                return;
            }

            // 피해를 준 대상이 플레이어인지 확인
            bool isPlayerDamage = damageSource != null &&
                                 (damageSource.CompareTag("Player") ||
                                  damageSource.transform == player);

            // 어그로 활성화
            if (enableAggroSystem && (isPlayerDamage || instantAggroOnDamage))
            {
                ActivateAggro();
            }

            // 피해 받은 상황에 따른 상태 전환
            HandleDamageStateTransition(damageSource);
        }

        /// <summary>
        /// 어그로 상태 활성화
        /// </summary>
        public void ActivateAggro()
        {
            if (!enableAggroSystem) return;

            // 플레이어가 어그로 범위 내에 있는지 확인
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= aggroActivationRange)
                {
                    // ChaseState에 어그로 활성화 알림
                    if (chaseState != null)
                    {
                        chaseState.ActivateAggro();
                    }

                    OnAggroActivated?.Invoke();
                    Debug.Log($"[{name}] 어그로 활성화! 플레이어를 끈질기게 추적합니다.");

                    // 현재 패트롤 상태라면 즉시 추적 상태로 전환
                    if (StateManager.CurrentState == patrolState)
                    {
                        StateManager.ChangeState(chaseState);
                    }
                }
                else
                {
                    Debug.Log($"[{name}] 플레이어가 어그로 범위를 벗어남 ({distanceToPlayer:F1}m > {aggroActivationRange}m)");
                }
            }
        }

        /// <summary>
        /// 피해받은 후 상태 전환 처리
        /// </summary>
        private void HandleDamageStateTransition(GameObject damageSource)
        {
            if (player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            float healthPercentage = currentHealth / maxHealth;

            // 체력이 낮으면 엄폐 상태 고려
            if (healthPercentage < 0.3f && coverState != null && coverPoints != null && coverPoints.Length > 0)
            {
                Debug.Log($"[{name}] 체력 부족으로 엄폐 시도");
                StateManager.ChangeState(coverState);
                return;
            }

            // 거리에 따른 상태 전환
            if (distanceToPlayer <= attackState.attackRange)
            {
                // 공격 범위 내 - 공격 상태
                if (StateManager.CurrentState != attackState)
                {
                    StateManager.ChangeState(attackState);
                }
            }
            else
            {
                // 공격 범위 밖 - 추적 상태
                if (StateManager.CurrentState != chaseState)
                {
                    StateManager.ChangeState(chaseState);
                }
            }
        }

        /// <summary>
        /// 죽음 처리
        /// </summary>
        private void HandleDeath()
        {
            Debug.Log($"[{name}] 사망!");

            // 에이전트 정지
            if (agent != null)
            {
                agent.enabled = false;
            }

            // 애니메이션 트리거
            if (animator != null)
            {
                animator.SetTrigger("Death");
                animator.SetBool("IsDead", true);
            }

            // 사망 이벤트 발생
            OnDeath?.Invoke();

            // 상태 매니저 비활성화
            if (StateManager != null)
            {
                StateManager.enabled = false;
            }

            // 스크립트 비활성화 (또는 오브젝트 제거)
            this.enabled = false;

            // 일정 시간 후 오브젝트 제거 (선택사항)
            // Destroy(gameObject, 5f);
        }

        // === 공개 헬퍼 메서드들 ===

        /// <summary>
        /// 체력 회복
        /// </summary>
        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth);
            Debug.Log($"[{name}] {amount} 체력 회복! 현재: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// 체력 비율 반환 (0.0 ~ 1.0)
        /// </summary>
        public float GetHealthPercentage()
        {
            return currentHealth / maxHealth;
        }

        /// <summary>
        /// 어그로 상태인지 확인
        /// </summary>
        public bool IsAggro()
        {
            return chaseState != null && chaseState.IsAggro;
        }

        // 상태 전환을 위한 헬퍼 메서드들
        public void GoToPatrol() => StateManager?.ChangeState(patrolState);
        public void GoToChase() => StateManager?.ChangeState(chaseState);
        public void GoToAttack() => StateManager?.ChangeState(attackState);
        public void GoToCover() => StateManager?.ChangeState(coverState);

        /// <summary>
        /// 강제로 특정 상태로 전환 (디버그용)
        /// </summary>
        public void ForceState(EnemyState targetState)
        {
            if (targetState != null)
            {
                StateManager?.ChangeState(targetState);
                Debug.Log($"[{name}] 강제로 {targetState.GetType().Name} 상태로 전환");
            }
        }

        /// <summary>
        /// 디버그 정보 표시
        /// </summary>
        private void DisplayDebugInfo()
        {
            if (StateManager == null) return;

            string debugText = $"[{name}]\n";
            debugText += $"상태: {StateManager.CurrentState?.GetType().Name}\n";
            debugText += $"체력: {currentHealth:F0}/{maxHealth:F0} ({GetHealthPercentage():P0})\n";
            debugText += $"어그로: {IsAggro()}\n";

            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                debugText += $"플레이어 거리: {distance:F1}m\n";
            }

            // UI나 콘솔에 표시 (여기서는 로그로 대체)
            if (Time.frameCount % 60 == 0) // 1초에 한 번만
            {
                Debug.Log(debugText);
            }
        }

        // === 에디터 헬퍼 메서드들 ===

        void OnValidate()
        {
            // 에디터에서 값 변경 시 검증
            maxHealth = Mathf.Max(1f, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            aggroActivationRange = Mathf.Max(0f, aggroActivationRange);
        }
    }
}