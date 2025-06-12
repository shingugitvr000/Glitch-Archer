using UnityEngine;

public class ChaseState : EnemyStateBase
{
    public override void Enter(EnemyController enemy)
    {
        base.Enter(enemy);
        enemy.Agent.speed = enemy.ChaseSpeed;
        enemy.lastSeenTime = Time.time;
    }

    public override void Update()
    {
        if (enemy.player == null)
        {
            enemy.ChangeState<PatrolState>();
            return;
        }

        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

        // 너무 멀어지면 포기
        if (distance > enemy.LoseTargetRange)
        {
            // 마지막 목격 위치 근처를 잠시 수색 후 포기
            if (Time.time - enemy.lastSeenTime > 5f) // 5초간 수색
            {
                enemy.ChangeState<PatrolState>();
                return;
            }

            // 마지막 목격 위치로 이동하여 수색
            Vector3 lastSeenPosition = enemy.player.position;
            enemy.Agent.SetDestination(lastSeenPosition);
            Debug.Log($"[{enemy.name}] 마지막 목격 위치 수색 중...");
            return;
        }

        // 공격 범위에 들어오면 공격
        if (distance <= enemy.AttackRange)
        {
            // 타입별로 다른 공격 상태로 전환
            if (enemy is AssaultController)
                enemy.ChangeState<RapidFireState>();
            else if (enemy is MidRangeController)
                enemy.ChangeState<BurstFireState>();
            else if (enemy is SniperController)
                enemy.ChangeState<SniperAttackState>();
            return;
        }

        // 플레이어 추적
        enemy.lastSeenTime = Time.time; // 플레이어를 보고 있는 시간 업데이트
        enemy.Agent.SetDestination(enemy.player.position);
        LookAtPlayer();
    }

    protected virtual void LookAtPlayer()
    {
        Vector3 direction = (enemy.player.position - enemy.transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
}