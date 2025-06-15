using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;

public class ThirdPersonShooterController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private LayerMask aimColliderLayerMask;
    [SerializeField] private Transform debugTransform;
    [SerializeField] private Transform arrowProjectile;
    [SerializeField] private Transform spawnArrowPosition;
    [SerializeField] private float arrowSpawnDelay = 0.2f;

    [Header("Aim Movement Settings")]
    [SerializeField] private bool canMoveWhileAiming = false;
    [SerializeField] private float aimMoveSpeedMultiplier = 0.3f;

    [Header("Camera Aim Settings")]
    [SerializeField] private float maxAimDistance = 100f;
    [SerializeField] private bool useSpawnPositionForAiming = true;

    [Header("Jump Aim Restriction")]
    [SerializeField] private bool allowAimWhileJumping = false;
    [SerializeField] private bool exitAimOnJump = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;
    private Camera playerCamera;
    private PlayerStats playerStats; // 스탯 참조

    // 레이어 가중치 목표값 저장 변수
    private float aimLayerTarget = 0f;
    private float arrowSpawnTimer = 0f;
    private bool isArrowSpawnPending = false;
    private Vector3 lastMouseWorldPosition;

    // 조준 상태 관리
    private bool wasAiming = false;
    private bool wasGrounded = true;

    // 애니메이터 파라미터 해시
    private string ShootParamName = "Shoot";
    private int ShootParam;

    // 발사 상태 관리 - 애니메이션과 동기화하되 안전장치 추가
    private bool canShoot = true;
    private float lastShootTime = 0f;
    private bool isShootingInProgress = false;
    private float shootTimeoutDuration = 2f; // 애니메이션이 꼬였을 때 강제 리셋 시간

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeCooldown = 0.7f;
    [SerializeField] private float dodgeDuration = 0.5f;
    [SerializeField] private float dodgeSpeed = 10f;
    [SerializeField] private float dodgeSpeedMultiplier = 1.5f;

    // 회피 관련 변수
    [SerializeField] private bool isDodging = false;
    [SerializeField] private float dodgeTimer = 0f;
    [SerializeField] public float dodgeCooldownTimer = 0f;
    [SerializeField] private Vector3 dodgeDirection;

    // 애니메이터 파라미터 해시
    private int DodgeForwardParam;
    private int DodgeBackwardParam;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponentInChildren<Animator>();
        playerStats = GetComponent<PlayerStats>(); // 스탯 참조 추가

        // 플레이어 카메라 찾기
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }

        // 애니메이터 파라미터 해시 ID 얻기
        ShootParam = Animator.StringToHash(ShootParamName);
        DodgeForwardParam = Animator.StringToHash("DodgeForward");
        DodgeBackwardParam = Animator.StringToHash("DodgeBackward");
    }

    private void Update()
    {
        Vector3 mouseWorldPosition = CalculateAimPosition();

        // ★ 발사 쿨다운 업데이트 - 스탯 기반
        float shootCooldown = playerStats != null ? (1f / playerStats.FinalAttackSpeed) : 1f;

        if (!canShoot && Time.time - lastShootTime >= shootCooldown)
        {
            canShoot = true;
            isShootingInProgress = false;
            if (showDebugInfo) Debug.Log("발사 쿨다운 완료 - 발사 가능");
        }

        // ★ 애니메이션이 꼬였을 때 안전장치
        if (isShootingInProgress && Time.time - lastShootTime > shootTimeoutDuration)
        {
            if (showDebugInfo) Debug.LogWarning("발사 애니메이션 타임아웃 - 강제 리셋");
            canShoot = true;
            isShootingInProgress = false;
            isArrowSpawnPending = false;
        }

        // ★ 화살 발사 타이머 업데이트
        if (isArrowSpawnPending)
        {
            arrowSpawnTimer -= Time.deltaTime;
            if (arrowSpawnTimer <= 0)
            {
                SpawnArrow();
                isArrowSpawnPending = false;
            }
        }

        // 현재 애니메이터 상태 확인 - 발사 완료 감지
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);

        // ★ 발사 애니메이션 완료 감지
        if (isShootingInProgress && stateInfo.IsName("Attack_1Shoot_Loop"))
        {
            // 애니메이션이 95% 완료되면 발사 완료 처리
            if (stateInfo.normalizedTime >= 0.95f)
            {
                if (showDebugInfo) Debug.Log("발사 애니메이션 완료 감지");
                // 쿨다운은 스탯 기반으로 별도 처리
            }
        }

        // 접지 상태 확인 및 점프 감지
        bool currentlyGrounded = thirdPersonController.Grounded;
        bool justJumped = wasGrounded && !currentlyGrounded;

        // 조준 상태 처리
        bool aimInputPressed = starterAssetsInputs.aim;
        bool canAim = CanAim(currentlyGrounded, justJumped);
        bool shouldAim = aimInputPressed && canAim;

        // 실시간으로 회전 설정 업데이트
        thirdPersonController.SetRotateOnMove(!shouldAim);

        if (shouldAim)
        {
            if (!wasAiming)
            {
                OnStartAiming();
            }
            HandleAiming(mouseWorldPosition);
        }
        else
        {
            if (wasAiming)
            {
                OnStopAiming();
            }
        }

        // 이전 프레임 상태 저장
        wasAiming = shouldAim;
        wasGrounded = currentlyGrounded;

        // 에임 레이어 가중치 부드럽게 업데이트
        animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), aimLayerTarget, Time.deltaTime * 10f));

        // 회피 처리
        HandleDodge();
    }

    // 조준 가능 여부 확인
    private bool CanAim(bool isGrounded, bool justJumped)
    {
        if (!allowAimWhileJumping && !isGrounded)
        {
            if (wasAiming && showDebugInfo)
            {
                Debug.Log("점프로 인해 조준 모드 해제");
            }
            return false;
        }

        if (exitAimOnJump && justJumped && wasAiming)
        {
            if (showDebugInfo) Debug.Log("점프 감지 - 조준 모드 해제");
            return false;
        }

        if (isDodging)
        {
            return false;
        }

        return true;
    }

    // 카메라 방향을 고려한 조준 위치 계산
    private Vector3 CalculateAimPosition()
    {
        Vector3 aimPosition = Vector3.zero;

        if (useSpawnPositionForAiming && spawnArrowPosition != null)
        {
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray screenRay = playerCamera.ScreenPointToRay(screenCenterPoint);

            Vector3 targetPoint;
            if (Physics.Raycast(screenRay, out RaycastHit hit, maxAimDistance, aimColliderLayerMask))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = screenRay.origin + screenRay.direction * maxAimDistance;
            }

            Vector3 aimDirection = (targetPoint - spawnArrowPosition.position).normalized;

            if (Physics.Raycast(spawnArrowPosition.position, aimDirection, out RaycastHit spawnHit, maxAimDistance, aimColliderLayerMask))
            {
                aimPosition = spawnHit.point;
            }
            else
            {
                aimPosition = spawnArrowPosition.position + aimDirection * maxAimDistance;
            }

            debugTransform.position = aimPosition;
        }
        else
        {
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = playerCamera.ScreenPointToRay(screenCenterPoint);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, maxAimDistance, aimColliderLayerMask))
            {
                aimPosition = raycastHit.point;
            }
            else
            {
                aimPosition = ray.origin + ray.direction * maxAimDistance;
            }
            debugTransform.position = aimPosition;
        }

        lastMouseWorldPosition = aimPosition;
        return aimPosition;
    }

    // 조준 시작
    private void OnStartAiming()
    {
        if (showDebugInfo) Debug.Log("조준 시작 - 이동 제한");
        aimVirtualCamera.gameObject.SetActive(true);
        thirdPersonController.SetSensivitity(aimSensitivity);
        thirdPersonController.SetRotateOnMove(false);

        if (!canMoveWhileAiming && thirdPersonController.Grounded)
        {
            thirdPersonController.SetCanMove(false);
        }

        aimLayerTarget = 1f;
    }

    // 조준 중 처리
    private void HandleAiming(Vector3 mouseWorldPosition)
    {
        Vector3 worldAimTarget = mouseWorldPosition;
        worldAimTarget.y = transform.position.y;
        Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

        if (aimDirection != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20.0f);
        }

        // ★ 발사 조건 - 애니메이션과 동기화
        bool shootConditions = thirdPersonController.Grounded &&  // 땅에 있어야 함
                               canShoot &&                       // 쿨다운 완료
                               !isShootingInProgress &&          // 발사 중이 아님
                               !isArrowSpawnPending;             // 화살 생성 대기 중이 아님

        if (starterAssetsInputs.shoot && shootConditions)
        {
            HandleShooting();
            starterAssetsInputs.shoot = false; // 즉시 입력 소모
        }
        else if (starterAssetsInputs.shoot)
        {
            // 발사할 수 없는 상황들 - 디버그 정보
            if (showDebugInfo)
            {
                if (!thirdPersonController.Grounded)
                    Debug.Log("공중에서는 발사할 수 없습니다");
                else if (!canShoot)
                {
                    float shootCooldown = playerStats != null ? (1f / playerStats.FinalAttackSpeed) : 1f;
                    Debug.Log($"발사 쿨다운 중: {shootCooldown - (Time.time - lastShootTime):F2}초 남음");
                }
                else if (isShootingInProgress)
                    Debug.Log("발사 애니메이션 진행 중");
                else if (isArrowSpawnPending)
                    Debug.Log("화살 생성 중");
            }

            starterAssetsInputs.shoot = false; // 입력 소모
        }
    }

    // 조준 종료
    private void OnStopAiming()
    {
        if (showDebugInfo) Debug.Log("조준 종료 - 이동 허용");
        aimVirtualCamera.gameObject.SetActive(false);
        thirdPersonController.SetSensivitity(normalSensitivity);
        thirdPersonController.SetRotateOnMove(true);
        thirdPersonController.SetCanMove(true);
        aimLayerTarget = 0f;
    }

    // ★ 발사 처리 - 애니메이션과 동기화하되 안전장치 포함
    private void HandleShooting()
    {
        if (showDebugInfo) Debug.Log("발사 요청 처리 시작");

        // 발사 상태 업데이트
        canShoot = false;
        isShootingInProgress = true;
        lastShootTime = Time.time;

        // 애니메이션 트리거 (안전하게)
        try
        {
            animator.SetInteger(ShootParam, 1);
            StartCoroutine(ResetShootParameterNextFrame());
        }
        catch (System.Exception e)
        {
            if (showDebugInfo) Debug.LogError($"애니메이션 트리거 실패: {e.Message}");
            // 애니메이션 실패 시 즉시 화살 발사
            isArrowSpawnPending = true;
            arrowSpawnTimer = 0.1f; // 짧은 딜레이 후 발사
            return;
        }

        // 화살 생성 타이머 시작
        isArrowSpawnPending = true;
        arrowSpawnTimer = arrowSpawnDelay;

        // 현재 조준점 저장
        lastMouseWorldPosition = CalculateAimPosition();

        if (showDebugInfo) Debug.Log($"화살 생성 예약 - {arrowSpawnDelay}초 후 생성");
    }

    // ★ 화살 생성 함수 분리
    private void SpawnArrow()
    {
        if (arrowProjectile == null)
        {
            Debug.LogError("arrowProjectile이 null입니다!");
            return;
        }

        if (spawnArrowPosition == null)
        {
            Debug.LogError("spawnArrowPosition이 null입니다!");
            return;
        }

        // 화살 발사
        Vector3 aimDir = (lastMouseWorldPosition - spawnArrowPosition.position).normalized;
        Transform arrow = Instantiate(arrowProjectile, spawnArrowPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));

        // 발사자 정보 설정
        ProjectileMoveScript projectile = arrow.GetComponent<ProjectileMoveScript>();
        if (projectile != null)
        {
            projectile.SetShooter(transform);
            projectile.isPlayerBullet = true;

            // 스탯 기반 데미지 적용
            if (playerStats != null)
            {
                projectile.damage = playerStats.FinalAttackPower;
            }

            if (showDebugInfo) Debug.Log($"화살 발사 완료 - 데미지: {projectile.damage}");
        }
        else
        {
            Debug.LogError("ProjectileMoveScript를 찾을 수 없습니다!");
        }
    }

    // ★ 애니메이션 파라미터 리셋 - 실패해도 발사에 영향 없음
    private System.Collections.IEnumerator ResetShootParameterNextFrame()
    {
        yield return null; // 한 프레임 대기
        try
        {
            animator.SetInteger(ShootParam, 0);
            if (showDebugInfo) Debug.Log("Shoot 파라미터 자동 리셋");
        }
        catch (System.Exception e)
        {
            if (showDebugInfo) Debug.LogWarning($"애니메이션 리셋 실패: {e.Message} - 발사에는 영향 없음");
        }
    }

    // 회피 처리
    private void HandleDodge()
    {
        if (dodgeCooldownTimer > 0)
        {
            dodgeCooldownTimer -= Time.deltaTime;
        }

        if (isDodging)
        {
            dodgeTimer -= Time.deltaTime;
            transform.position += dodgeDirection * dodgeSpeed * Time.deltaTime;

            if (dodgeTimer <= 0)
            {
                isDodging = false;
                if (!starterAssetsInputs.aim)
                {
                    thirdPersonController.SetCanMove(true);
                }
            }
        }
        else
        {
            if (!starterAssetsInputs.aim && dodgeCooldownTimer <= 0 && thirdPersonController.Grounded)
            {
                if (starterAssetsInputs.dodgeForward)
                {
                    StartDodge(transform.forward);
                    animator.SetTrigger(DodgeForwardParam);
                    starterAssetsInputs.dodgeForward = false;
                }
                else if (starterAssetsInputs.dodgeBackward)
                {
                    StartDodge(-transform.forward);
                    animator.SetTrigger(DodgeBackwardParam);
                    starterAssetsInputs.dodgeBackward = false;
                }
            }
        }
    }

    // 회피 시작
    private void StartDodge(Vector3 direction)
    {
        isDodging = true;
        dodgeTimer = dodgeDuration;
        dodgeCooldownTimer = dodgeCooldown;
        dodgeDirection = direction.normalized;
        thirdPersonController.SetCanMove(false);
        if (showDebugInfo) Debug.Log("회피 시작 - 이동 제어 일시 중지");
    }
}