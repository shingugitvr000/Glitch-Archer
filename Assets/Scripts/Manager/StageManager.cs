

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageManager : MonoBehaviour
{
    [Header("스테이지 데이터")]
    public StageData currentStageData;              // 현재 스테이지 SO

    [Header("스폰 포인트")]
    public Transform playerSpawnPoint;
    public Transform[] enemySpawnPoints;

    [Header("UI 참조 (선택사항)")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI stageNameText;

    [Header("진행 상황 (읽기 전용)")]
    [SerializeField] private bool stageStarted = false;
    [SerializeField] private bool stageCompleted = false;
    [SerializeField] private int currentProgress = 0;
    [SerializeField] private float stageTimer = 0f;

    // 프로퍼티로 외부 접근 제공
    public bool StageStarted => stageStarted;
    public bool StageCompleted => stageCompleted;
    public int CurrentProgress => currentProgress;
    public float StageTimer => stageTimer;

    // 이벤트
    public static event Action OnStageStart;
    public static event Action OnStageComplete;
    public static event Action OnStageFailed;
    public static event Action<int, int> OnProgressChanged; // 현재 진행도, 목표

    private static StageManager instance;
    public static StageManager Instance => instance;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        if (currentStageData == null)
        {
            Debug.LogError("StageData가 설정되지 않았습니다!");
            return;
        }

        InitializeStage();
    }

    private void Update()
    {
        if (!stageStarted || stageCompleted) return;

        // 시간 업데이트
        stageTimer += Time.deltaTime;
        UpdateUI();

        // 시간 제한 체크
        if (stageTimer >= currentStageData.timeLimit)
        {
            if (currentStageData.objective != StageObjective.Survival)
            {
                FailStage();
            }
            else
            {
                // 생존 모드는 시간 다 버티면 성공
                CompleteStage();
            }
        }

        // 테스트 키
        if (Input.GetKeyDown(KeyCode.R))
        {
            CompleteStage();
        }
    }

    private void InitializeStage()
    {
        // 플레이어 이동
        MovePlayerToSpawn();

        // UI 초기화
        UpdateUI();
        if (stageNameText != null)
            stageNameText.text = currentStageData.stageName;

        // 적 수 자동 계산 (SO에 설정이 없는 경우)
        if (currentStageData.targetCount == 0 &&
            currentStageData.objective == StageObjective.KillAllEnemies)
        {
            int totalEnemies = 0;
            foreach (var spawn in currentStageData.enemySpawns)
            {
                totalEnemies += spawn.spawnCount;
            }
            // SO를 직접 수정하지 않고 런타임에만 사용
            Debug.Log($"자동 계산된 적 수: {totalEnemies}");
        }

        // 3초 후 시작
        StartCoroutine(StartStageAfterDelay(3f));
    }

    private void UpdateUI()
    {
        if (timeText != null)
        {
            float remainingTime = Mathf.Max(0, currentStageData.timeLimit - stageTimer);
            timeText.text = $"시간: {remainingTime:F0}초";
        }

        if (objectiveText != null)
        {
            string objectiveString = GetObjectiveString();
            objectiveText.text = objectiveString;
        }
    }

    private string GetObjectiveString()
    {
        switch (currentStageData.objective)
        {
            case StageObjective.KillAllEnemies:
                return $"적 처치: {currentProgress}/{GetTargetCount()}";
            case StageObjective.KillSpecificCount:
                return $"적 처치: {currentProgress}/{currentStageData.targetCount}";
            case StageObjective.Survival:
                return "생존하세요!";
            case StageObjective.CollectItems:
                return $"아이템 수집: {currentProgress}/{currentStageData.targetCount}";
            case StageObjective.ReachGoal:
                return "목적지에 도달하세요!";
            case StageObjective.DefendBase:
                return "기지를 방어하세요!";
            default:
                return $"진행도: {currentProgress}/{currentStageData.targetCount}";
        }
    }

    private int GetTargetCount()
    {
        if (currentStageData.objective == StageObjective.KillAllEnemies)
        {
            // SO에서 설정한 값이 있으면 사용, 없으면 스폰될 적 수 계산
            if (currentStageData.targetCount > 0)
                return currentStageData.targetCount;

            int totalEnemies = 0;
            foreach (var spawn in currentStageData.enemySpawns)
            {
                totalEnemies += spawn.spawnCount;
            }
            return totalEnemies;
        }
        return currentStageData.targetCount;
    }

    private IEnumerator StartStageAfterDelay(float delay)
    {
        for (int i = 3; i >= 1; i--)
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.ShowPopup("스테이지 시작",
                    $"{currentStageData.stageName}\n{i}초 후 시작됩니다");
            }
            yield return new WaitForSeconds(1f);
        }

        StartStage();
    }

    private void StartStage()
    {
        stageStarted = true;
        stageTimer = 0f;
        currentProgress = 0;

        // 적 스폰
        SpawnEnemies();

        OnStageStart?.Invoke();

        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup("스테이지 시작!",
                currentStageData.stageDescription);
        }

        Debug.Log($"{currentStageData.stageName} 시작!");
    }

    private void SpawnEnemies()
    {
        foreach (var spawnInfo in currentStageData.enemySpawns)
        {
            StartCoroutine(SpawnEnemyWithDelay(spawnInfo));
        }
    }

    private IEnumerator SpawnEnemyWithDelay(EnemySpawnInfo spawnInfo)
    {
        if (spawnInfo.enemyPrefab == null)
        {
            Debug.LogWarning("적 프리팹이 설정되지 않았습니다!");
            yield break;
        }

        yield return new WaitForSeconds(spawnInfo.spawnDelay);

        for (int i = 0; i < spawnInfo.spawnCount; i++)
        {
            Vector3 spawnPos = GetSpawnPosition(spawnInfo);
            GameObject enemy = Instantiate(spawnInfo.enemyPrefab, spawnPos, Quaternion.identity);

            // 난이도 적용
            ApplyDifficultyToEnemy(enemy);

            spawnedEnemies.Add(enemy);

            // 연속 스폰 시 약간의 딜레이
            if (i < spawnInfo.spawnCount - 1)
                yield return new WaitForSeconds(0.5f);
        }
    }

    private Vector3 GetSpawnPosition(EnemySpawnInfo spawnInfo)
    {
        if (spawnInfo.useSpawnPoint && enemySpawnPoints != null && enemySpawnPoints.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, enemySpawnPoints.Length);
            return enemySpawnPoints[randomIndex].position;
        }
        return spawnInfo.spawnPosition;
    }

    private void ApplyDifficultyToEnemy(GameObject enemy)
    {
        var enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.maxHealth *= currentStageData.enemyHealthMultiplier;
            enemyController.currentHealth = enemyController.maxHealth;
            enemyController.damage *= currentStageData.enemyDamageMultiplier;
        }
    }

    private void MovePlayerToSpawn()
    {
        if (playerSpawnPoint != null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = playerSpawnPoint.position;
                player.transform.rotation = playerSpawnPoint.rotation;
            }
        }
    }

    // ========================================
    // 공개 메서드들 - 다른 시스템에서 호출
    // ========================================

    public void RegisterProgress()
    {
        if (!stageStarted || stageCompleted) return;

        currentProgress++;
        OnProgressChanged?.Invoke(currentProgress, GetTargetCount());

        Debug.Log($"진행도: {currentProgress}/{GetTargetCount()}");

        CheckObjectiveComplete();
    }

    public void RegisterEnemyKill()
    {
        if (currentStageData.objective == StageObjective.KillAllEnemies ||
            currentStageData.objective == StageObjective.KillSpecificCount)
        {
            RegisterProgress();
        }
    }

    public void RegisterItemCollected()
    {
        if (currentStageData.objective == StageObjective.CollectItems)
        {
            RegisterProgress();
        }
    }

    public void RegisterGoalReached()
    {
        if (currentStageData.objective == StageObjective.ReachGoal)
        {
            CompleteStage();
        }
    }

    private void CheckObjectiveComplete()
    {
        bool isComplete = false;

        switch (currentStageData.objective)
        {
            case StageObjective.KillAllEnemies:
            case StageObjective.KillSpecificCount:
            case StageObjective.CollectItems:
                isComplete = currentProgress >= GetTargetCount();
                break;
            case StageObjective.Survival:
                isComplete = stageTimer >= currentStageData.timeLimit;
                break;
            case StageObjective.ReachGoal:
            case StageObjective.DefendBase:
                // 별도 메서드로 완료 처리
                break;
        }

        if (isComplete)
        {
            CompleteStage();
        }
    }

    public void CompleteStage()
    {
        if (stageCompleted) return;

        stageCompleted = true;
        OnStageComplete?.Invoke();

        // 시간 보너스 계산
        bool hasTimeBonus = currentStageData.hasTimeBonus &&
                           stageTimer < currentStageData.timeBonusThreshold;

        string bonusText = hasTimeBonus ? "\n⭐ 시간 보너스!" : "";

        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup(
                "스테이지 완료!",
                $"{currentStageData.stageName} 클리어!\n" +
                $"완료 시간: {stageTimer:F1}초{bonusText}",
                () => LoadNextStage()
            );
        }

        Debug.Log($"{currentStageData.stageName} 완료!");
    }

    public void FailStage()
    {
        if (stageCompleted) return;

        stageCompleted = true;
        OnStageFailed?.Invoke();

        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowConfirmPopup(
                "스테이지 실패",
                "목표를 달성하지 못했습니다.\n다시 시도하시겠습니까?",
                () => RestartStage(),
                () => ExitStage()
            );
        }

        Debug.Log($"{currentStageData.stageName} 실패!");
    }

    private void LoadNextStage()
    {
        Debug.Log("다음 스테이지로 이동");
        // 여기에 다음 스테이지 로드 로직 구현
        // SceneManager.LoadScene("NextStage");
    }

    private void RestartStage()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void ExitStage()
    {
        Debug.Log("메인 메뉴로 이동");
        // SceneManager.LoadScene("MainMenu");
    }

    // ========================================
    // 디버그 및 기타
    // ========================================

    private void OnGUI()
    {
        if (!Application.isPlaying || currentStageData == null) return;

        GUILayout.BeginArea(new Rect(Screen.width - 300, 10, 290, 220));
        GUILayout.Label($"=== {currentStageData.stageName} ===");
        GUILayout.Label($"목표: {currentStageData.objective}");

        if (stageStarted && !stageCompleted)
        {
            GUILayout.Label($"남은 시간: {(currentStageData.timeLimit - stageTimer):F1}초");
            GUILayout.Label(GetObjectiveString());
            GUILayout.Label($"난이도: {currentStageData.difficultyLevel}");

            if (currentStageData.hasTimeBonus)
            {
                float bonusTime = currentStageData.timeBonusThreshold - stageTimer;
                if (bonusTime > 0)
                    GUILayout.Label($"⭐ 보너스까지: {bonusTime:F1}초");
            }
        }
        else if (stageCompleted)
        {
            GUILayout.Label("스테이지 완료!");
        }
        else
        {
            GUILayout.Label("스테이지 준비 중...");
        }

        GUILayout.Space(10);
        GUILayout.Label("테스트 키:");
        GUILayout.Label("R - 강제 완료");
        GUILayout.EndArea();
    }

    // 에디터에서 스테이지 정보 확인용
#if UNITY_EDITOR
    [ContextMenu("스테이지 정보 출력")]
    private void PrintStageInfo()
    {
        if (currentStageData == null)
        {
            Debug.Log("StageData가 설정되지 않음");
            return;
        }

        Debug.Log($"=== {currentStageData.stageName} ===");
        Debug.Log($"목표: {currentStageData.objective}");
        Debug.Log($"목표 수: {currentStageData.targetCount}");
        Debug.Log($"제한 시간: {currentStageData.timeLimit}초");
        Debug.Log($"스폰할 적 수: {currentStageData.enemySpawns.Count}종류");
    }
#endif
}
