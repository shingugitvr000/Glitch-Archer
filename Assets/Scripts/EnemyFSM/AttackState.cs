using UnityEngine;
using System.Collections;

namespace EnemyAI
{
    [CreateAssetMenu(fileName = "New Attack State", menuName = "Enemy AI/States/Attack State")]
    public class AttackState : EnemyState
    {
        [Header("공격 설정")]
        public float attackRange = 8f;
        public float attackCooldown = 3f;           // 연사 후 대기 시간
        public float aimTime = 0.5f;                // 조준 시간 (단축)
        public bool stopToAttack = true;            // 공격 시 정지 여부

        [Header("연사 설정")]
        public int burstCount = 6;                  // 연사 횟수 (5-6발)
        public float burstInterval = 0.15f;         // 연사 간격 (빠르게)
        public float firstShotDelay = 0.5f;         // 첫 발사 전 대기

        [Header("총알 설정")]
        public GameObject bulletPrefab;             // 총알 프리팹
        public Transform firePoint;                 // 발사 지점 (총구)
        public float bulletSpeed = 20f;             // 총알 속도
        public float bulletLifetime = 5f;           // 총알 수명

        [Header("위치 및 이동")]
        public float optimalRange = 6f;             // 최적 공격 거리
        public float repositionRange = 2f;          // 재배치 거리
        public float maxAttackTime = 15f;           // 최대 공격 시간

        // 런타임 데이터
        private float lastAttackTime;
        private float aimStartTime;
        private float attackStateStartTime;
        private bool isAiming = false;
        private bool isAttacking = false;
        private bool isBurstFiring = false;         // 연사 중인지
        private int currentBurstCount = 0;
        private Coroutine burstFireCoroutine;       // 연사 코루틴

        public override void EnterState(EnemyFSM enemy)
        {
            base.EnterState(enemy);

            // 즉시 정지 - 미끄러짐 방지
            enemy.Agent.velocity = Vector3.zero;
            enemy.Agent.SetDestination(enemy.transform.position);
            enemy.Agent.speed = 0f;

            StartAttackState(enemy);
        }

        public override void ExitState(EnemyFSM enemy)
        {
            base.ExitState(enemy);

            // 연사 코루틴 중단
            if (burstFireCoroutine != null)
            {
                enemy.StopCoroutine(burstFireCoroutine);
                burstFireCoroutine = null;
            }

            // 상태 종료 시 속도 복원
            enemy.Agent.speed = 3.5f; // 기본 속도로 복원

            // 상태 초기화
            isAiming = false;
            isAttacking = false;
            isBurstFiring = false;
        }

        public override void UpdateState(EnemyFSM enemy)
        {
            if (enemy.player == null)
            {
                enemy.StateManager.ChangeState(enemy.patrolState);
                return;
            }

            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);

            // 플레이어가 너무 멀어지면 추적으로 전환
            if (distanceToPlayer > attackRange * 1.5f)
            {
                enemy.StateManager.ChangeState(enemy.chaseState);
                return;
            }

            // 최대 공격 시간 초과 시 추적으로 전환
            if (Time.time - attackStateStartTime > maxAttackTime)
            {
                enemy.StateManager.ChangeState(enemy.chaseState);
                return;
            }

            // 공격 로직 처리
            HandleAttack(enemy, distanceToPlayer);
        }

        public override void SetAnimationParameters(EnemyFSM enemy)
        {
            var animController = enemy.AnimationController;
            if (animController == null) return;

            // 공격 중: 정지 상태, 전투 중
            animController.SetBool("InCombat", true);
        }

        public override float GetMoveSpeed()
        {
            return 0f; // 공격 중에는 절대 이동하지 않음
        }

        public override float GetTurnSpeed()
        {
            return 90f; // 공격 시 느린 회전 (플레이어 조준용)
        }

        private void StartAttackState(EnemyFSM enemy)
        {
            attackStateStartTime = Time.time;
            lastAttackTime = 0f;
            aimStartTime = 0f;
            isAiming = false;
            isAttacking = false;
            isBurstFiring = false;
            currentBurstCount = 0;
            burstFireCoroutine = null;

            // 발사 지점 설정 (없으면 자동으로 찾기)
            if (firePoint == null)
            {
                FindFirePoint(enemy);
            }

            Debug.Log($"[{enemy.name}] 공격 상태 시작!");
        }

        private void FindFirePoint(EnemyFSM enemy)
        {
            // 총구 찾기 시도
            Transform[] childTransforms = enemy.GetComponentsInChildren<Transform>();
            foreach (Transform child in childTransforms)
            {
                if (child.name.ToLower().Contains("muzzle") ||
                    child.name.ToLower().Contains("firepoint") ||
                    child.name.ToLower().Contains("gunpoint"))
                {
                    firePoint = child;
                    break;
                }
            }

            // 못 찾으면 캐릭터 위치에서 약간 앞쪽으로 설정
            if (firePoint == null)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(enemy.transform);
                firePointObj.transform.localPosition = new Vector3(0, 1.5f, 1f);
                firePoint = firePointObj.transform;
            }
        }

        private void HandleAttack(EnemyFSM enemy, float distanceToPlayer)
        {
            // 매 프레임마다 강제 정지 - 미끄러짐 완전 방지
            enemy.Agent.velocity = Vector3.zero;
            enemy.Agent.SetDestination(enemy.transform.position);

            // 플레이어를 향해 회전만 수행
            RotateTowardsPlayer(enemy);

            // 연사 중이면 다른 행동 하지 않음
            if (isBurstFiring) return;

            // 공격 쿨다운 확인
            if (Time.time - lastAttackTime < attackCooldown) return;

            // 조준 시작
            if (!isAiming && !isAttacking)
            {
                StartAiming();
            }

            // 조준 완료 후 연사 시작
            if (isAiming && Time.time - aimStartTime >= aimTime)
            {
                StartBurstFire(enemy);
            }
        }

        private void StartAiming()
        {
            isAiming = true;
            aimStartTime = Time.time;
            Debug.Log("조준 시작...");
        }

        private void StartBurstFire(EnemyFSM enemy)
        {
            isAiming = false;
            isAttacking = true;
            isBurstFiring = true;
            currentBurstCount = 0;

            // 연사 코루틴 시작
            burstFireCoroutine = enemy.StartCoroutine(BurstFireCoroutine(enemy));
        }

        private IEnumerator BurstFireCoroutine(EnemyFSM enemy)
        {
            Debug.Log($"[{enemy.name}] 연사 시작! {burstCount}발");

            // 첫 발사 전 약간의 대기
            yield return new WaitForSeconds(firstShotDelay);

            for (int i = 0; i < burstCount; i++)
            {
                // 플레이어가 여전히 사정거리 내에 있는지 확인
                if (enemy.player == null) break;

                float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
                if (distanceToPlayer > attackRange * 1.5f) break;

                // 총알 발사
                FireBullet(enemy, i + 1);

                // Fire 애니메이션 트리거
                var animController = enemy.AnimationController;
                if (animController != null)
                {
                    animController.SetTrigger("Fire");
                }

                currentBurstCount++;

                // 마지막 발사가 아니면 대기
                if (i < burstCount - 1)
                {
                    yield return new WaitForSeconds(burstInterval);
                }
            }

            // 연사 완료
            Debug.Log($"[{enemy.name}] 연사 완료! {currentBurstCount}발 발사");

            isAttacking = false;
            isBurstFiring = false;
            lastAttackTime = Time.time; // 쿨다운 시작
            burstFireCoroutine = null;
        }

        private void FireBullet(EnemyFSM enemy, int shotNumber)
        {
            if (bulletPrefab == null || firePoint == null)
            {
                Debug.LogWarning($"[{enemy.name}] 총알 프리팹 또는 발사점이 설정되지 않았습니다!");
                return;
            }

            // 플레이어 방향 계산 (약간의 예측 포함)
            Vector3 targetPosition = enemy.player.position + enemy.player.GetComponent<CharacterController>()?.velocity * 0.1f ?? Vector3.zero;
            Vector3 fireDirection = (targetPosition - firePoint.position).normalized;

            // 총알 생성
            GameObject bullet = Object.Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(fireDirection));

            // 총알에 속도 적용
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.velocity = fireDirection * bulletSpeed;
            }

            // 총알 수명 설정
            Object.Destroy(bullet, bulletLifetime);

            Debug.Log($"[{enemy.name}] 총알 발사 {shotNumber}/{burstCount}");

            // 간단한 히트스캔도 함께 (즉시 명중 확인)
            RaycastHit hit;
            if (Physics.Raycast(firePoint.position, fireDirection, out hit, attackRange))
            {
                if (hit.transform == enemy.player)
                {
                    Debug.Log($"[{enemy.name}] 플레이어 명중! (Shot {shotNumber})");
                    // 여기에 플레이어 피해 처리 로직 추가
                }
            }
        }

        private void RotateTowardsPlayer(EnemyFSM enemy)
        {
            Vector3 direction = (enemy.player.position - enemy.transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                float rotationSpeed = GetTurnSpeed() * Time.deltaTime;
                enemy.transform.rotation = Quaternion.RotateTowards(enemy.transform.rotation, targetRotation, rotationSpeed);
            }
        }

        public override void DrawGizmos(EnemyFSM enemy)
        {
            if (enemy == null) return;

            // 공격 범위
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(enemy.transform.position, attackRange);

            // 최적 거리
            Gizmos.color = new Color(1f, 0.5f, 0f); // 오렌지색
            Gizmos.DrawWireSphere(enemy.transform.position, optimalRange);

            // 플레이어와의 연결선
            if (enemy.player != null)
            {
                Gizmos.color = isBurstFiring ? Color.red : (isAiming ? new Color(1f, 0.5f, 0f) : Color.white);
                Gizmos.DrawLine(enemy.transform.position + Vector3.up * 1.5f, enemy.player.position);
            }

            // 발사 지점 표시
            if (firePoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(firePoint.position, 0.1f);

                // 발사 방향 표시
                if (enemy.player != null)
                {
                    Vector3 fireDirection = (enemy.player.position - firePoint.position).normalized;
                    Gizmos.DrawLine(firePoint.position, firePoint.position + fireDirection * attackRange);
                }
            }
        }

        public void ResetRuntimeData()
        {
            lastAttackTime = 0f;
            aimStartTime = 0f;
            attackStateStartTime = 0f;
            isAiming = false;
            isAttacking = false;
            isBurstFiring = false;
            currentBurstCount = 0;
            burstFireCoroutine = null;
        }
    }
}