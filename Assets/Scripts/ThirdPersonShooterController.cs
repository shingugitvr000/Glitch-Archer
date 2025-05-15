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





    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;

    // 레이어 가중치 목표값 저장 변수
    private float aimLayerTarget = 0f;
    private float shootLayerTarget = 0f;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponentInChildren<Animator>();
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

            // 발사 처리
            if (starterAssetsInputs.shoot)
            {
                shootLayerTarget = 1f;
                Vector3 aimDir = (mouseWorldPosition - spawnArrowPosition.position).normalized;
                Instantiate(arrowProjectile, spawnArrowPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
                starterAssetsInputs.shoot = false;
            }
            else
            {
                shootLayerTarget = 0f;
            }
        }
        else
        {
            // 에임 비활성화 상태
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensivitity(normalSensitivity);
            thirdPersonController.SetRotateOnMove(true);

            // 모든 레이어 목표 0으로 설정
            aimLayerTarget = 0f;
            shootLayerTarget = 0f;
        }

        // 레이어 가중치 부드럽게 업데이트
        animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), aimLayerTarget, Time.deltaTime * 10f));
        animator.SetLayerWeight(2, Mathf.Lerp(animator.GetLayerWeight(2), shootLayerTarget, Time.deltaTime * 10f));


    }
}
