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
        AddState(new ChaseState());         // 기본 추적 상태 추가
        AddState(new AssaultChaseState());  // 강화된 추적
        AddState(new RapidFireState());     // 연발 공격

        ChangeState<PatrolState>();
    }

    protected override void OnTakeDamage(float damage)
    {
        // 돌격형은 데미지를 받으면 더 공격적이 됨
        if (CurrentState is PatrolState || CurrentState is ChaseState || CurrentState is AssaultChaseState)
        {
            ChangeState<RapidFireState>();
        }
    }

    protected override void HandleLongRangeResponse()
    {
        // 돌격형은 적극적으로 추적
        if (CurrentState is PatrolState)
        {
            ChangeState<AssaultChaseState>();
            Debug.Log($"[{name}] 돌격형 - 적극적 추적 시작");
        }
    }


    protected override Color GetTypeColor() => Color.red;
}