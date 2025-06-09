using UnityEngine;

public class AssaultController : EnemyController
{
    [Header("돌격형 설정")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float loseTargetRange = 20f;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed = 7f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float bulletSpeed = 25f;

    public override float DetectionRange => detectionRange;
    public override float AttackRange => attackRange;
    public override float LoseTargetRange => loseTargetRange;
    public override float PatrolSpeed => patrolSpeed;
    public override float ChaseSpeed => chaseSpeed;
    public override float AttackCooldown => attackCooldown;
    public override float BulletSpeed => bulletSpeed;

    protected override void RegisterStates()
    {
        AddState(new PatrolState());
        AddState(new AssaultChaseState());  // 공격적인 추적
        AddState(new RapidFireState());     // 연발 공격

        ChangeState<PatrolState>();
    }

    protected override void OnTakeDamage(float damage)
    {
        // 돌격형은 데미지를 받으면 더 공격적이 됨
        if (CurrentState is PatrolState || CurrentState is AssaultChaseState)
        {
            ChangeState<RapidFireState>();
        }
    }

    protected override Color GetTypeColor() => Color.red;
}