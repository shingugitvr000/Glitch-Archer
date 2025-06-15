using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMoveScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float damage = 25f;
    public bool isPlayerBullet = false;
    public float speed = 100f;
    public float lifeTime = 5f;
    public float fireRate = 1f; // SpawnProjectilesScript 호환성을 위해 추가

    [Header("이펙트 (선택)")]
    public GameObject muzzlePrefab;
    public GameObject hitPrefab;
    public AudioClip shotSFX;
    public AudioClip hitSFX;
    public List<GameObject> trails;

    [Header("충돌 감지 설정")]
    [SerializeField] private LayerMask collisionLayers = -1; // 모든 레이어와 충돌
    [SerializeField] private float raycastDistance = 2f;     // 레이캐스트 거리

    private Transform shooter;
    private PlayerStats playerStats; // 플레이어 스탯 참조
    private Rigidbody bulletRigidbody;
    private Vector3 previousPosition; // 이전 프레임 위치

    // 관통 및 유도 관련 변수 추가
    private int remainingPierceCount = 0;  // 남은 관통 횟수
    private bool isGuided = false;         // 유도 모드 여부
    private Transform guidedTarget;        // 유도 대상
    private Vector3 originalDirection;     // 원래 진행 방향

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
        if (bulletRigidbody == null)
        {
            bulletRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        // ★ Rigidbody 설정 - 관통 방지 강화
        bulletRigidbody.useGravity = false;
        bulletRigidbody.drag = 0f;
        bulletRigidbody.angularDrag = 0f;
        bulletRigidbody.freezeRotation = true;
        bulletRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // 더 정확한 충돌 감지
        bulletRigidbody.interpolation = RigidbodyInterpolation.Interpolate; // 부드러운 움직임
    }

    private void Start()
    {
        // 원래 진행 방향 저장
        originalDirection = transform.forward;
        previousPosition = transform.position; // 초기 위치 저장

        // 화살 이동 (기존 ArrowProjectile 로직)
        bulletRigidbody.velocity = originalDirection * speed;

        // 생명주기 설정
        Destroy(gameObject, lifeTime);

        // 플레이어 총알인 경우 스탯 정보 가져오기
        if (isPlayerBullet && shooter != null)
        {
            playerStats = shooter.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // 플레이어 스탯에 따라 데미지 계산
                damage = playerStats.CalculateDamage(damage);

                // 관통 횟수 설정
                remainingPierceCount = playerStats.pierceCount;
            }
        }

        // 이펙트 재생
        PlayMuzzleEffect();
        PlayShotSound();

        Destroy(gameObject, 5f);
    }

    private void FixedUpdate()
    {
        // ★ 수동 레이캐스트로 추가 충돌 감지 (관통 방지)
        PerformManualCollisionCheck();

        // 유도 모드일 때 타겟 추적
        if (isGuided && guidedTarget != null)
        {
            // 타겟이 죽었거나 없어졌는지 확인
            var enemyController = guidedTarget.GetComponent<EnemyController>();
            if (enemyController == null || enemyController.currentHealth <= 0)
            {
                guidedTarget = null;
            }

            // 유효한 타겟이 있으면 추적
            if (guidedTarget != null)
            {
                Vector3 direction = (guidedTarget.position - transform.position).normalized;
                bulletRigidbody.velocity = direction * speed * 0.8f; // 유도 시 속도 약간 감소
                transform.rotation = Quaternion.LookRotation(direction);
                return;
            }
        }

        // 유도 대상이 없으면 새로운 대상 찾기
        if (isGuided && guidedTarget == null)
        {
            guidedTarget = FindClosestEnemy();

            // 새로운 대상도 없으면 원래 방향으로 복귀
            if (guidedTarget == null)
            {
                isGuided = false;
                bulletRigidbody.velocity = originalDirection * speed;
                transform.rotation = Quaternion.LookRotation(originalDirection);
            }
        }

        // 현재 위치 저장 (다음 프레임용)
        previousPosition = transform.position;
    }

    // ★ 수동 충돌 감지 - 빠른 속도에서도 정확한 충돌
    private void PerformManualCollisionCheck()
    {
        Vector3 currentPosition = transform.position;
        Vector3 direction = (currentPosition - previousPosition).normalized;
        float distance = Vector3.Distance(previousPosition, currentPosition);

        // 레이캐스트로 이동 경로상의 충돌 체크
        RaycastHit hit;
        if (Physics.Raycast(previousPosition, direction, out hit, distance + raycastDistance, collisionLayers))
        {
            // 발사자와의 충돌은 무시
            if (hit.transform == shooter) return;

            // 충돌 처리
            HandleCollision(hit.collider, hit.point);
        }
    }

    public void SetShooter(Transform shooterTransform)
    {
        shooter = shooterTransform;
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other, transform.position);
    }

    // ★ 충돌 처리 통합 함수
    private void HandleCollision(Collider other, Vector3 hitPoint)
    {
        // 발사자 자신과의 충돌 무시
        if (other.transform == shooter) return;

        // 플레이어 총알과 적 충돌
        if (isPlayerBullet && other.CompareTag("Enemy"))
        {
            var enemyController = other.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                // 크리티컬 확인
                bool isCritical = playerStats != null && playerStats.ShouldCritical();
                float finalDamage = damage;

                if (isCritical)
                {
                    finalDamage *= playerStats.criticalMultiplier;
                    Debug.Log($"💥 크리티컬 히트! {damage} → {finalDamage} 데미지");
                }

                // ★ 적에게 데미지와 정보 전달만 (데미지 넘버 표시는 적이 담당)
                enemyController.TakeDamage(finalDamage, isCritical, false);

                // 폭발 화살 체크
                if (playerStats != null && playerStats.hasExplosiveArrow)
                {
                    CreateExplosion(hitPoint, finalDamage, isCritical);
                }

                // 관통 확인
                if (remainingPierceCount > 0)
                {
                    remainingPierceCount--;
                    Debug.Log($"관통 공격! 데미지: {finalDamage} - 남은 관통: {remainingPierceCount}");

                    // 관통 후 유도 활성화 체크
                    if (playerStats != null && playerStats.hasGuidedAfterPierce && !isGuided)
                    {
                        ActivateGuidedMode();
                    }

                    // 이펙트 재생 (관통 시에도)
                    PlayHitEffectAt(hitPoint);
                    PlayHitSound();

                    return; // 관통 시 화살을 파괴하지 않고 계속 진행
                }
                else
                {
                    Debug.Log($"일반 공격! 데미지: {finalDamage} - 화살 파괴");
                    // 이펙트 재생
                    PlayHitEffectAt(hitPoint);
                    PlayHitSound();
                }
            }
        }
        // 적 총알과 플레이어 충돌
        else if (!isPlayerBullet && other.CompareTag("Player"))
        {
            var playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
                // 이펙트 재생
                PlayHitEffectAt(hitPoint);
                PlayHitSound();
            }
        }
        // 벽이나 장애물과 충돌
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle") || other.CompareTag("Ground"))
        {
            // 벽/땅에 맞으면 즉시 파괴
            PlayHitEffectAt(hitPoint);
            PlayHitSound();
        }
        else
        {
            // 기타 충돌 처리하지 않고 통과
            return;
        }

        // 총알 파괴
        DestroyProjectile();
    }

    // ★ 화살 파괴 함수
    private void DestroyProjectile()
    {
        // 물리 비활성화
        if (bulletRigidbody != null)
        {
            bulletRigidbody.velocity = Vector3.zero;
            bulletRigidbody.isKinematic = true;
        }

        // 콜라이더 비활성화
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 즉시 파괴
        Destroy(gameObject);
    }

    // 유도 모드 활성화
    private void ActivateGuidedMode()
    {
        isGuided = true;

        // 가장 가까운 적 찾기
        Transform closestEnemy = FindClosestEnemy();
        if (closestEnemy != null)
        {
            guidedTarget = closestEnemy;
        }
    }

    // 가장 가까운 적 찾기
    private Transform FindClosestEnemy()
    {
        if (playerStats == null) return null;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closest = null;
        float closestDistance = playerStats.guidedRange;

        foreach (GameObject enemy in enemies)
        {
            // 죽은 적은 제외
            var enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController == null || enemyController.currentHealth <= 0) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy.transform;
            }
        }

        return closest;
    }

    // 폭발 생성 (크리티컬 정보 포함)
    private void CreateExplosion(Vector3 explosionCenter, float originalDamage, bool wasCritical = false)
    {
        if (playerStats == null) return;

        // 폭발 반경 내의 모든 적 찾기
        Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, playerStats.explosiveRadius);

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                var enemyController = hit.GetComponent<EnemyController>();
                if (enemyController != null && enemyController.currentHealth > 0)
                {
                    // 폭발 데미지 계산
                    float explosionDamage = damage * playerStats.explosiveDamage;

                    // ★ 폭발 데미지도 적이 처리하도록 전달 (isExplosion = true)
                    enemyController.TakeDamage(explosionDamage, wasCritical, true);
                }
            }
        }

        // 폭발 이펙트 (크리티컬일 때 더 크게)
        if (hitPrefab != null)
        {
            var explosion = Instantiate(hitPrefab, explosionCenter, Quaternion.identity);

            // 크리티컬일 때 폭발 이펙트 1.5배 크게
            float effectScale = wasCritical ? 1.5f : 1f;
            explosion.transform.localScale = Vector3.one * (playerStats.explosiveRadius / 2f) * effectScale;

            var ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(explosion, ps.main.duration);
            else
                Destroy(explosion, 3f);
        }
    }

    private void PlayMuzzleEffect()
    {
        if (muzzlePrefab != null)
        {
            var muzzle = Instantiate(muzzlePrefab, transform.position, transform.rotation);
            var ps = muzzle.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(muzzle, ps.main.duration);
            else
                Destroy(muzzle, 2f);
        }
    }

    private void PlayShotSound()
    {
        if (shotSFX != null)
        {
            var audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
                audioSource.PlayOneShot(shotSFX);
        }
    }

    private void PlayHitSound()
    {
        if (hitSFX != null)
        {
            var audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
                audioSource.PlayOneShot(hitSFX);
        }
    }

    private void PlayHitEffect()
    {
        PlayHitEffectAt(transform.position);
    }

    // ★ 특정 위치에 히트 이펙트 재생
    private void PlayHitEffectAt(Vector3 position)
    {
        if (hitPrefab != null)
        {
            var hit = Instantiate(hitPrefab, position, Quaternion.identity);

            var ps = hit.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(hit, ps.main.duration);
            else
                Destroy(hit, 2f);
        }
    }

    private void OnDrawGizmos()
    {
        if (isPlayerBullet)
        {
            // 유도 모드일 때는 청록색, 일반일 때는 초록색
            Gizmos.color = isGuided ? Color.cyan : Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawWireSphere(transform.position, 0.1f);

        // ★ 레이캐스트 경로 표시
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 direction = (transform.position - previousPosition).normalized;
            Gizmos.DrawLine(previousPosition, transform.position + direction * raycastDistance);
        }

        // 유도 모드일 때 타겟과의 연결선 표시
        if (isGuided && guidedTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, guidedTarget.position);
        }

        // 폭발 화살일 때 폭발 반경 표시 (플레이어 화살만)
        if (isPlayerBullet && playerStats != null && playerStats.hasExplosiveArrow)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, playerStats.explosiveRadius);
        }
    }
}