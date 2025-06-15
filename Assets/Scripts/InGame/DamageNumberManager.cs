using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    [Header("데미지 넘버 설정")]
    public GameObject damageNumberPrefab;  // 데미지 넘버 프리팹
    public Canvas damageCanvas;            // UI 캔버스

    private static DamageNumberManager instance;
    private Camera playerCamera;

    public static DamageNumberManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DamageNumberManager>();
                if (instance == null)
                {
                    // 자동으로 생성
                    GameObject obj = new GameObject("DamageNumberManager");
                    instance = obj.AddComponent<DamageNumberManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // 메인 카메라 찾기
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();

        // 캔버스가 없으면 자동 생성
        if (damageCanvas == null)
        {
            CreateDamageCanvas();
        }
    }

    // 캔버스 자동 생성
    private void CreateDamageCanvas()
    {
        GameObject canvasObj = new GameObject("DamageCanvas");
        damageCanvas = canvasObj.AddComponent<Canvas>();
        damageCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        damageCanvas.sortingOrder = 100; // 다른 UI보다 위에 표시

        // CanvasScaler 추가
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // GraphicRaycaster 추가
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
    }

    // 데미지 넘버 표시 (월드 위치에서)
    public void ShowDamageNumber(Vector3 worldPosition, float damage, bool isCritical = false, bool isExplosion = false)
    {
        if (damageNumberPrefab == null)
        {
            Debug.LogError("DamageNumberManager: damageNumberPrefab이 null입니다!");
            return;
        }

        if (damageCanvas == null)
        {
            Debug.LogError("DamageNumberManager: damageCanvas가 null입니다!");
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogError("DamageNumberManager: playerCamera가 null입니다!");
            return;
        }

        // 월드 위치를 스크린 위치로 변환
        Vector3 screenPosition = playerCamera.WorldToScreenPoint(worldPosition);

        // 카메라 뒤에 있으면 표시하지 않음
        if (screenPosition.z < 0)
        {
            return;
        }

        // 데미지 넘버 생성
        GameObject damageObj = Instantiate(damageNumberPrefab, damageCanvas.transform);

        RectTransform rectTransform = damageObj.GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            // 스크린 위치로 설정
            rectTransform.position = screenPosition;
        }
        else
        {
            Debug.LogError("RectTransform을 찾을 수 없습니다!");
        }

        // 데미지 설정
        DamageNumber damageNumber = damageObj.GetComponent<DamageNumber>();
        if (damageNumber != null)
        {
            damageNumber.SetDamage(damage, isCritical, isExplosion);
        }
        else
        {
            Debug.LogError("DamageNumber 컴포넌트를 찾을 수 없습니다!");
        }
    }

    // 간편 호출 함수들
    public static void ShowDamage(Vector3 worldPosition, float damage)
    {
        Instance.ShowDamageNumber(worldPosition, damage, false, false);
    }

    public static void ShowCriticalDamage(Vector3 worldPosition, float damage)
    {
        Instance.ShowDamageNumber(worldPosition, damage, true, false);
    }

    public static void ShowExplosionDamage(Vector3 worldPosition, float damage)
    {
        Instance.ShowDamageNumber(worldPosition, damage, false, true);
    }

    public static void ShowCriticalExplosionDamage(Vector3 worldPosition, float damage)
    {
        Instance.ShowDamageNumber(worldPosition, damage, true, true);
    }
}