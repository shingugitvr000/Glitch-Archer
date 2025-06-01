using UnityEngine;

namespace EnemyAI
{
    [CreateAssetMenu(fileName = "New Chase State", menuName = "Enemy AI/States/Chase State")]
    public class ChaseState : EnemyState
    {
        [Header("추적 설정")]
        public float chaseSpeed = 3.5f;
        public float maxChaseDistance = 25f;        // 일반 최대 추적 거리
        public float loseTargetTime = 5f;           // 목표를 잃는 시간
        public float attackRange = 8f;              // 공격 전환 거리

        [Header("어그로 시스템")]
        public float aggroChaseDistance = 50f;      // 어그로 상태일 때 최대 추적 거리
        public float aggroLoseTargetTime = 15f;     // 어그로 상태일 때 목표를 잃는 시간
        public float aggroDecayTime = 30f;          // 어그로가 자연적으로 감소하는 시간
        public bool infiniteAggroChase = false;     // 어그로 상태에서 무한 추적 여부

        [Header("추적 행동")]
        public bool useLastKnownPosition = true;    // 마지막 위치 추적
        public float searchTime = 3f;               // 마지막 위치에서 찾는 시간
        public float updatePathInterval = 0.2f;     // 경로 업데이트 간격

        // 런타임 데이터
        private float lastSeenTime;
        private Vector3 lastKnownPosition;
        private float pathUpdateTimer;
        private float searchTimer;
        private bool isSearching = false;

        // 어그로 시스템
        private bool isAggro = false;
        private float aggroStartTime;
        private float lastAggroTime;

        public override void EnterState(EnemyFSM enemy)
        {
            base.EnterState(enemy);

            // 추적 시작
            StartChase(enemy);
        }

        public override void UpdateState(EnemyFSM enemy)
        {
            if (enemy.player == null)
            {
                enemy.StateManager.ChangeState(enemy.patrolState);
                return;
            }

            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);

            // 어그로 상태 업데이트
            UpdateAggroState();

            // 현재 상태에 따른 최대 추적 거리 결정
            float currentMaxDistance = GetCurrentMaxChaseDistance();
            float currentLoseTime = GetCurrentLoseTargetTime();

            // 너무 멀어지면 포기 (어그로 상태가 아니거나 무한 추적이 아닐 때만)
            if (!infiniteAggroChase || !isAggro)
            {
                if (distanceToPlayer > currentMaxDistance)
                {
                    Debug.Log($"[{enemy.name}] 추적 거리 초과 ({distanceToPlayer:F1}m > {currentMaxDistance}m) - 포기");
                    enemy.StateManager.ChangeState(enemy.patrolState);
                    return;
                }
            }

            // 플레이어를 볼 수 있는지 확인
            if (CanSeePlayer(enemy))
            {
                // 플레이어 발견 - 직접 추적
                HandleDirectChase(enemy, distanceToPlayer);
            }
            else
            {
                // 플레이어를 놓침 - 마지막 위치 추적
                HandleLostTarget(enemy, currentLoseTime);
            }
        }

        public override void SetAnimationParameters(EnemyFSM enemy)
        {
            var animController = enemy.AnimationController;
            if (animController == null) return;

            // 추적 중: 이동 가능, 전투 아님 (아직 공격 범위 아님)
            animController.SetBool("InCombat", isAggro); // 어그로 상태면 전투 모드

            // Speed는 EnemyAnimationController에서 자동으로 처리됨
            // 추적 속도(3.5f)에 따라 블렌드 트리가 Walk/Sprint 애니메이션 선택
        }

        public override float GetMoveSpeed()
        {
            // 어그로 상태일 때 더 빠르게 추적
            return isAggro ? chaseSpeed * 1.2f : chaseSpeed;
        }

        public override float GetTurnSpeed()
        {
            return 180f; // 추적 시 빠른 회전
        }

        private void StartChase(EnemyFSM enemy)
        {
            lastSeenTime = Time.time;
            lastKnownPosition = enemy.player.position;
            pathUpdateTimer = 0f;
            searchTimer = 0f;
            isSearching = false;

            // 추적 속도로 설정
            enemy.Agent.speed = GetMoveSpeed();

            Debug.Log($"[{enemy.name}] 플레이어 추적 시작! (어그로: {isAggro})");
        }

        private void HandleDirectChase(EnemyFSM enemy, float distanceToPlayer)
        {
            // 플레이어를 볼 수 있음
            lastSeenTime = Time.time;
            lastKnownPosition = enemy.player.position;
            isSearching = false;
            searchTimer = 0f;

            // 어그로 상태라면 어그로 시간 갱신
            if (isAggro)
            {
                lastAggroTime = Time.time;
            }

            // 공격 거리에 들어오면 공격 상태로
            if (distanceToPlayer <= attackRange)
            {
                enemy.StateManager.ChangeState(enemy.attackState);
                return;
            }

            // 경로 업데이트 (너무 자주 하지 않기 위해)
            pathUpdateTimer += Time.deltaTime;
            if (pathUpdateTimer >= updatePathInterval)
            {
                enemy.Agent.SetDestination(enemy.player.position);
                pathUpdateTimer = 0f;
            }
        }

        private void HandleLostTarget(EnemyFSM enemy, float currentLoseTime)
        {
            float timeSinceSeen = Time.time - lastSeenTime;

            if (useLastKnownPosition && timeSinceSeen <= currentLoseTime)
            {
                // 마지막 위치로 이동
                if (!isSearching)
                {
                    isSearching = true;
                    enemy.Agent.SetDestination(lastKnownPosition);
                    Debug.Log($"[{enemy.name}] 마지막 목격 위치로 이동 (어그로: {isAggro})");
                }

                // 마지막 위치에 도착했으면 잠시 찾아보기
                if (HasReachedDestination(enemy))
                {
                    searchTimer += Time.deltaTime;
                    if (searchTimer >= searchTime)
                    {
                        // 찾기 포기, 패트롤로 복귀
                        Debug.Log($"[{enemy.name}] 플레이어 추적 포기");
                        ResetAggroState(); // 어그로 상태 해제
                        enemy.StateManager.ChangeState(enemy.patrolState);
                        return;
                    }
                }
            }
            else
            {
                // 추적 포기
                Debug.Log($"[{enemy.name}] 추적 시간 초과 - 포기");
                ResetAggroState(); // 어그로 상태 해제
                enemy.StateManager.ChangeState(enemy.patrolState);
            }
        }

        private void UpdateAggroState()
        {
            if (isAggro)
            {
                // 어그로 상태 자연 감소 체크
                float timeSinceLastAggro = Time.time - lastAggroTime;
                if (timeSinceLastAggro > aggroDecayTime)
                {
                    Debug.Log("어그로 상태 자연 해제");
                    ResetAggroState();
                }
            }
        }

        private float GetCurrentMaxChaseDistance()
        {
            return isAggro ? aggroChaseDistance : maxChaseDistance;
        }

        private float GetCurrentLoseTargetTime()
        {
            return isAggro ? aggroLoseTargetTime : loseTargetTime;
        }

        private bool CanSeePlayer(EnemyFSM enemy)
        {
            if (enemy.player == null) return false;

            Vector3 directionToPlayer = (enemy.player.position - enemy.transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);

            // 시야 확인 (추적 중에는 시야각 제한 없음)
            RaycastHit hit;
            Vector3 rayOrigin = enemy.transform.position + Vector3.up * 1.5f;

            if (Physics.Raycast(rayOrigin, directionToPlayer, out hit, distanceToPlayer))
            {
                return hit.transform == enemy.player;
            }

            return true;
        }

        private bool HasReachedDestination(EnemyFSM enemy)
        {
            return !enemy.Agent.pathPending && enemy.Agent.remainingDistance < 1f;
        }

        // 공개 메서드: 다른 상태에서 어그로 활성화 호출용
        public void ActivateAggro()
        {
            if (!isAggro)
            {
                isAggro = true;
                aggroStartTime = Time.time;
                lastAggroTime = Time.time;
                Debug.Log("어그로 상태 활성화!");
            }
            else
            {
                // 이미 어그로 상태라면 시간만 갱신
                lastAggroTime = Time.time;
            }
        }

        public void ResetAggroState()
        {
            isAggro = false;
            aggroStartTime = 0f;
            lastAggroTime = 0f;
        }

        public bool IsAggro => isAggro;

        public override void DrawGizmos(EnemyFSM enemy)
        {
            if (enemy == null) return;

            // 일반 최대 추적 거리
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(enemy.transform.position, maxChaseDistance);

            // 어그로 최대 추적 거리
            if (isAggro)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(enemy.transform.position, aggroChaseDistance);
            }

            // 공격 전환 거리
            Gizmos.color = new Color(1f, 0.5f, 0f); // 오렌지색
            Gizmos.DrawWireSphere(enemy.transform.position, attackRange);

            // 플레이어와의 연결선
            if (enemy.player != null)
            {
                Gizmos.color = isAggro ? Color.red : Color.green;
                Gizmos.DrawLine(enemy.transform.position, enemy.player.position);
            }

            // 마지막 목격 위치
            if (useLastKnownPosition && lastKnownPosition != Vector3.zero)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(lastKnownPosition, 2f);
                Gizmos.DrawLine(enemy.transform.position, lastKnownPosition);
            }
        }

        public void ResetRuntimeData()
        {
            lastSeenTime = 0f;
            lastKnownPosition = Vector3.zero;
            pathUpdateTimer = 0f;
            searchTimer = 0f;
            isSearching = false;
            ResetAggroState();
        }
    }
}