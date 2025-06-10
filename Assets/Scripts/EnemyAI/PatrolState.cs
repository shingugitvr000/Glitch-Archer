using UnityEngine;

public class PatrolState : EnemyStateBase
{
    public override void Update()
    {
        // 플레이어 감지
        float distance = enemy.player != null ? Vector3.Distance(enemy.transform.position, enemy.player.position) : float.MaxValue;

        if (distance <= enemy.DetectionRange && CanSeePlayer())
        {
            // 적 타입별로 적절한 추적 상태로 전환
            if (enemy is AssaultController)
                enemy.ChangeState<AssaultChaseState>();
            else if (enemy is MidRangeController)
                enemy.ChangeState<TacticalChaseState>();
            else if (enemy is SniperController)
                enemy.ChangeState<CautiousChaseState>();
            else
                enemy.ChangeState<ChaseState>(); // 기본 추적
            return;
        }

        // 패트롤 로직
        DoPatrol();
    }

    bool CanSeePlayer()
    {
        if (enemy.player == null) return false;

        Vector3 rayOrigin = enemy.transform.position + Vector3.up * 1.5f;
        Vector3 direction = (enemy.player.position - rayOrigin).normalized;
        float distance = Vector3.Distance(rayOrigin, enemy.player.position);

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, direction, out hit, distance))
        {
            // 플레이어를 직접 보고 있는지 확인
            if (hit.transform == enemy.player)
            {
                Debug.Log($"[{enemy.name}] 플레이어 발견!");
                return true;
            }
            else
            {
                Debug.Log($"[{enemy.name}] 시야 차단됨: {hit.transform.name}");
                return false;
            }
        }
        return true;
    }

    void DoPatrol()
    {
        if (enemy.patrolPoints == null || enemy.patrolPoints.Length == 0)
        {
            // 패트롤 포인트가 없으면 제자리에서 대기
            enemy.Agent.SetDestination(enemy.transform.position);
            return;
        }

        enemy.Agent.speed = enemy.PatrolSpeed;

        if (enemy.isWaiting)
        {
            enemy.waitTimer += Time.deltaTime;
            if (enemy.waitTimer >= 2f)
            {
                enemy.isWaiting = false;
                enemy.currentPatrolIndex = (enemy.currentPatrolIndex + 1) % enemy.patrolPoints.Length;

                // 다음 패트롤 포인트가 유효한지 확인
                if (enemy.patrolPoints[enemy.currentPatrolIndex] != null)
                {
                    enemy.Agent.SetDestination(enemy.patrolPoints[enemy.currentPatrolIndex].position);
                    Debug.Log($"[{enemy.name}] 다음 패트롤 포인트로 이동: {enemy.currentPatrolIndex}");
                }
            }
        }
        else
        {
            if (!enemy.Agent.pathPending && enemy.Agent.remainingDistance < 1f)
            {
                enemy.isWaiting = true;
                enemy.waitTimer = 0f;
                Debug.Log($"[{enemy.name}] 패트롤 포인트 도착, 대기 중...");
            }
        }
    }

    public override void Enter(EnemyController enemy)
    {
        base.Enter(enemy);

        // 플레이어 참조 확인 및 설정
        if (enemy.player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                enemy.player = playerObj.transform;
                Debug.Log($"[{enemy.name}] 플레이어 참조 설정됨");
            }
            else
            {
                Debug.LogWarning($"[{enemy.name}] 플레이어를 찾을 수 없습니다!");
            }
        }

        // 가장 가까운 패트롤 포인트 찾기
        if (enemy.patrolPoints != null && enemy.patrolPoints.Length > 0)
        {
            float closestDistance = float.MaxValue;
            for (int i = 0; i < enemy.patrolPoints.Length; i++)
            {
                if (enemy.patrolPoints[i] == null) continue;
                float distance = Vector3.Distance(enemy.transform.position, enemy.patrolPoints[i].position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    enemy.currentPatrolIndex = i;
                }
            }

            if (enemy.patrolPoints[enemy.currentPatrolIndex] != null)
            {
                enemy.Agent.SetDestination(enemy.patrolPoints[enemy.currentPatrolIndex].position);
                Debug.Log($"[{enemy.name}] 첫 패트롤 포인트 설정: {enemy.currentPatrolIndex}");
            }
        }
        else
        {
            Debug.LogWarning($"[{enemy.name}] 패트롤 포인트가 설정되지 않았습니다!");
        }

        enemy.isWaiting = false;
        enemy.Agent.speed = enemy.PatrolSpeed;
    }

    public override void DrawGizmos()
    {
        if (enemy == null) return;

        // 시야 레이 그리기
        if (enemy.player != null)
        {
            Vector3 rayOrigin = enemy.transform.position + Vector3.up * 1.5f;
            Vector3 direction = (enemy.player.position - rayOrigin).normalized;
            float distance = Vector3.Distance(rayOrigin, enemy.player.position);

            Gizmos.color = CanSeePlayer() ? Color.red : Color.yellow;
            Gizmos.DrawLine(rayOrigin, rayOrigin + direction * distance);
        }

        // 현재 패트롤 경로 그리기
        if (enemy.patrolPoints != null && enemy.patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < enemy.patrolPoints.Length; i++)
            {
                if (enemy.patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(enemy.patrolPoints[i].position, 0.5f);

                    // 현재 목표 지점 강조
                    if (i == enemy.currentPatrolIndex)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireSphere(enemy.patrolPoints[i].position, 1f);
                        Gizmos.DrawLine(enemy.transform.position, enemy.patrolPoints[i].position);
                        Gizmos.color = Color.blue;
                    }
                }
            }
        }
    }
}