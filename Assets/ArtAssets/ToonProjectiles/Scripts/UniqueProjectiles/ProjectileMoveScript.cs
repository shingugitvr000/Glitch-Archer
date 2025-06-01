using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyAI;

public class ProjectileMoveScript : MonoBehaviour
{
    [Header("기본 설정")]
    public float speed;
    public float fireRate;
    public float damage = 25f;              // 데미지 추가

    [Header("이펙트")]
    public GameObject muzzlePrefab;
    public GameObject hitPrefab;
    public AudioClip shotSFX;
    public AudioClip hitSFX;
    public List<GameObject> trails;

    [Header("발사자 정보")]
    public bool isPlayerBullet = true;      // 플레이어가 쏜 총알인지

    private float speedRandomness;
    private Vector3 offset;
    private bool collided;
    private Rigidbody bulletRigidbody;
    private Transform shooter;              // 발사자 참조

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // 중력 비활성화 (떨어지는 문제 해결)
        if (bulletRigidbody != null)
        {
            bulletRigidbody.useGravity = false;
            bulletRigidbody.drag = 0f;
            bulletRigidbody.angularDrag = 0f;
        }

        // 발사자 찾기 (플레이어 총알인 경우)
        if (isPlayerBullet && shooter == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                shooter = player.transform;
            }
        }

        // 기존 이펙트 코드
        if (muzzlePrefab != null)
        {
            var muzzleVFX = Instantiate(muzzlePrefab, transform.position, Quaternion.identity);
            muzzleVFX.transform.forward = gameObject.transform.forward + offset;
            var ps = muzzleVFX.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(muzzleVFX, ps.main.duration);
            else
            {
                var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(muzzleVFX, psChild.main.duration);
            }
        }

        if (shotSFX != null && GetComponent<AudioSource>())
        {
            GetComponent<AudioSource>().PlayOneShot(shotSFX);
        }

        bulletRigidbody.velocity = transform.forward * speed;
    }

    void Update()
    {

    }

    void OnCollisionEnter(Collision co)
    {
        if (co.gameObject.tag != "Bullet" && !collided)
        {
            collided = true;

            // 새로운 적 피격 처리
            HandleEnemyHit(co.gameObject);

            // 기존 사운드 및 이펙트 코드
            if (hitSFX != null && GetComponent<AudioSource>())
            {
                GetComponent<AudioSource>().PlayOneShot(hitSFX);
            }

            if (trails.Count > 0)
            {
                for (int i = 0; i < trails.Count; i++)
                {
                    trails[i].transform.parent = null;
                    var ps = trails[i].GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        ps.Stop();
                        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
                    }
                }
            }

            speed = 0;
            GetComponent<Rigidbody>().isKinematic = true;
            ContactPoint contact = co.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 pos = contact.point;

            if (hitPrefab != null)
            {
                var hitVFX = Instantiate(hitPrefab, pos, rot);
                var ps = hitVFX.GetComponent<ParticleSystem>();
                if (ps == null)
                {
                    var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                    Destroy(hitVFX, psChild.main.duration);
                }
                else
                    Destroy(hitVFX, ps.main.duration);
            }

            StartCoroutine(DestroyParticle(0f));
        }
    }

    // 새로운 적 피격 처리 메서드
    private void HandleEnemyHit(GameObject hitObject)
    {
        // 플레이어가 쏜 총알이고 적에게 맞은 경우에만 처리
        if (!isPlayerBullet || !hitObject.CompareTag("Enemy") || shooter == null)
        {
            return;
        }

        Debug.Log($"플레이어 총알이 적 {hitObject.name}에게 명중!");

        // 새로운 통합 피해 시스템 사용
        EnemyFSM enemyFSM = hitObject.GetComponent<EnemyFSM>();
        if (enemyFSM != null)
        {
            // 플레이어 참조 설정
            enemyFSM.player = shooter;

            // *** 새로운 피해 시스템 호출 (어그로 자동 활성화됨) ***
            enemyFSM.TakeDamage(damage, shooter.gameObject);

            Debug.Log($"적 {hitObject.name}에게 {damage} 데미지! 현재 체력: {enemyFSM.currentHealth}/{enemyFSM.maxHealth}");

            // 주변 적들에게도 경고
            AlertNearbyEnemies(hitObject.transform.position, shooter);
        }
        else
        {
            Debug.LogWarning($"{hitObject.name}에 EnemyFSM이 없습니다!");
        }
    }

    // 주변 적들에게 경고 (기존 메서드도 약간 수정)
    private void AlertNearbyEnemies(Vector3 hitPosition, Transform player)
    {
        float alertRadius = 15f; // 경고 범위

        // 주변의 적들 찾기
        Collider[] nearbyObjects = Physics.OverlapSphere(hitPosition, alertRadius);

        foreach (Collider col in nearbyObjects)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyFSM nearbyEnemy = col.GetComponent<EnemyFSM>();
                if (nearbyEnemy != null)
                {
                    // 플레이어 참조 설정
                    nearbyEnemy.player = player;

                    // *** 어그로 활성화 (피해받지 않았지만 동료가 공격받았으므로) ***
                    nearbyEnemy.ActivateAggro();

                    // 패트롤 중인 적만 추적 상태로 변경
                    if (nearbyEnemy.StateManager.CurrentState == nearbyEnemy.patrolState)
                    {
                        nearbyEnemy.StateManager.ChangeState(nearbyEnemy.chaseState);
                        Debug.Log($"{col.name}: 동료 피격 감지! → 어그로 추적 시작");
                    }
                }
            }
        }
    }

    public IEnumerator DestroyParticle(float waitTime)
    {
        if (transform.childCount > 0 && waitTime != 0)
        {
            List<Transform> tList = new List<Transform>();
            foreach (Transform t in transform.GetChild(0).transform)
            {
                tList.Add(t);
            }
            while (transform.GetChild(0).localScale.x > 0)
            {
                yield return new WaitForSeconds(0.01f);
                transform.GetChild(0).localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                for (int i = 0; i < tList.Count; i++)
                {
                    tList[i].localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                }
            }
        }

        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
    }

    // 발사자 설정 메서드 (외부에서 호출 가능)
    public void SetShooter(Transform shooterTransform)
    {
        shooter = shooterTransform;
        isPlayerBullet = shooterTransform != null && shooterTransform.CompareTag("Player");
    }
}