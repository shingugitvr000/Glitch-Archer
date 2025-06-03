using UnityEngine;

namespace EnemyAI
{
    [CreateAssetMenu(fileName = "New Patrol State", menuName = "Enemy AI/States/Patrol State")]
    public class PatrolState : EnemyState
    {
        [Header("패트롤 설정")]
        public float patrolSpeed = 2f;
        public float waitTime = 2f;                 // 각 포인트에서 대기 시간
        public float detectionRange = 15f;          // 플레이어 감지 거리
        public bool randomPatrol = false;           // 랜덤 순서로 패트롤

        [Header("감지 설정")]
        public float viewAngle = 90f;               // 시야각
        public LayerMask obstacleLayer = -1;        // 장애물 레이어

        [Header("즉시 공격 설정")]
        public float immediateAttackRange = 8f;     // 즉시 공격 범위 (패트롤 중에도 바로 공격)
        public bool enableImmediateAttack = true;   // 즉시 공격 활성화

        // 런타임 데이터 (ScriptableObject에 저장하면 안 되는 데이터)
        private int currentWaypointIndex = 0;
        private float waitTimer = 0f;
        private bool isWaiting = false;

        public override void EnterState(EnemyFSM enemy)
        {
            base.EnterState(enemy);

            // 패트롤 시작
            StartPatrol(enemy);
        }

        public override void UpdateState(EnemyFSM enemy)
        {
            // 플레이어 감지 확인
            PlayerDetectionResult detectionResult = CheckForPlayer(enemy);

            if (detectionResult.playerDetected)
            {
                if (detectionResult.shouldAttackImmediately)
                {
                    // 즉시 공격 상태로 전환
                    Debug.Log($"[{enemy.name}] 패트롤 중 플레이어 발견! 즉시 공격 시작 (거리: {detectionResult.distanceToPlayer:F1}m)");
                    enemy.StateManager.ChangeState(enemy.attackState);
                }
                else
                {
                    // 추적 상태로 전환
                    Debug.Log($"[{enemy.name}] 패트롤 중 플레이어 발견! 추적 시작 (거리: {detectionResult.distanceToPlayer:F1}m)");
                    enemy.StateManager.ChangeState(enemy.chaseState);
                }
                return;
            }

            // 패트롤 로직
            HandlePatrol(enemy);
        }

        public override void SetAnimationParameters(EnemyFSM enemy)
        {
            var animController = enemy.AnimationController;
            if (animController == null) return;

            // 패트롤 중: 이동 가능, 전투 아님
            animController.SetBool("InCombat", false);

            // Speed는 EnemyAnimationController에서 자동으로 처리됨
            // 블렌드 트리가 Speed 값에 따라 Idle/Walk/Sprint 자동 전환
        }

        public override float GetMoveSpeed()
        {
            return patrolSpeed;
        }

        private void StartPatrol(EnemyFSM enemy)
        {
            if (enemy.patrolPoints == null || enemy.patrolPoints.Length == 0)
            {
                Debug.LogWarning($"[{enemy.name}] 패트롤 포인트가 없습니다!");
                return;
            }

            // 가장 가까운 패트롤 포인트 찾기
            FindClosestWaypoint(enemy);

            // 목적지 설정
            SetDestination(enemy);

            // 초기화
            isWaiting = false;
            waitTimer = 0f;
        }

        private void HandlePatrol(EnemyFSM enemy)
        {
            if (enemy.patrolPoints == null || enemy.patrolPoints.Length == 0) return;

            // 대기 중인 경우
            if (isWaiting)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitTime)
                {
                    // 다음 웨이포인트로 이동
                    MoveToNextWaypoint(enemy);
                    isWaiting = false;
                    waitTimer = 0f;
                }
            }
            // 이동 중인 경우
            else
            {
                // 목적지에 도착했는지 확인
                if (HasReachedDestination(enemy))
                {
                    isWaiting = true;
                }
            }
        }

        // 플레이어 감지 결과를 담는 구조체
        private struct PlayerDetectionResult
        {
            public bool playerDetected;
            public bool shouldAttackImmediately;
            public float distanceToPlayer;
        }

        private PlayerDetectionResult CheckForPlayer(EnemyFSM enemy)
        {
            PlayerDetectionResult result = new PlayerDetectionResult();
            result.playerDetected = false;
            result.shouldAttackImmediately = false;
            result.distanceToPlayer = 0f;

            if (enemy.player == null) return result;

            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
            result.distanceToPlayer = distanceToPlayer;

            // 거리 체크
            if (distanceToPlayer > detectionRange) return result;

            // 시야각 체크
            Vector3 directionToPlayer = (enemy.player.position - enemy.transform.position).normalized;
            float angle = Vector3.Angle(enemy.transform.forward, directionToPlayer);

            if (angle > viewAngle / 2) return result;

            // 장애물 체크
            if (!HasLineOfSight(enemy, enemy.player.position)) return result;

            // 플레이어 감지됨
            result.playerDetected = true;

            // 즉시 공격 범위 체크
            if (enableImmediateAttack && distanceToPlayer <= immediateAttackRange)
            {
                result.shouldAttackImmediately = true;
            }

            return result;
        }

        private bool HasLineOfSight(EnemyFSM enemy, Vector3 targetPosition)
        {
            Vector3 rayOrigin = enemy.transform.position + Vector3.up * 1.5f; // 눈 높이
            Vector3 directionToTarget = (targetPosition - rayOrigin).normalized;
            float distanceToTarget = Vector3.Distance(rayOrigin, targetPosition);

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, directionToTarget, out hit, distanceToTarget, obstacleLayer))
            {
                return hit.transform == enemy.player;
            }

            return true; // 장애물 없음
        }

        private void FindClosestWaypoint(EnemyFSM enemy)
        {
            float closestDistance = float.MaxValue;

            for (int i = 0; i < enemy.patrolPoints.Length; i++)
            {
                if (enemy.patrolPoints[i] == null) continue;

                float distance = Vector3.Distance(enemy.transform.position, enemy.patrolPoints[i].position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    currentWaypointIndex = i;
                }
            }
        }

        private void MoveToNextWaypoint(EnemyFSM enemy)
        {
            if (randomPatrol)
            {
                // 랜덤 패트롤
                int randomIndex = Random.Range(0, enemy.patrolPoints.Length);
                currentWaypointIndex = randomIndex;
            }
            else
            {
                // 순차 패트롤
                currentWaypointIndex = (currentWaypointIndex + 1) % enemy.patrolPoints.Length;
            }

            SetDestination(enemy);
        }

        private void SetDestination(EnemyFSM enemy)
        {
            if (enemy.patrolPoints[currentWaypointIndex] != null)
            {
                enemy.Agent.SetDestination(enemy.patrolPoints[currentWaypointIndex].position);
                enemy.Agent.speed = patrolSpeed;
            }
        }

        private bool HasReachedDestination(EnemyFSM enemy)
        {
            if (!enemy.Agent.pathPending && enemy.Agent.remainingDistance < 0.5f)
            {
                return true;
            }
            return false;
        }

        public override void DrawGizmos(EnemyFSM enemy)
        {
            if (enemy == null) return;

            // 감지 범위
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(enemy.transform.position, detectionRange);

            // 즉시 공격 범위 (새로 추가)
            if (enableImmediateAttack)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(enemy.transform.position, immediateAttackRange);
            }

            // 시야각
            Gizmos.color = Color.green;
            Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * enemy.transform.forward * detectionRange;
            Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * enemy.transform.forward * detectionRange;

            Gizmos.DrawLine(enemy.transform.position, enemy.transform.position + leftBoundary);
            Gizmos.DrawLine(enemy.transform.position, enemy.transform.position + rightBoundary);

            // 패트롤 포인트들
            if (enemy.patrolPoints != null)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < enemy.patrolPoints.Length; i++)
                {
                    if (enemy.patrolPoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(enemy.patrolPoints[i].position, 1f);

                        // 현재 목표 포인트 강조
                        if (i == currentWaypointIndex)
                        {
                            Gizmos.color = Color.cyan;
                            Gizmos.DrawWireSphere(enemy.patrolPoints[i].position, 1.5f);
                            Gizmos.color = Color.blue;
                        }

                        // 패트롤 경로 표시
                        if (i < enemy.patrolPoints.Length - 1 && enemy.patrolPoints[i + 1] != null)
                        {
                            Gizmos.DrawLine(enemy.patrolPoints[i].position, enemy.patrolPoints[i + 1].position);
                        }
                        else if (i == enemy.patrolPoints.Length - 1 && enemy.patrolPoints[0] != null && !randomPatrol)
                        {
                            Gizmos.DrawLine(enemy.patrolPoints[i].position, enemy.patrolPoints[0].position);
                        }
                    }
                }
            }
        }

        // 에디터에서 상태 초기화 (런타임 데이터 리셋)
        public void ResetRuntimeData()
        {
            currentWaypointIndex = 0;
            waitTimer = 0f;
            isWaiting = false;
        }
    }
}