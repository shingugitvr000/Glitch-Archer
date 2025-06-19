using UnityEngine;

public class CursorLockManager : MonoBehaviour
{
    [Header("설정")]
    public bool lockCursorOnStart = true;
    public bool allowTemporaryUnlock = true; // UI 조작 시 임시 해제 허용
    public float relockDelay = 0.5f; // UI 조작 후 다시 락하는 딜레이

    [Header("디버그")]
    public bool showDebugInfo = true;

    private bool isUIActive = false;
    private bool shouldRelock = false;
    private float relockTimer = 0f;

    void Start()
    {
        if (lockCursorOnStart)
        {
            LockCursor();
        }
    }

    void Update()
    {
        // UI가 비활성화되고 일정 시간 후 다시 락
        if (shouldRelock && !isUIActive)
        {
            relockTimer -= Time.deltaTime;
            if (relockTimer <= 0f)
            {
                LockCursor();
                shouldRelock = false;
            }
        }

        // ESC 키로 마우스 락/해제 토글 (디버그용)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }

        // 게임 중에 마우스가 실수로 해제되었을 때 자동 복구
        if (!isUIActive && !shouldRelock && Cursor.lockState != CursorLockMode.Locked)
        {
            if (showDebugInfo)
                Debug.Log("마우스 락이 의도치 않게 해제됨 - 자동 복구");
            LockCursor();
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isUIActive = false;
        shouldRelock = false;

        if (showDebugInfo)
            Debug.Log("🔒 마우스 락 활성화");
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isUIActive = true;

        if (showDebugInfo)
            Debug.Log("🔓 마우스 락 해제");
    }

    public void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    // UI가 열릴 때 호출 (팝업, 메뉴 등)
    public void OnUIOpened()
    {
        if (allowTemporaryUnlock)
        {
            UnlockCursor();
        }
    }

    // UI가 닫힐 때 호출
    public void OnUIClosed()
    {
        if (allowTemporaryUnlock)
        {
            isUIActive = false;
            shouldRelock = true;
            relockTimer = relockDelay;

            if (showDebugInfo)
                Debug.Log($"UI 닫힘 - {relockDelay}초 후 마우스 락 예정");
        }
    }

    // 즉시 다시 락 (던전 복귀, 게임 재개 시)
    public void ForceRelock()
    {
        shouldRelock = false;
        LockCursor();
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        // 화면 우하단에 마우스 락 상태 표시
        GUILayout.BeginArea(new Rect(Screen.width - 200, Screen.height - 80, 190, 70));
        GUILayout.Label("=== 마우스 상태 ===");
        GUILayout.Label($"락 상태: {Cursor.lockState}");
        GUILayout.Label($"커서 보임: {Cursor.visible}");
        GUILayout.Label($"UI 활성: {isUIActive}");
        if (shouldRelock)
            GUILayout.Label($"재락 예정: {relockTimer:F1}초");
        GUILayout.EndArea();
    }
}