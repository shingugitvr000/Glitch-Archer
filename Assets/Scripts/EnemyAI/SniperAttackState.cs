using UnityEngine;

public class SniperAttackState : EnemyStateBase
{
    private float aimStartTime;
    private bool isAiming;
    private bool hasShot;

    public override void Enter(EnemyController enemy)
    {
        base.Enter(enemy);
        enemy.Agent.speed = 0f;
        enemy.Agent.SetDestination(enemy.transform.position);
        isAiming = false;
        hasShot = false;
        Debug.Log($"[{enemy.name}] 스나이퍼 공격 준비");
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

        // 공격 범위를 벗어나면 다시 추적
        if (distance > enemy.AttackRange * 1.3f)
        {
            Debug.Log($"[{enemy.name}] 타겟이 너무 멀어짐 - 추적 재개");
            enemy.ChangeState<CautiousChaseState>();
            return;
        }

        if (!isAiming && !hasShot)
        {
            // 조준 시작
            isAiming = true;
            aimStartTime = Time.time;
            Debug.Log($"[{enemy.name}] 스나이퍼 조준 시작... ({sniper.AimTime}초)");
        }
        else if (isAiming && !hasShot)
        {
            // 조준 중
            LookAtPlayer();

            if (Time.time - aimStartTime >= sniper.AimTime)
            {
                // 정밀 사격
                PrecisionShot();
                hasShot = true;
                enemy.lastAttackTime = Time.time;
                Debug.Log($"[{enemy.name}] 정밀 사격 완료 - 위치 변경 예정");
            }
        }
        else if (hasShot)
        {
            // 사격 후 잠깐 대기 후 위치 변경
            if (Time.time - enemy.lastAttackTime >= 1f) // 1초 대기
            {
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
            // 조준 중에는 매우 정밀하게 회전
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 4f);
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
