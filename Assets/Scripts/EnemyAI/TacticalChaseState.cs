using UnityEngine;

public class TacticalChaseState : ChaseState
{
    public override void Update()
    {
        base.Update();

        // 중거리형은 적절한 거리 유지
        if (enemy.player != null)
        {
            float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);
            if (distance < enemy.AttackRange * 0.7f)
            {
                // 너무 가까우면 후퇴
                Vector3 retreatDirection = (enemy.transform.position - enemy.player.position).normalized;
                Vector3 retreatPosition = enemy.transform.position + retreatDirection * 3f;
                enemy.Agent.SetDestination(retreatPosition);
            }
        }
    }
}