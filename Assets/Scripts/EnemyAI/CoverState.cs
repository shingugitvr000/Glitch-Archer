using UnityEngine;

public class CoverState : EnemyStateBase
{
    public override void Enter(EnemyController enemy)
    {
        base.Enter(enemy);
        enemy.coverStartTime = Time.time;

        // 플레이어 반대 방향으로 후퇴
        if (enemy.player != null)
        {
            Vector3 retreatDirection = (enemy.transform.position - enemy.player.position).normalized;
            enemy.coverPosition = enemy.transform.position + retreatDirection * 8f;
            enemy.Agent.SetDestination(enemy.coverPosition);
            enemy.Agent.speed = enemy.ChaseSpeed;
        }
    }

    public override void Update()
    {
        // 3초 후 복귀
        if (Time.time - enemy.coverStartTime > 3f)
        {
            float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);
            if (distance <= enemy.AttackRange)
                enemy.ChangeState<BurstFireState>();
            else
                enemy.ChangeState<TacticalChaseState>();
            return;
        }

        // 엄폐 위치로 이동
        if (Vector3.Distance(enemy.transform.position, enemy.coverPosition) > 2f)
        {
            enemy.Agent.SetDestination(enemy.coverPosition);
        }
        else
        {
            // 엄폐 완료, 정지하고 플레이어 바라보기
            enemy.Agent.SetDestination(enemy.transform.position);
            LookAtPlayer();
        }
    }

    void LookAtPlayer()
    {
        if (enemy.player == null) return;

        Vector3 direction = (enemy.player.position - enemy.transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 2f);
        }
    }

    public override void DrawGizmos()
    {
        if (enemy.coverPosition != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(enemy.coverPosition, 1f);
            Gizmos.DrawLine(enemy.transform.position, enemy.coverPosition);
        }
    }
}