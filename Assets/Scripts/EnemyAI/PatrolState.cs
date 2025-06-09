using UnityEngine;

public class PatrolState : EnemyStateBase
{
    public override void Update()
    {
        // 플레이어 감지
        float distance = enemy.player != null ? Vector3.Distance(enemy.transform.position, enemy.player.position) : float.MaxValue;

        if (distance <= enemy.DetectionRange && CanSeePlayer())
        {
            enemy.ChangeState<ChaseState>();
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
            return hit.transform == enemy.player;
        }
        return true;
    }

    void DoPatrol()
    {
        if (enemy.patrolPoints == null || enemy.patrolPoints.Length == 0) return;

        enemy.Agent.speed = enemy.PatrolSpeed;

        if (enemy.isWaiting)
        {
            enemy.waitTimer += Time.deltaTime;
            if (enemy.waitTimer >= 2f)
            {
                enemy.isWaiting = false;
                enemy.currentPatrolIndex = (enemy.currentPatrolIndex + 1) % enemy.patrolPoints.Length;
                enemy.Agent.SetDestination(enemy.patrolPoints[enemy.currentPatrolIndex].position);
            }
        }
        else
        {
            if (!enemy.Agent.pathPending && enemy.Agent.remainingDistance < 1f)
            {
                enemy.isWaiting = true;
                enemy.waitTimer = 0f;
            }
        }
    }

    public override void Enter(EnemyController enemy)
    {
        base.Enter(enemy);

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
            enemy.Agent.SetDestination(enemy.patrolPoints[enemy.currentPatrolIndex].position);
        }

        enemy.isWaiting = false;
        enemy.Agent.speed = enemy.PatrolSpeed;
    }
}