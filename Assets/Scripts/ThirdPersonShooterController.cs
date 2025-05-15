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

    // ���̾� ����ġ ��ǥ�� ���� ����
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

        // ���¿� ���� ��ǥ ����ġ ����
        if (starterAssetsInputs.aim)
        {
            // ���� ī�޶� �� ���� Ȱ��ȭ
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensivitity(aimSensitivity);
            thirdPersonController.SetRotateOnMove(false);

            // ���� ���̾� ��ǥ ����
            aimLayerTarget = 1f;

            // ���� �������� ĳ���� ȸ��
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20.0f);

            // �߻� ó��
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
            // ���� ��Ȱ��ȭ ����
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensivitity(normalSensitivity);
            thirdPersonController.SetRotateOnMove(true);

            // ��� ���̾� ��ǥ 0���� ����
            aimLayerTarget = 0f;
            shootLayerTarget = 0f;
        }

        // ���̾� ����ġ �ε巴�� ������Ʈ
        animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), aimLayerTarget, Time.deltaTime * 10f));
        animator.SetLayerWeight(2, Mathf.Lerp(animator.GetLayerWeight(2), shootLayerTarget, Time.deltaTime * 10f));


    }
}
