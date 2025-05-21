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

    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;
    // 레이어 가중치 목표값 저장 변수
    private float aimLayerTarget = 0f;
    private float arrowSpawnTimer = 0f; // 화살 발사 타이머
    private bool isArrowSpawnPending = false; // 화살 발사 대기 중인지 확인
    private Vector3 lastMouseWorldPosition; // 마지막 마우스 위치 저장

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
        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
            lastMouseWorldPosition = mouseWorldPosition; // 마지막 마우스 위치 저장
        }

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
                // 화살 발사
                Vector3 aimDir = (lastMouseWorldPosition - spawnArrowPosition.position).normalized;
                Instantiate(arrowProjectile, spawnArrowPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
                isArrowSpawnPending = false;
                Debug.Log("화살 발사 완료");
            }
        }

        // 현재 애니메이터 상태 및 Shoot 파라미터 확인
        int currentShootValue = animator.GetInteger(ShootParam);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);

        // 애니메이션 상태 디버깅
        if (Time.frameCount % 30 == 0) // 30프레임마다 로그 출력 (콘솔 범람 방지)
        {
            Debug.Log($"프레임: {Time.frameCount}, Shoot 값: {currentShootValue}, " +
                      $"애니메이션 상태: {stateInfo.shortNameHash}, " +
                      $"시간: {stateInfo.normalizedTime:F2}, " +
                      $"isShootingRequested: {isShootingRequested}, " +
                      $"isShootAnimationActive: {isShootAnimationActive}, " +
                      $"쿨다운: {shootAnimationCooldown:F2}");
        }

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

        // 상태에 따라 목표 가중치 설정
        if (starterAssetsInputs.aim)
        {
            // 에임 카메라 및 설정 활성화
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensivitity(aimSensitivity);
            thirdPersonController.SetRotateOnMove(false);

            // 에임 레이어 목표 설정
            aimLayerTarget = 1f;

            // 에임 방향으로 캐릭터 회전
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20.0f);

            if(animator.GetInteger(ShootParam) == 1)
            {
                animator.SetInteger(ShootParam, 0);
            }
          
            if (starterAssetsInputs.shoot)
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

               

                starterAssetsInputs.shoot = false;
            }
        }
        else
        {
            // 에임 비활성화 상태
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensivitity(normalSensitivity);
            thirdPersonController.SetRotateOnMove(true);

            // 에임 레이어 목표 0으로 설정
            aimLayerTarget = 0f;
        }

        // 에임 레이어 가중치 부드럽게 업데이트
        animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), aimLayerTarget, Time.deltaTime * 10f));

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

                // ThirdPersonController 스크립트의 이동 제어 복원
                thirdPersonController.SetCanMove(true);
            }
        }
        else
        {
            // 회피 시작 처리 - 쿨다운이 끝났고 현재 에임이 아닐때만
            if (!starterAssetsInputs.aim && dodgeCooldownTimer <= 0)
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

        // ThirdPersonController 스크립트의 이동 제어 일시 중지 (선택적)
        thirdPersonController.SetCanMove(false);

        // 회피 중 카메라 효과 (선택적)
        // cinemachineTransposer.m_FollowOffset.y = Mathf.Lerp(cinemachineTransposer.m_FollowOffset.y, dodgeCameraOffset, Time.deltaTime * 10f);
    }
}