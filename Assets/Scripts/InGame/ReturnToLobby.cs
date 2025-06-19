// ReturnToLobby.cs - 던전에서 로비로 돌아가는 스크립트
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToLobby : MonoBehaviour
{
    [Header("씬 설정")]
    public string lobbySceneName = "LobbyScene"; // 로비 씬 이름

    [Header("디버그")]
    public bool showDebugInfo = true;

    void Update()
    {
        // F10 키로 로비 돌아가기
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ReturnToLobbyScene();
        }

        // ESC 키로도 가능하게 (선택사항)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowReturnConfirmation();
        }
    }

    void ReturnToLobbyScene()
    {
        if (showDebugInfo)
        {
            Debug.Log("F10 키 - 로비로 돌아갑니다...");
        }

        // 커서 잠금 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 시간 스케일 정상화 (일시정지되어 있을 수 있음)
        Time.timeScale = 1f;

        // 로비 씬 로드
        SceneManager.LoadScene(lobbySceneName);
    }

    void ShowReturnConfirmation()
    {
        // PopupManager가 있으면 확인 팝업 표시
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowConfirmPopup(
                "던전 나가기",
                "정말로 던전을 나가시겠습니까?\n진행 상황이 저장되지 않습니다.",
                () => ReturnToLobbyScene(),  // 확인
                () => Debug.Log("던전 계속")  // 취소
            );
        }
        else
        {
            // PopupManager가 없으면 바로 로비로
            ReturnToLobbyScene();
        }
    }

    // 다른 스크립트에서 호출할 수 있는 공개 메서드
    public void GoToLobby()
    {
        ReturnToLobbyScene();
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        // 화면 우상단에 조작법 표시
        GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 100));
        GUILayout.Label("=== 던전 조작법 ===");
        GUILayout.Label("F1 - 스킬 상태 확인");
        GUILayout.Label("F2 - 저장된 스킬 데이터");
        GUILayout.Label("F10 - 로비로 돌아가기");
        GUILayout.Label("ESC - 나가기 확인");
        GUILayout.EndArea();
    }
}