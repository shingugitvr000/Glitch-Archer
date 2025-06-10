using UnityEngine;

public class RapidFireState : EnemyStateBase
{
    private int shotsRemaining = 3;
    private float nextShotTime;
    private bool isAttacking = false;

    public override void Enter(EnemyController enemy)
    {
        base.Enter(enemy);
        enemy.Agent.speed = 0f;
        enemy.Agent.SetDestination(enemy.transform.position);
        shotsRemaining = 3;
        nextShotTime = Time.time;
        isAttacking = true;

        Debug.Log($"[{enemy.name}] 연발 공격 모드 시작! 남은 탄수: {shotsRemaining}");
    }

    public override void Update()
    {
        if (enemy.player == null)
        {
            Debug.Log($"[{enemy.name}] 플레이어 없음 - 패트롤로 복귀");
            enemy.ChangeState<PatrolState>();
            return;
        }

        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

        // 너무 멀어지면 추적으로
        if (distance > enemy.AttackRange * 1.5f)
        {
            Debug.Log($"[{enemy.name}] 플레이어가 너무 멀어짐 (거리: {distance:F1}) - 추적 모드로 전환");
            enemy.ChangeState<AssaultChaseState>();
            return;
        }

        // 플레이어 바라보기
        LookAtPlayer();

        // 연발 사격
        if (shotsRemaining > 0 && Time.time >= nextShotTime)
        {
            Fire();
            shotsRemaining--;
            nextShotTime = Time.time + 0.3f; // 0.3초 간격으로 연발

            Debug.Log($"[{enemy.name}] 발사! 남은 탄수: {shotsRemaining}");
        }
        else if (shotsRemaining <= 0)
        {
            // 연발 완료, 다시 추적으로
            Debug.Log($"[{enemy.name}] 연발 완료 - 추적 모드로 전환");
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
        if (enemy.bulletPrefab == null)
        {
            Debug.LogError($"[{enemy.name}] bulletPrefab이 설정되지 않았습니다!");
            return;
        }

        if (enemy.firePoint == null)
        {
            Debug.LogError($"[{enemy.name}] firePoint가 설정되지 않았습니다!");
            return;
        }

        // 총알 발사 방향 계산 (약간의 예측 사격 추가)
        Vector3 targetPosition = enemy.player.position;

        // 플레이어가 움직이고 있다면 예측 사격
        var playerController = enemy.player.GetComponent<CharacterController>();
        if (playerController != null)
        {
            Vector3 playerVelocity = playerController.velocity;
            float bulletTravelTime = Vector3.Distance(enemy.firePoint.position, enemy.player.position) / enemy.BulletSpeed;
            targetPosition += playerVelocity * bulletTravelTime * 0.5f; // 50% 예측
        }

        Vector3 direction = (targetPosition - enemy.firePoint.position).normalized;

        // 총알 생성
        GameObject bullet = GameObject.Instantiate(enemy.bulletPrefab, enemy.firePoint.position, Quaternion.LookRotation(direction));

        // 총알에 물리 적용
        var rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * enemy.BulletSpeed;
        }
        else
        {
            Debug.LogWarning($"[{enemy.name}] 총알에 Rigidbody가 없습니다!");
        }

        // 발사체 설정
        var projectile = bullet.GetComponent<ProjectileMoveScript>();
        if (projectile != null)
        {
            projectile.SetShooter(enemy.transform);
            projectile.isPlayerBullet = false;
            projectile.damage = enemy.damage;
            Debug.Log($"[{enemy.name}] 발사체 설정 완료 - 데미지: {enemy.damage}");
        }
        else
        {
            Debug.LogWarning($"[{enemy.name}] 총알에 ProjectileMoveScript가 없습니다!");
        }

        // 총알 자동 제거
        GameObject.Destroy(bullet, 5f);

        // 애니메이션 트리거
        if (enemy.Anim != null)
        {
            enemy.Anim.SetTrigger("Fire");
        }

        // 마지막 공격 시간 업데이트
        enemy.lastAttackTime = Time.time;

        Debug.Log($"[{enemy.name}] 연발 사격! ({4 - shotsRemaining}/3) - 방향: {direction}");
    }

    public override void Exit()
    {
        base.Exit();
        isAttacking = false;
        Debug.Log($"[{enemy.name}] 연발 공격 모드 종료");
    }

    public override void DrawGizmos()
    {
        if (enemy == null || enemy.player == null) return;

        // 공격 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemy.transform.position, enemy.AttackRange);

        // 플레이어까지의 거리 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(enemy.transform.position, enemy.player.position);

        // 발사 지점에서 플레이어로의 조준선
        if (enemy.firePoint != null)
        {
            Gizmos.color = isAttacking ? Color.red : Color.yellow;
            Gizmos.DrawLine(enemy.firePoint.position, enemy.player.position);
        }
    }
}