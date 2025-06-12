using UnityEngine;

public class CautiousChaseState : ChaseState
{
    private float lastSeenPlayerTime;

    public override void Enter(EnemyController enemy)
    {
        base.Enter(enemy);
        lastSeenPlayerTime = Time.time;
        Debug.Log($"[{enemy.name}] 스나이퍼 신중한 추적 시작");
    }

    public override void Update()
    {
        if (enemy.player == null)
        {
            enemy.ChangeState<PatrolState>();
            return;
        }

        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

        // 플레이어를 볼 수 있으면 시간 업데이트
        if (CanSeePlayer())
        {
            lastSeenPlayerTime = Time.time;
        }

        // 너무 오래 못 봤으면 포기 (10초)
        if (Time.time - lastSeenPlayerTime > 10f)
        {
            Debug.Log($"[{enemy.name}] 스나이퍼 - 플레이어를 너무 오래 못 봄, 포기");
            enemy.ChangeState<PatrolState>();
            return;
        }

        // 거리로만 포기하지 않고, 시야와 함께 판단
        if (distance > enemy.LoseTargetRange && !CanSeePlayer())
        {
            Debug.Log($"[{enemy.name}] 스나이퍼 - 너무 멀고 시야에 없음, 포기");
            enemy.ChangeState<PatrolState>();
            return;
        }

        // 공격 범위에 들어오면 공격
        if (distance <= enemy.AttackRange && CanSeePlayer())
        {
            Debug.Log($"[{enemy.name}] 스나이퍼 - 공격 범위 진입, 사격 준비");
            enemy.ChangeState<SniperAttackState>();
            return;
        }

        // 스나이퍼는 신중하게 천천히 이동하면서 적절한 거리 유지
        float optimalDistance = enemy.AttackRange * 0.9f; // 공격 범위의 90% 지점을 선호

        if (distance < optimalDistance * 0.7f)
        {
            // 너무 가까우면 후퇴하면서 이동
            Vector3 retreatDirection = (enemy.transform.position - enemy.player.position).normalized;
            Vector3 retreatPosition = enemy.transform.position + retreatDirection * 8f;
            enemy.Agent.SetDestination(retreatPosition);
            enemy.Agent.speed = enemy.ChaseSpeed * 0.8f; // 느리게 후퇴
            Debug.Log($"[{enemy.name}] 스나이퍼 후퇴 - 거리: {distance:F1}m");
        }
        else
        {
            // 적절한 거리면 천천히 추적
            enemy.Agent.SetDestination(enemy.player.position);
            enemy.Agent.speed = enemy.ChaseSpeed * 0.9f; // 신중하게
            Debug.Log($"[{enemy.name}] 스나이퍼 추적 - 거리: {distance:F1}m");
        }

        LookAtPlayer();
    }

    private bool CanSeePlayer()
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

    protected override void LookAtPlayer()
    {
        Vector3 direction = (enemy.player.position - enemy.transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            // 스나이퍼는 더 천천히 회전 (신중함 표현)
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 3f);
        }
    }
}