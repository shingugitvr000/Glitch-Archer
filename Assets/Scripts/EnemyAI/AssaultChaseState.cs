using UnityEngine;

public class AssaultChaseState : ChaseState
{
    public override void Update()
    {
        base.Update();

        // 돌격형은 더 빠르게 접근
        if (enemy.player != null)
        {
            float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);
            if (distance < enemy.AttackRange * 2f)
            {
                enemy.Agent.speed = enemy.ChaseSpeed * 1.2f; // 20% 더 빠르게
            }
        }
    }
}