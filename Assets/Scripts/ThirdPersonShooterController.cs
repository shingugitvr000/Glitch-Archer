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
    [SerializeField] private float arrowSpawnDelay = 0.2f; // ȭ�� �߻� ���� �ð�

    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;
    // ���̾� ����ġ ��ǥ�� ���� ����
    private float aimLayerTarget = 0f;
    private float arrowSpawnTimer = 0f; // ȭ�� �߻� Ÿ�̸�
    private bool isArrowSpawnPending = false; // ȭ�� �߻� ��� ������ Ȯ��
    private Vector3 lastMouseWorldPosition; // ������ ���콺 ��ġ ����

    // �ִϸ����� �Ķ���� �ؽ� (���� ����ȭ)
    private string ShootParamName = "Shoot"; // ���� �Ķ���� �̸�
    private int ShootParam;

    // �߻� ���� ����
    private bool isShootingRequested = false; // �߻� ��û ����
    private bool isShootAnimationActive = false; // �߻� �ִϸ��̼� Ȱ��ȭ ����
    private float shootAnimationCooldown = 0f; // ���� �߻� ���� ��ٿ�

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeCooldown = 0.7f; // ȸ�� ��ٿ� �ð�
    [SerializeField] private float dodgeDuration = 0.5f; // ȸ�� ���� �ð�
    [SerializeField] private float dodgeSpeed = 10f; // ȸ�ǽ� �̵� �ӵ�
    [SerializeField] private float dodgeSpeedMultiplier = 1.5f; // ȸ�� �� �ӵ� ���� ����

    // ȸ�� ���� ����
    [SerializeField] private bool isDodging = false;
    [SerializeField] private float dodgeTimer = 0f;
    [SerializeField] public float dodgeCooldownTimer = 0f;
    [SerializeField] private Vector3 dodgeDirection;

    // �ִϸ����� �Ķ���� �ؽ�
    private int DodgeForwardParam;
    private int DodgeBackwardParam;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponentInChildren<Animator>();

        // �ִϸ����� �Ķ���� �ؽ� ID ���
        ShootParam = Animator.StringToHash(ShootParamName);

        // ȸ�� �Ķ���� �ؽ� �ʱ�ȭ
        DodgeForwardParam = Animator.StringToHash("DodgeForward");
        DodgeBackwardParam = Animator.StringToHash("DodgeBackward");

        // ����� - �ִϸ����� �Ķ���� ��� Ȯ��
        AnimatorControllerParameter[] parameters = animator.parameters;
        Debug.Log("�ִϸ����� �Ķ���� ���:");
        foreach (var param in parameters)
        {
            Debug.Log($"�̸�: {param.name}, Ÿ��: {param.type}, �ؽ�: {Animator.StringToHash(param.name)}");
            if (param.name == ShootParamName)
            {
                Debug.Log($"Shoot �Ķ���� �߰�: �ؽ� ID = {ShootParam}");
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
            lastMouseWorldPosition = mouseWorldPosition; // ������ ���콺 ��ġ ����
        }

        // ��ٿ� Ÿ�̸� ������Ʈ
        if (shootAnimationCooldown > 0)
        {
            shootAnimationCooldown -= Time.deltaTime;
        }

        // ȭ�� �߻� Ÿ�̸� ������Ʈ
        if (isArrowSpawnPending)
        {
            arrowSpawnTimer -= Time.deltaTime;
            if (arrowSpawnTimer <= 0)
            {
                // ȭ�� �߻�
                Vector3 aimDir = (lastMouseWorldPosition - spawnArrowPosition.position).normalized;
                Instantiate(arrowProjectile, spawnArrowPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
                isArrowSpawnPending = false;
                Debug.Log("ȭ�� �߻� �Ϸ�");
            }
        }

        // ���� �ִϸ����� ���� �� Shoot �Ķ���� Ȯ��
        int currentShootValue = animator.GetInteger(ShootParam);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);

        // �ִϸ��̼� ���� �����
        if (Time.frameCount % 30 == 0) // 30�����Ӹ��� �α� ��� (�ܼ� ���� ����)
        {
            Debug.Log($"������: {Time.frameCount}, Shoot ��: {currentShootValue}, " +
                      $"�ִϸ��̼� ����: {stateInfo.shortNameHash}, " +
                      $"�ð�: {stateInfo.normalizedTime:F2}, " +
                      $"isShootingRequested: {isShootingRequested}, " +
                      $"isShootAnimationActive: {isShootAnimationActive}, " +
                      $"��ٿ�: {shootAnimationCooldown:F2}");
        }

        // �߻� �ִϸ��̼� ���� ����
        if (stateInfo.IsName("Attack_1Shoot_Loop"))
        {
            isShootAnimationActive = true;

            // �ִϸ��̼��� ���� ������ Shoot �Ķ���� ����
            if (stateInfo.normalizedTime >= 0.9f && currentShootValue != 0)
            {
                animator.SetInteger(ShootParam, 0);
                isShootingRequested = false;
                shootAnimationCooldown = 0.5f; // ���� �߻� ������ ���� ��ٿ� ����
                Debug.Log("�ִϸ��̼� �Ϸ� - Shoot �Ķ���� ����");
            }
        }
        else
        {
            isShootAnimationActive = false;
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

            if(animator.GetInteger(ShootParam) == 1)
            {
                animator.SetInteger(ShootParam, 0);
            }
          
            if (starterAssetsInputs.shoot)
            {
                // �߻� ��û �÷��� ����
                isShootingRequested = true;

                // Shoot �Ķ���� ���� ���� �α�
                Debug.Log($"�߻� ��û - ���� Shoot ��: {animator.GetInteger(ShootParam)}, �õ��մϴ�...");

                // Shoot �Ķ���͸� 1�� �����Ͽ� �ִϸ��̼� Ʈ����
                animator.SetInteger(ShootParam, 1);

                // ���� �� Ȯ��
                Debug.Log($"Shoot �Ķ���� ���� �Ϸ� - �� ��: {animator.GetInteger(ShootParam)}");

                // ȭ�� �߻� Ÿ�̸� ����
                isArrowSpawnPending = true;
                arrowSpawnTimer = arrowSpawnDelay;

               

                starterAssetsInputs.shoot = false;
            }
        }
        else
        {
            // ���� ��Ȱ��ȭ ����
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensivitity(normalSensitivity);
            thirdPersonController.SetRotateOnMove(true);

            // ���� ���̾� ��ǥ 0���� ����
            aimLayerTarget = 0f;
        }

        // ���� ���̾� ����ġ �ε巴�� ������Ʈ
        animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), aimLayerTarget, Time.deltaTime * 10f));

        // ȸ�� ��ٿ� Ÿ�̸� ������Ʈ
        if (dodgeCooldownTimer > 0)
        {
            dodgeCooldownTimer -= Time.deltaTime;
        }

        // ���� ȸ�� ������ Ȯ�� �� ó��
        if (isDodging)
        {
            dodgeTimer -= Time.deltaTime;

            // ȸ�� �̵� ó��
            transform.position += dodgeDirection * dodgeSpeed * Time.deltaTime;

            // ȸ�� ���� ó��
            if (dodgeTimer <= 0)
            {
                isDodging = false;

                // ThirdPersonController ��ũ��Ʈ�� �̵� ���� ����
                thirdPersonController.SetCanMove(true);
            }
        }
        else
        {
            // ȸ�� ���� ó�� - ��ٿ��� ������ ���� ������ �ƴҶ���
            if (!starterAssetsInputs.aim && dodgeCooldownTimer <= 0)
            {
                // ������ ȸ��
                if (starterAssetsInputs.dodgeForward)
                {
                    StartDodge(transform.forward);
                    // �ִϸ��̼� Ʈ���� (Shoot�� ���� ������� ��� ����)
                    animator.SetTrigger(DodgeForwardParam);
                    
                    starterAssetsInputs.dodgeForward = false;
                }
                // �ڷ� ȸ��
                else if (starterAssetsInputs.dodgeBackward)
                {
                    StartDodge(-transform.forward);
                    // �ִϸ��̼� Ʈ����
                    animator.SetTrigger(DodgeBackwardParam);
                  
                    starterAssetsInputs.dodgeBackward = false;
                }
            }
        }
    }

    // ȸ�� ���� �޼���
    private void StartDodge(Vector3 direction)
    {
        isDodging = true;
        dodgeTimer = dodgeDuration;
        dodgeCooldownTimer = dodgeCooldown;
        dodgeDirection = direction.normalized;

        // ThirdPersonController ��ũ��Ʈ�� �̵� ���� �Ͻ� ���� (������)
        thirdPersonController.SetCanMove(false);

        // ȸ�� �� ī�޶� ȿ�� (������)
        // cinemachineTransposer.m_FollowOffset.y = Mathf.Lerp(cinemachineTransposer.m_FollowOffset.y, dodgeCameraOffset, Time.deltaTime * 10f);
    }
}