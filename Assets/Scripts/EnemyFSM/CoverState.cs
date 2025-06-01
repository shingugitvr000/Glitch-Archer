using UnityEngine;

namespace EnemyAI
{
    [CreateAssetMenu(fileName = "New Cover State", menuName = "Enemy AI/States/Cover State")]
    public class CoverState : EnemyState
    {
        [Header("엄폐 설정")]
        public float coverTime = 5f;               // 엄폐 시간
        public float peekInterval = 2f;            // 엿보기 간격
        public float peekDuration = 1f;            // 엿보기 시간
        public float coverSearchRange = 15f;       // 엄폐지 찾기 범위

        [Header("엄폐 조건")]
        public float lowHealthThreshold = 30f;     // 엄폐하는 체력 임계값
        public float reloadTime = 3f;              // 재장전 시간
        public bool coverWhenReloading = true;     // 재장전 시 엄폐

        // 런타임 데이터
        private float coverStartTime;
        private float lastPeekTime;
        private bool isPeeking = false;
        private Transform currentCoverPoint;
        private Vector3 coverPosition;

        public override void EnterState(EnemyFSM enemy)
        {
            base.EnterState(enemy);

            StartCover(enemy);
        }

        public override void UpdateState(EnemyFSM enemy)
        {
            if (enemy.player == null)
            {
                enemy.StateManager.ChangeState(enemy.patrolState);
                return;
            }

            float timeSinceCoverStart = Time.time - coverStartTime;

            // 엄폐 시간이 지나면 다시 전투로
            if (timeSinceCoverStart >= coverTime)
            {
                ExitCover(enemy);
                return;
            }

            // 엄폐 행동 처리
            HandleCoverBehavior(enemy, timeSinceCoverStart);
        }


        public override void SetAnimationParameters(EnemyFSM enemy)
        {
            var animController = enemy.AnimationController;
            if (animController == null) return;

            // 엄폐 중: 정지 상태, 전투 중
            animController.SetBool("InCombat", true);

            // Speed는 0이 되어 Combat_Idle 애니메이션이 재생됨
            // 엄폐 관련 특별한 애니메이션이 있다면 추가 가능
        }

        public override float GetMoveSpeed()
        {
            return 1.5f; // 엄폐지로 이동할 때만 사용
        }

        private void StartCover(EnemyFSM enemy)
        {
            coverStartTime = Time.time;
            lastPeekTime = 0f;
            isPeeking = false;

            // 엄폐지 찾기
            FindCoverPoint(enemy);

            Debug.Log($"[{enemy.name}] 엄폐 시작!");
        }

        private void FindCoverPoint(EnemyFSM enemy)
        {
            // 설정된 엄폐 포인트들 중에서 찾기
            if (enemy.coverPoints != null && enemy.coverPoints.Length > 0)
            {
                Transform bestCover = null;
                float bestScore = float.MinValue;

                foreach (Transform coverPoint in enemy.coverPoints)
                {
                    if (coverPoint == null) continue;

                    float score = EvaluateCoverPoint(enemy, coverPoint);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestCover = coverPoint;
                    }
                }

                if (bestCover != null)
                {
                    currentCoverPoint = bestCover;
                    coverPosition = bestCover.position;
                    enemy.Agent.SetDestination(coverPosition);
                    return;
                }
            }

            // 엄폐 포인트가 없으면 간단한 후퇴
            Vector3 retreatDirection = (enemy.transform.position - enemy.player.position).normalized;
            coverPosition = enemy.transform.position + retreatDirection * 5f;
            enemy.Agent.SetDestination(coverPosition);
        }

        private float EvaluateCoverPoint(EnemyFSM enemy, Transform coverPoint)
        {
            float score = 0f;

            // 거리 점수 (너무 멀지 않고 너무 가깝지 않게)
            float distanceToEnemy = Vector3.Distance(enemy.transform.position, coverPoint.position);
            float distanceScore = Mathf.Clamp01(1f - (distanceToEnemy / coverSearchRange));
            score += distanceScore * 30f;

            // 플레이어로부터의 보호 점수
            Vector3 directionToPlayer = (enemy.player.position - coverPoint.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(coverPoint.position, directionToPlayer, out hit))
            {
                if (hit.transform != enemy.player)
                {
                    score += 50f; // 플레이어에게서 보호됨
                }
            }

            // 이미 다른 적이 사용 중인지 확인 (추후 구현)

            return score;
        }

        private void HandleCoverBehavior(EnemyFSM enemy, float timeSinceCoverStart)
        {
            // 엄폐지에 도착했는지 확인
            bool hasReachedCover = Vector3.Distance(enemy.transform.position, coverPosition) < 1f;

            if (hasReachedCover)
            {
                // 엄폐지에서 정지 - Combat_Idle 애니메이션 재생
                enemy.Agent.SetDestination(enemy.transform.position);

                // 엄폐 중 - 가끔 엿보기
                HandlePeeking(enemy, timeSinceCoverStart);
            }
            else
            {
                // 아직 엄폐지로 이동 중
                enemy.Agent.SetDestination(coverPosition);
            }
        }

        private void HandlePeeking(EnemyFSM enemy, float timeSinceCoverStart)
        {
            float timeSinceLastPeek = Time.time - lastPeekTime;

            if (!isPeeking && timeSinceLastPeek >= peekInterval)
            {
                // 엿보기 시작
                StartPeeking(enemy);
            }
            else if (isPeeking && timeSinceLastPeek >= peekDuration)
            {
                // 엿보기 종료
                StopPeeking(enemy);
            }
        }

        private void StartPeeking(EnemyFSM enemy)
        {
            isPeeking = true;
            lastPeekTime = Time.time;

            // 엿보기 시 Fire 트리거 발동 (선택사항)
            enemy.AnimationController?.SetTrigger("Fire");

            // 플레이어 방향으로 살짝 회전
            Vector3 directionToPlayer = (enemy.player.position - enemy.transform.position).normalized;
            directionToPlayer.y = 0;

            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, 0.5f);
            }
        }

        private void StopPeeking(EnemyFSM enemy)
        {
            isPeeking = false;

            // 엄폐 애니메이션 트리거
            enemy.AnimationController?.SetTrigger("TakeCover");
        }

        private void ExitCover(EnemyFSM enemy)
        {
            // 플레이어와의 거리에 따라 다음 상태 결정
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);

            if (distanceToPlayer <= enemy.attackState.attackRange)
            {
                enemy.StateManager.ChangeState(enemy.attackState);
            }
            else
            {
                enemy.StateManager.ChangeState(enemy.chaseState);
            }
        }

        public override void DrawGizmos(EnemyFSM enemy)
        {
            if (enemy == null) return;

            // 엄폐지 찾기 범위
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(enemy.transform.position, coverSearchRange);

            // 현재 엄폐 위치
            if (coverPosition != Vector3.zero)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(coverPosition, 1f);
                Gizmos.DrawLine(enemy.transform.position, coverPosition);
            }

            // 엄폐 포인트들
            if (enemy.coverPoints != null)
            {
                foreach (Transform coverPoint in enemy.coverPoints)
                {
                    if (coverPoint != null)
                    {
                        Gizmos.color = coverPoint == currentCoverPoint ? Color.green : Color.gray;
                        Gizmos.DrawWireCube(coverPoint.position, Vector3.one * 2f);
                    }
                }
            }
        }

        public void ResetRuntimeData()
        {
            coverStartTime = 0f;
            lastPeekTime = 0f;
            isPeeking = false;
            currentCoverPoint = null;
            coverPosition = Vector3.zero;
        }
    }
}