using UnityEngine;

public class SniperAttackState : EnemyStateBase
{
    private float aimStartTime;
    private bool isAiming;

    public override void Enter(EnemyController enemy)
    {
        base.Enter(enemy);
        enemy.Agent.speed = 0f;
        enemy.Agent.SetDestination(enemy.transform.position);
        isAiming = false;
    }

    public override void Update()
    {
        var sniper = enemy as SniperController;
        if (sniper == null || enemy.player == null)
        {
            enemy.ChangeState<PatrolState>();
            return;
        }

        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

        if (distance > enemy.AttackRange * 1.2f)
        {
            enemy.ChangeState<CautiousChaseState>();
            return;
        }

        if (!isAiming)
        {
            // 조준 시작
            isAiming = true;
            aimStartTime = Time.time;
            Debug.Log($"[{enemy.name}] 스나이퍼 조준 시작... ({sniper.AimTime}초)");
        }
        else
        {
            // 조준 중
            LookAtPlayer();

            if (Time.time - aimStartTime >= sniper.AimTime)
            {
                // 정밀 사격
                PrecisionShot();
                enemy.lastAttackTime = Time.time;

                // 위치 변경으로 전환
                enemy.ChangeState<RelocateState>();
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
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 3f);
        }
    }

    void PrecisionShot()
    {
        if (enemy.bulletPrefab == null || enemy.firePoint == null) return;

        // 정확한 조준
        Vector3 direction = (enemy.player.position - enemy.firePoint.position).normalized;
        GameObject bullet = GameObject.Instantiate(enemy.bulletPrefab, enemy.firePoint.position, Quaternion.LookRotation(direction));

        var rb = bullet.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = direction * enemy.BulletSpeed;

        var projectile = bullet.GetComponent<ProjectileMoveScript>();
        if (projectile != null)
        {
            projectile.SetShooter(enemy.transform);
            projectile.isPlayerBullet = false;
            projectile.damage = enemy.damage * 1.5f; // 스나이퍼는 50% 더 강한 데미지
        }

        GameObject.Destroy(bullet, 8f); // 더 긴 사거리

        if (enemy.Anim != null)
            enemy.Anim.SetTrigger("Fire");

        Debug.Log($"[{enemy.name}] 정밀 사격! (데미지: {enemy.damage * 1.5f})");
    }
}