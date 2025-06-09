using UnityEngine;

public class BurstFireState : EnemyStateBase
{
    private int burstCount = 0;
    private float nextBurstTime;

    public override void Enter(EnemyController enemy)
    {
        base.Enter(enemy);
        enemy.Agent.speed = 0f;
        enemy.Agent.SetDestination(enemy.transform.position);
        burstCount = 0;
        nextBurstTime = Time.time;
    }

    public override void Update()
    {
        if (enemy.player == null)
        {
            enemy.ChangeState<PatrolState>();
            return;
        }

        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

        if (distance > enemy.AttackRange * 1.5f)
        {
            enemy.ChangeState<TacticalChaseState>();
            return;
        }

        LookAtPlayer();

        // 3발 점사
        if (burstCount < 3 && Time.time >= nextBurstTime)
        {
            Fire();
            burstCount++;
            nextBurstTime = Time.time + 0.2f;
        }
        else if (burstCount >= 3)
        {
            // 점사 완료, 쿨다운 후 다시
            if (Time.time - enemy.lastAttackTime >= enemy.AttackCooldown)
            {
                burstCount = 0;
                enemy.lastAttackTime = Time.time;
            }
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = (enemy.player.position - enemy.transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 6f);
        }
    }

    void Fire()
    {
        if (enemy.bulletPrefab == null || enemy.firePoint == null) return;

        Vector3 direction = (enemy.player.position - enemy.firePoint.position).normalized;
        GameObject bullet = GameObject.Instantiate(enemy.bulletPrefab, enemy.firePoint.position, Quaternion.LookRotation(direction));

        var rb = bullet.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = direction * enemy.BulletSpeed;

        var projectile = bullet.GetComponent<ProjectileMoveScript>();
        if (projectile != null)
        {
            projectile.SetShooter(enemy.transform);
            projectile.isPlayerBullet = false;
            projectile.damage = enemy.damage;
        }

        GameObject.Destroy(bullet, 5f);

        if (enemy.Anim != null)
            enemy.Anim.SetTrigger("Fire");

        Debug.Log($"[{enemy.name}] 점사! ({burstCount + 1}/3)");
    }
}