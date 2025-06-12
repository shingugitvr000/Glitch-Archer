using UnityEngine;

public class RelocateState : EnemyStateBase
{
    private Vector3 newPosition;
    private bool hasDestination;
    private float relocateStartTime;

    public override void Enter(EnemyController enemy)
    {
        base.Enter(enemy);
        hasDestination = false;
        relocateStartTime = Time.time;
        FindNewPosition();
        Debug.Log($"[{enemy.name}] 스나이퍼 위치 재배치 시작");
    }

    public override void Update()
    {
        if (!hasDestination) return;

        // 새 위치로 이동 완료
        if (Vector3.Distance(enemy.transform.position, newPosition) < 2f)
        {
            Debug.Log($"[{enemy.name}] 위치 재배치 완료 - 추적 재개");
            enemy.ChangeState<CautiousChaseState>();
            return;
        }

        // 너무 오래 걸리면 그냥 추적 재개
        if (Time.time - relocateStartTime > 10f)
        {
            Debug.Log($"[{enemy.name}] 재배치 시간 초과 - 추적 재개");
            enemy.ChangeState<CautiousChaseState>();
            return;
        }
    }

    void FindNewPosition()
    {
        // 플레이어를 중심으로 측면으로 이동 (더 전술적)
        if (enemy.player != null)
        {
            Vector3 toPlayer = (enemy.player.position - enemy.transform.position).normalized;
            Vector3 perpendicular = Vector3.Cross(toPlayer, Vector3.up); // 수직 방향

            // 좌우 중 랜덤 선택
            if (Random.value > 0.5f) perpendicular = -perpendicular;

            newPosition = enemy.transform.position + perpendicular * Random.Range(10f, 15f);
        }
        else
        {
            // 플레이어가 없으면 랜덤 이동
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;

            newPosition = enemy.transform.position + randomDirection * Random.Range(8f, 15f);
        }

        enemy.Agent.SetDestination(newPosition);
        enemy.Agent.speed = enemy.ChaseSpeed * 1.2f; // 빠르게 이동
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