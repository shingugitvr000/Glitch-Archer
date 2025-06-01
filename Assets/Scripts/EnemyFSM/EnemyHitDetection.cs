using UnityEngine;

namespace EnemyAI
{
    public class EnemyHitDetection : MonoBehaviour
    {
        [Header("피격 설정")]
        public float maxHealth = 100f;             // 최대 체력
        public float currentHealth;                // 현재 체력
        public bool isInvulnerable = false;        // 무적 상태
        public float invulnerabilityTime = 0.5f;   // 무적 시간

        [Header("반응 설정")]
        public bool alertOnHit = true;             // 피격 시 경계 상태로 전환
        public float alertRadius = 20f;            // 주변 적들에게 알림 범위
        public LayerMask enemyLayerMask = -1;      // 적 레이어

        [Header("이펙트")]
        public GameObject hitEffect;              // 피격 이펙트
        public GameObject deathEffect;            // 죽음 이펙트
        public AudioClip hitSound;                // 피격 사운드
        public AudioClip deathSound;              // 죽음 사운드

        // 컴포넌트 참조
        private EnemyFSM enemyFSM;
        private AudioSource audioSource;
        private float lastHitTime;

        // 이벤트
        public System.Action<float> OnHealthChanged;
        public System.Action OnDeath;

        void Start()
        {
            // 컴포넌트 가져오기
            enemyFSM = GetComponent<EnemyFSM>();
            audioSource = GetComponent<AudioSource>();

            // 초기 체력 설정
            currentHealth = maxHealth;

            if (enemyFSM == null)
            {
                Debug.LogError($"[{gameObject.name}] EnemyFSM을 찾을 수 없습니다!");
            }
        }

        void Update()
        {
            // 무적 시간 해제
            if (isInvulnerable && Time.time - lastHitTime > invulnerabilityTime)
            {
                isInvulnerable = false;
            }
        }

        // 데미지 받기
        public void TakeDamage(float damage, Transform attacker = null)
        {
            // 무적 상태이거나 이미 죽었으면 무시
            if (isInvulnerable || currentHealth <= 0) return;

            // 데미지 적용
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            // 무적 상태 활성화
            isInvulnerable = true;
            lastHitTime = Time.time;

            Debug.Log($"[{gameObject.name}] {damage} 데미지 받음! 현재 체력: {currentHealth}/{maxHealth}");

            // 체력 변경 이벤트
            OnHealthChanged?.Invoke(currentHealth);

            // 피격 이펙트 및 사운드
            PlayHitEffects();

            // 죽음 처리
            if (currentHealth <= 0)
            {
                HandleDeath(attacker);
            }
            else
            {
                // 살아있으면 피격 반응
                HandleHitReaction(attacker);
            }
        }

        // 피격 반응 처리
        private void HandleHitReaction(Transform attacker)
        {
            if (enemyFSM == null) return;

            // 공격자가 플레이어인 경우 즉시 추적 및 공격
            if (attacker != null && attacker.CompareTag("Player"))
            {
                // 플레이어 참조 설정
                enemyFSM.player = attacker;

                // 즉시 추적 상태로 전환
                if (enemyFSM.chaseState != null)
                {
                    enemyFSM.StateManager.ForceChangeState(enemyFSM.chaseState);
                    Debug.Log($"[{gameObject.name}] 플레이어에게 피격! 즉시 추적 시작!");
                }
            }

            // 주변 적들에게 경고
            if (alertOnHit)
            {
                AlertNearbyEnemies(attacker);
            }
        }

        // 주변 적들에게 경고
        private void AlertNearbyEnemies(Transform attacker)
        {
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, alertRadius, enemyLayerMask);

            foreach (Collider enemyCollider in nearbyEnemies)
            {
                if (enemyCollider.gameObject == gameObject) continue; // 자기 자신 제외

                EnemyFSM nearbyEnemy = enemyCollider.GetComponent<EnemyFSM>();
                if (nearbyEnemy != null && attacker != null)
                {
                    // 플레이어 참조 설정
                    nearbyEnemy.player = attacker;

                    // 추적 상태로 전환 (패트롤 중인 경우만)
                    if (nearbyEnemy.StateManager.CurrentState == nearbyEnemy.patrolState)
                    {
                        nearbyEnemy.StateManager.ChangeState(nearbyEnemy.chaseState);
                        Debug.Log($"[{nearbyEnemy.name}] 동료 피격 감지! 플레이어 추적 시작!");
                    }
                }
            }
        }

        // 죽음 처리
        private void HandleDeath(Transform attacker)
        {
            Debug.Log($"[{gameObject.name}] 죽음!");

            // 죽음 이벤트
            OnDeath?.Invoke();

            // 죽음 이펙트 및 사운드
            PlayDeathEffects();

            // FSM 비활성화
            if (enemyFSM != null)
            {
                enemyFSM.enabled = false;
            }

            // NavMeshAgent 비활성화
            var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = false;
            }

            // 애니메이션 트리거 (죽음)
            var animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Die");
                animator.SetBool("IsDead", true);
            }

            // 콜라이더 비활성화 (선택사항)
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // 일정 시간 후 오브젝트 제거
            Destroy(gameObject, 5f);
        }

        // 피격 이펙트 재생
        private void PlayHitEffects()
        {
            // 피격 이펙트
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position + Vector3.up * 1f, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // 피격 사운드
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }

        // 죽음 이펙트 재생
        private void PlayDeathEffects()
        {
            // 죽음 이펙트
            if (deathEffect != null)
            {
                GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
                Destroy(effect, 5f);
            }

            // 죽음 사운드
            if (deathSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
        }

        // 체력 회복
        public void Heal(float amount)
        {
            if (currentHealth <= 0) return; // 죽은 상태에서는 회복 불가

            currentHealth += amount;
            currentHealth = Mathf.Min(maxHealth, currentHealth);

            OnHealthChanged?.Invoke(currentHealth);
            Debug.Log($"[{gameObject.name}] {amount} 체력 회복! 현재 체력: {currentHealth}/{maxHealth}");
        }

        // 체력 비율 반환
        public float GetHealthRatio()
        {
            return currentHealth / maxHealth;
        }

        // 죽었는지 확인
        public bool IsDead()
        {
            return currentHealth <= 0;
        }

        // 기즈모 그리기
        private void OnDrawGizmosSelected()
        {
            // 경고 범위 표시
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, alertRadius);
        }
    }
}