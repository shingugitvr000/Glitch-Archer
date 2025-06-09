using UnityEngine;

public class CautiousChaseState : ChaseState
{
    public override void Update()
    {
        base.Update();

        // 스나이퍼는 천천히 이동하면서 엄폐물 활용
        enemy.Agent.speed = enemy.ChaseSpeed * 0.8f;
    }
}