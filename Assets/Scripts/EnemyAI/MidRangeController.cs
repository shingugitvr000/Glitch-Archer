using UnityEngine;

public class MidRangeController : EnemyController
{
    [Header("중거리형 설정")]
    [SerializeField] private float detectionRange = 18f;
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private float loseTargetRange = 30f;
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float bulletSpeed = 20f;

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
        AddState(new TacticalChaseState()); // 전술적 추적
        AddState(new BurstFireState());     // 점사 공격
        AddState(new CoverState());         // 엄폐 사용

        ChangeState<PatrolState>();
    }

    protected override void OnTakeDamage(float damage)
    {
        // 중거리형은 체력이 낮고 이미 전투 중일 때만 엄폐 사용
        if (currentHealth / maxHealth < 0.5f && !(CurrentState is PatrolState))
        {
            ChangeState<CoverState>();
        }
    }

    protected override void HandleLongRangeResponse()
    {
        // 중거리형은 체력이 낮으면 엄폐, 높으면 추적
        if (currentHealth / maxHealth < 0.7f)
        {
            ChangeState<CoverState>();
            Debug.Log($"[{name}] 중거리형 - 엄폐 위치로 이동");
        }
        else
        {
            ChangeState<TacticalChaseState>();
            Debug.Log($"[{name}] 중거리형 - 전술적 추적 시작");
        }
    }

    protected override Color GetTypeColor() => Color.blue;
}