using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMoveScript : MonoBehaviour
{
    [Header("기본 설정")]
    public float speed = 20f;
    public float fireRate = 1f;        // 기존 호환성을 위해 유지 (사용 안함)
    public float damage = 25f;
    public bool isPlayerBullet = true;

    [Header("이펙트 (선택)")]
    public GameObject muzzlePrefab;
    public GameObject hitPrefab;
    public AudioClip shotSFX;
    public AudioClip hitSFX;
    public List<GameObject> trails;

    private bool collided;
    private Rigidbody bulletRigidbody;
    private Transform shooter;

    void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // 물리 설정
        if (bulletRigidbody != null)
        {
            bulletRigidbody.useGravity = false;
            bulletRigidbody.drag = 0f;
            bulletRigidbody.velocity = transform.forward * speed;
        }

        // 이펙트 재생
        PlayMuzzleEffect();
        PlayShotSound();

        // 자동 파괴
        Destroy(gameObject, 5f);
    }

    void OnCollisionEnter(Collision collision)
    {
        //if (collision.gameObject.CompareTag("Bullet") || collided) return;

        collided = true;

        // 피격 처리
        HandleHit(collision.gameObject);

        // 이펙트 재생
        PlayHitEffect(collision);
        PlayHitSound();

        // 총알 정지 및 파괴
        StopBullet();
    }

    private void HandleHit(GameObject hitObject)
    {
        // 플레이어 총알이 적을 맞춘 경우
        if (isPlayerBullet && hitObject.CompareTag("Enemy"))
        {
            var enemy = hitObject.GetComponent<EnemyController>();
            if (enemy != null)
            {
                // 플레이어 참조 설정
                if (shooter != null)
                {
                    enemy.player = shooter;
                }

                // 데미지 적용
                enemy.TakeDamage(damage);

                Debug.Log($"적 {hitObject.name}에게 {damage} 데미지!");
            }
        }
        // 적 총알이 플레이어를 맞춘 경우
        else if (!isPlayerBullet && hitObject.CompareTag("Player"))
        {
            // 플레이어 체력 시스템이 있다면 여기서 처리
            Debug.Log($"플레이어가 {damage} 데미지를 받았습니다!");

            // 예시: 플레이어 체력 컴포넌트가 있다면
            // var playerHealth = hitObject.GetComponent<PlayerHealth>();
            // if (playerHealth != null) playerHealth.TakeDamage(damage);
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

    private void PlayHitEffect(Collision collision)
    {
        if (hitPrefab != null)
        {
            ContactPoint contact = collision.contacts[0];
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
            var hit = Instantiate(hitPrefab, contact.point, rotation);

            var ps = hit.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(hit, ps.main.duration);
            else
                Destroy(hit, 2f);
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

    private void StopBullet()
    {
        speed = 0;
        if (bulletRigidbody != null)
        {
            bulletRigidbody.velocity = Vector3.zero;
            bulletRigidbody.isKinematic = true;
        }

        // 트레일 정리
        if (trails.Count > 0)
        {
            foreach (var trail in trails)
            {
                if (trail != null)
                {
                    trail.transform.parent = null;
                    var ps = trail.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        ps.Stop();
                        Destroy(trail, ps.main.duration + ps.main.startLifetime.constantMax);
                    }
                }
            }
        }

        // 총알 제거
        StartCoroutine(DestroyBullet());
    }

    private IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

    // 발사자 설정 (외부에서 호출)
    public void SetShooter(Transform shooterTransform)
    {
        shooter = shooterTransform;
        isPlayerBullet = shooterTransform != null && shooterTransform.CompareTag("Player");
    }
}