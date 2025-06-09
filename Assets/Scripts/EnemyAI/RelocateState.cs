using UnityEngine;

public class RelocateState : EnemyStateBase
{
    private Vector3 newPosition;
    private bool hasDestination;

    public override void Enter(EnemyController enemy)
    {
        base.Enter(enemy);
        hasDestination = false;
        FindNewPosition();
    }

    public override void Update()
    {
        if (!hasDestination) return;

        // 새 위치로 이동
        if (Vector3.Distance(enemy.transform.position, newPosition) < 2f)
        {
            // 이동 완료, 다시 추적으로
            enemy.ChangeState<CautiousChaseState>();
        }
    }

    void FindNewPosition()
    {
        // 현재 위치에서 랜덤한 방향으로 이동
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            0,
            Random.Range(-1f, 1f)
        ).normalized;

        newPosition = enemy.transform.position + randomDirection * Random.Range(8f, 15f);
        enemy.Agent.SetDestination(newPosition);
        enemy.Agent.speed = enemy.ChaseSpeed;
        hasDestination = true;

        Debug.Log($"[{enemy.name}] 새 위치로 이동 중...");
    }

    public override void DrawGizmos()
    {
        if (hasDestination && newPosition != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(newPosition, 1f);
            Gizmos.DrawLine(enemy.transform.position, newPosition);
        }
    }
}