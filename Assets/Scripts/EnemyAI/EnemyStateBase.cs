using UnityEngine;

public abstract class EnemyStateBase
{
    protected EnemyController enemy;

    public virtual void Enter(EnemyController enemy)
    {
        this.enemy = enemy;
        //Debug.Log($"[{enemy.name}] {GetType().Name} 진입");
    }

    public abstract void Update();

    public virtual void Exit()
    {
        //Debug.Log($"[{enemy.name}] {GetType().Name} 종료");
    }

    public virtual void DrawGizmos() { }
}