using UnityEngine;

public class SniperController : EnemyController
{
    [Header("스나이퍼 설정")]
    [SerializeField] private float detectionRange = 25f;
    [SerializeField] private float attackRange = 20f;
    [SerializeField] private float loseTargetRange = 40f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float bulletSpeed = 40f;
    [SerializeField] private float aimTime = 2f;

    public override float DetectionRange => detectionRange;
    public override float AttackRange => attackRange;
    public override float LoseTargetRange => loseTargetRange;
    public override float PatrolSpeed => patrolSpeed;
    public override float ChaseSpeed => chaseSpeed;
    public override float AttackCooldown => attackCooldown;
    public override float BulletSpeed => bulletSpeed;

    public float AimTime => aimTime;

    protected override void RegisterStates()
    {
        AddState(new PatrolState());
        AddState(new CautiousChaseState()); // 신중한 추적
        AddState(new SniperAttackState());  // 정밀 사격
        AddState(new RelocateState());      // 위치 변경

        ChangeState<PatrolState>();
    }

    protected override void OnTakeDamage(float damage)
    {
        // 스나이퍼는 데미지를 받으면 위치를 바꿈
        ChangeState<RelocateState>();
    }

    protected override Color GetTypeColor() => Color.green;
}