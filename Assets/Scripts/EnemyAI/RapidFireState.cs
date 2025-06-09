using UnityEngine;

public class RapidFireState : EnemyStateBase
{
    private int shotsRemaining = 3;
    private float nextShotTime;

    public override void Enter(EnemyController enemy)
    {
        base.Enter(enemy);
        enemy.Agent.speed = 0f;
        enemy.Agent.SetDestination(enemy.transform.position);
        shotsRemaining = 3;
        nextShotTime = Time.time;
    }

    public override void Update()
    {
        if (enemy.player == null)
        {
            enemy.ChangeState<PatrolState>();
            return;
        }

        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

        // 너무 멀어지면 추적으로
        if (distance > enemy.AttackRange * 1.5f)
        {
            enemy.ChangeState<AssaultChaseState>();
            return;
        }

        LookAtPlayer();

        // 연발 사격
        if (shotsRemaining > 0 && Time.time >= nextShotTime)
        {
            Fire();
            shotsRemaining--;
            nextShotTime = Time.time + 0.3f; // 0.3초 간격으로 연발
        }
        else if (shotsRemaining <= 0)
        {
            // 연발 완료, 다시 추적으로
            enemy.ChangeState<AssaultChaseState>();
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = (enemy.player.position - enemy.transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 8f);
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

        Debug.Log($"[{enemy.name}] 연발 사격! ({4 - shotsRemaining}/3)");
    }
}
