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

    private Transform shooter;
    private PlayerStats playerStats; // 플레이어 스탯 참조
    private Rigidbody bulletRigidbody;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
        if (bulletRigidbody == null)
        {
            bulletRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        // Rigidbody 설정 (기존 ArrowProjectile 방식과 동일)
        bulletRigidbody.useGravity = false;
        bulletRigidbody.drag = 0f;
    }

    private void Start()
    {
        // 화살 이동 (기존 ArrowProjectile 로직)
        bulletRigidbody.velocity = transform.forward * speed;

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
                Debug.Log($"플레이어 화살 데미지: {damage}");
            }
        }

        // 이펙트 재생
        PlayMuzzleEffect();
        PlayShotSound();

        Destroy(gameObject, 5f);
    }

    public void SetShooter(Transform shooterTransform)
    {
        shooter = shooterTransform;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 발사자 자신과의 충돌 무시
        if (other.transform == shooter) return;

        // 플레이어 총알과 적 충돌
        if (isPlayerBullet && other.CompareTag("Enemy"))
        {
            var enemyController = other.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                // 데미지 적용
                enemyController.TakeDamage(damage);

                // 관통 확인 - 관통하지 않으면 화살 파괴
                bool shouldPierce = playerStats != null && playerStats.ShouldPierce();

                if (shouldPierce)
                {
                    Debug.Log($"관통 공격! 데미지: {damage} - 화살 계속 진행");
                    return; // 관통 시 화살을 파괴하지 않고 계속 진행
                }
                else
                {
                    Debug.Log($"일반 공격! 데미지: {damage} - 화살 파괴");
                    // 일반 공격 시 화살 파괴
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
            }
        }
        // 벽이나 장애물과 충돌
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            // 벽에 맞으면 무조건 파괴          

            // 이펙트 재생
            PlayHitEffect();
            PlayHitSound();
        }
        else
        {
            // 기타 충돌 처리하지 않고 통과
            return;
        }

        // 총알 파괴
        Destroy(gameObject);
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
        if (hitPrefab != null)
        {
            
            var hit = Instantiate(hitPrefab, gameObject.transform.position, Quaternion.identity);

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
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}