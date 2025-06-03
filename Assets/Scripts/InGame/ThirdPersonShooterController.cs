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
    [SerializeField] private float arrowSpawnDelay = 0.2f; // 화살 발사 지연 시간

    [Header("Aim Movement Settings")]
    [SerializeField] private bool canMoveWhileAiming = false; // 조준 중 이동 가능 여부
    [SerializeField] private float aimMoveSpeedMultiplier = 0.3f; // 조준 중 이동 속도 배율 (사용 안 함)

    [Header("Camera Aim Settings")]
    [SerializeField] private float maxAimDistance = 100f; // 최대 조준 거리
    [SerializeField] private bool useSpawnPositionForAiming = true; // 스폰 위치 기준 조준

    [Header("Jump Aim Restriction")]
    [SerializeField] private bool allowAimWhileJumping = false; // 점프 중 조준 허용 여부
    [SerializeField] private bool exitAimOnJump = true; // 점프 시 조준 모드 자동 해제

    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;
    private Camera playerCamera; // 플레이어 카메라 참조

    // 레이어 가중치 목표값 저장 변수
    private float aimLayerTarget = 0f;
    private float arrowSpawnTimer = 0f; // 화살 발사 타이머
    private bool isArrowSpawnPending = false; // 화살 발사 대기 중인지 확인
    private Vector3 lastMouseWorldPosition; // 마지막 마우스 위치 저장

    // 조준 상태 관리
    private bool wasAiming = false; // 이전 프레임에서 조준 중이었는지
    private bool wasGrounded = true; // 이전 프레임에서 접지 상태였는지

    // 애니메이터 파라미터 해시 (성능 최적화)
    private string ShootParamName = "Shoot"; // 실제 파라미터 이름
    private int ShootParam;

    // 발사 상태 관리
    private bool isShootingRequested = false; // 발사 요청 상태
    private bool isShootAnimationActive = false; // 발사 애니메이션 활성화 상태
    private float shootAnimationCooldown = 0f; // 연속 발사 방지 쿨다운

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeCooldown = 0.7f; // 회피 쿨다운 시간
    [SerializeField] private float dodgeDuration = 0.5f; // 회피 동작 시간
    [SerializeField] private float dodgeSpeed = 10f; // 회피시 이동 속도
    [SerializeField] private float dodgeSpeedMultiplier = 1.5f; // 회피 시 속도 증가 배율

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

        // 플레이어 카메라 찾기
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }

        // 애니메이터 파라미터 해시 ID 얻기
        ShootParam = Animator.StringToHash(ShootParamName);

        // 회피 파라미터 해시 초기화
        DodgeForwardParam = Animator.StringToHash("DodgeForward");
        DodgeBackwardParam = Animator.StringToHash("DodgeBackward");

        // 디버깅 - 애니메이터 파라미터 목록 확인
        AnimatorControllerParameter[] parameters = animator.parameters;
        Debug.Log("애니메이터 파라미터 목록:");
        foreach (var param in parameters)
        {
            Debug.Log($"이름: {param.name}, 타입: {param.type}, 해시: {Animator.StringToHash(param.name)}");
            if (param.name == ShootParamName)
            {
                Debug.Log($"Shoot 파라미터 발견: 해시 ID = {ShootParam}");
            }
        }
    }

    private void Update()
    {
        Vector3 mouseWorldPosition = CalculateAimPosition();

        // 쿨다운 타이머 업데이트
        if (shootAnimationCooldown > 0)
        {
            shootAnimationCooldown -= Time.deltaTime;
        }

        // 화살 발사 타이머 업데이트
        if (isArrowSpawnPending)
        {
            arrowSpawnTimer -= Time.deltaTime;
            if (arrowSpawnTimer <= 0)
            {
                // 화살 발사 - 스폰 위치에서 마지막 조준점으로
                Vector3 aimDir = (lastMouseWorldPosition - spawnArrowPosition.position).normalized;
                Transform arrow = Instantiate(arrowProjectile, spawnArrowPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));

                // 🎯 중요: 발사자 정보 설정 (플레이어가 쏜 총알)
                ProjectileMoveScript projectile = arrow.GetComponent<ProjectileMoveScript>();
                if (projectile != null)
                {
                    projectile.SetShooter(transform);               // 발사자를 플레이어로 설정
                    projectile.isPlayerBullet = true;               // 플레이어 총알임
                    projectile.damage = 25f;                        // 플레이어 총알 데미지
                }

                isArrowSpawnPending = false;
                Debug.Log("화살 발사 완료");
            }
        }

        // 현재 애니메이터 상태 및 Shoot 파라미터 확인
        int currentShootValue = animator.GetInteger(ShootParam);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);

        // 발사 애니메이션 상태 추적
        if (stateInfo.IsName("Attack_1Shoot_Loop"))
        {
            isShootAnimationActive = true;

            // 애니메이션이 거의 끝나면 Shoot 파라미터 리셋
            if (stateInfo.normalizedTime >= 0.9f && currentShootValue != 0)
            {
                animator.SetInteger(ShootParam, 0);
                isShootingRequested = false;
                shootAnimationCooldown = 0.5f; // 연속 발사 방지를 위한 쿨다운 설정
                Debug.Log("애니메이션 완료 - Shoot 파라미터 리셋");
            }
        }
        else
        {
            isShootAnimationActive = false;
        }

        // 접지 상태 확인 및 점프 감지
        bool currentlyGrounded = thirdPersonController.Grounded;
        bool justJumped = wasGrounded && !currentlyGrounded; // 방금 점프했는지 확인

        // 조준 상태 처리
        bool aimInputPressed = starterAssetsInputs.aim;
        bool canAim = CanAim(currentlyGrounded, justJumped);
        bool shouldAim = aimInputPressed && canAim;

        if (shouldAim)
        {
            // 조준 시작 시 처리
            if (!wasAiming)
            {
                OnStartAiming();
            }

            // 조준 중 처리
            HandleAiming(mouseWorldPosition);
        }
        else
        {
            // 조준 종료 시 처리
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

        // 회피 처리 (조준 중이 아닐 때만)
        HandleDodge();
    }

    // 조준 가능 여부 확인
    private bool CanAim(bool isGrounded, bool justJumped)
    {
        // 점프 중 조준 제한 확인
        if (!allowAimWhileJumping && !isGrounded)
        {
            if (wasAiming)
            {
                Debug.Log("점프로 인해 조준 모드 해제");
            }
            return false;
        }

        // 점프 시 조준 모드 자동 해제
        if (exitAimOnJump && justJumped && wasAiming)
        {
            Debug.Log("점프 감지 - 조준 모드 해제");
            return false;
        }

        // 회피 중에는 조준 불가
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
            // 방법 1: 스폰 위치에서 카메라 방향으로 레이캐스트
            Vector3 cameraForward = playerCamera.transform.forward;
            Vector3 rayOrigin = spawnArrowPosition.position;
            Vector3 rayDirection = cameraForward;

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxAimDistance, aimColliderLayerMask))
            {
                aimPosition = hit.point;
                debugTransform.position = hit.point;
            }
            else
            {
                // 충돌하지 않으면 최대 거리의 점 설정
                aimPosition = rayOrigin + rayDirection * maxAimDistance;
                debugTransform.position = aimPosition;
            }
        }
        else
        {
            // 방법 2: 기존 방식 (화면 중앙에서 레이캐스트)
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = playerCamera.ScreenPointToRay(screenCenterPoint);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, maxAimDistance, aimColliderLayerMask))
            {
                aimPosition = raycastHit.point;
                debugTransform.position = raycastHit.point;
            }
            else
            {
                // 충돌하지 않으면 레이 방향으로 최대 거리
                aimPosition = ray.origin + ray.direction * maxAimDistance;
                debugTransform.position = aimPosition;
            }
        }

        lastMouseWorldPosition = aimPosition; // 마지막 조준 위치 저장
        return aimPosition;
    }

    // 조준 시작 시 호출
    private void OnStartAiming()
    {
        Debug.Log("조준 시작 - 이동 제한");

        // 에임 카메라 및 설정 활성화
        aimVirtualCamera.gameObject.SetActive(true);
        thirdPersonController.SetSensivitity(aimSensitivity);
        thirdPersonController.SetRotateOnMove(false);

        // 조준 중 이동 제한 (접지 상태일 때만)
        if (!canMoveWhileAiming && thirdPersonController.Grounded)
        {
            thirdPersonController.SetCanMove(false);
        }

        // 에임 레이어 목표 설정
        aimLayerTarget = 1f;
    }

    // 조준 중 처리
    private void HandleAiming(Vector3 mouseWorldPosition)
    {
        // 에임 방향으로 캐릭터 회전 (Y축만)
        Vector3 worldAimTarget = mouseWorldPosition;
        worldAimTarget.y = transform.position.y;
        Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

        if (aimDirection != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20.0f);
        }

        // Shoot 파라미터 초기화 (필요한 경우)
        if (animator.GetInteger(ShootParam) == 1)
        {
            animator.SetInteger(ShootParam, 0);
        }

        // 발사 처리 (접지 상태일 때만)
        if (starterAssetsInputs.shoot && thirdPersonController.Grounded)
        {
            HandleShooting();
            starterAssetsInputs.shoot = false;
        }
        else if (starterAssetsInputs.shoot && !thirdPersonController.Grounded)
        {
            // 공중에서 발사 시도 시 입력 소모만 하고 발사하지 않음
            starterAssetsInputs.shoot = false;
            Debug.Log("공중에서는 발사할 수 없습니다");
        }
    }

    // 조준 종료 시 호출
    private void OnStopAiming()
    {
        Debug.Log("조준 종료 - 이동 허용");

        // 에임 비활성화 상태
        aimVirtualCamera.gameObject.SetActive(false);
        thirdPersonController.SetSensivitity(normalSensitivity);
        thirdPersonController.SetRotateOnMove(true);

        // 이동 제한 해제
        thirdPersonController.SetCanMove(true);

        // 에임 레이어 목표 0으로 설정
        aimLayerTarget = 0f;
    }

    // 발사 처리
    private void HandleShooting()
    {
        // 발사 요청 플래그 설정
        isShootingRequested = true;

        // Shoot 파라미터 설정 전에 로그
        Debug.Log($"발사 요청 - 현재 Shoot 값: {animator.GetInteger(ShootParam)}, 시도합니다...");

        // Shoot 파라미터를 1로 설정하여 애니메이션 트리거
        animator.SetInteger(ShootParam, 1);

        // 설정 후 확인
        Debug.Log($"Shoot 파라미터 설정 완료 - 새 값: {animator.GetInteger(ShootParam)}");

        // 화살 발사 타이머 설정
        isArrowSpawnPending = true;
        arrowSpawnTimer = arrowSpawnDelay;

        // 현재 조준점 저장 (발사 시점의 정확한 방향)
        lastMouseWorldPosition = CalculateAimPosition();
    }

    // 회피 처리
    private void HandleDodge()
    {
        // 회피 쿨다운 타이머 업데이트
        if (dodgeCooldownTimer > 0)
        {
            dodgeCooldownTimer -= Time.deltaTime;
        }

        // 현재 회피 중인지 확인 및 처리
        if (isDodging)
        {
            dodgeTimer -= Time.deltaTime;

            // 회피 이동 처리
            transform.position += dodgeDirection * dodgeSpeed * Time.deltaTime;

            // 회피 종료 처리
            if (dodgeTimer <= 0)
            {
                isDodging = false;

                // 조준 중이 아니면 ThirdPersonController 스크립트의 이동 제어 복원
                if (!starterAssetsInputs.aim)
                {
                    thirdPersonController.SetCanMove(true);
                }
            }
        }
        else
        {
            // 회피 시작 처리 - 쿨다운이 끝났고 현재 에임이 아닐때만, 그리고 접지 상태일 때만
            if (!starterAssetsInputs.aim && dodgeCooldownTimer <= 0 && thirdPersonController.Grounded)
            {
                // 앞으로 회피
                if (starterAssetsInputs.dodgeForward)
                {
                    StartDodge(transform.forward);
                    // 애니메이션 트리거 (Shoot과 같은 방식으로 즉시 리셋)
                    animator.SetTrigger(DodgeForwardParam);

                    starterAssetsInputs.dodgeForward = false;
                }
                // 뒤로 회피
                else if (starterAssetsInputs.dodgeBackward)
                {
                    StartDodge(-transform.forward);
                    // 애니메이션 트리거
                    animator.SetTrigger(DodgeBackwardParam);

                    starterAssetsInputs.dodgeBackward = false;
                }
            }
        }
    }

    // 회피 시작 메서드
    private void StartDodge(Vector3 direction)
    {
        isDodging = true;
        dodgeTimer = dodgeDuration;
        dodgeCooldownTimer = dodgeCooldown;
        dodgeDirection = direction.normalized;

        // ThirdPersonController 스크립트의 이동 제어 일시 중지
        thirdPersonController.SetCanMove(false);

        Debug.Log("회피 시작 - 이동 제어 일시 중지");
    }

    // 디버그용 기즈모 그리기
    private void OnDrawGizmos()
    {
        if (spawnArrowPosition != null && playerCamera != null)
        {
            // 스폰 위치에서 카메라 방향으로 선 그리기
            Gizmos.color = Color.red;
            Vector3 direction = playerCamera.transform.forward;
            Gizmos.DrawLine(spawnArrowPosition.position, spawnArrowPosition.position + direction * maxAimDistance);

            // 스폰 위치 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spawnArrowPosition.position, 0.1f);

            // 마지막 조준점 표시
            if (lastMouseWorldPosition != Vector3.zero)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(lastMouseWorldPosition, 0.2f);
                Gizmos.DrawLine(spawnArrowPosition.position, lastMouseWorldPosition);
            }

            // 접지 상태 표시
            if (thirdPersonController != null)
            {
                Gizmos.color = thirdPersonController.Grounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
    }
}