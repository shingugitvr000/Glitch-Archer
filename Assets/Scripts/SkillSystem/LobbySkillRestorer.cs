// LobbySkillRestorer.cs - 로비에서 스킬 상태 복원하는 스크립트
using UnityEngine;

public class LobbySkillRestorer : MonoBehaviour
{
    [Header("자동 연결")]
    public ArrowSkillTreeManager skillManager;
    public LobbySkillTreeUI skillTreeUI;

    [Header("디버그")]
    public bool showDebugInfo = true;

    void Start()
    {
        // 컴포넌트 자동 찾기
        if (skillManager == null)
            skillManager = FindObjectOfType<ArrowSkillTreeManager>();

        if (skillTreeUI == null)
            skillTreeUI = FindObjectOfType<LobbySkillTreeUI>();

        // 스킬 상태 복원
        RestoreSkillsFromPersistence();
    }

    void RestoreSkillsFromPersistence()
    {
        // SkillPersistenceManager 확인
        if (SkillPersistenceManager.Instance == null)
        {
            if (showDebugInfo)
                Debug.Log("SkillPersistenceManager가 없습니다. 새로운 게임으로 시작합니다.");
            return;
        }

        if (skillManager == null)
        {
            Debug.LogError("ArrowSkillTreeManager를 찾을 수 없습니다!");
            return;
        }

        var saveData = SkillPersistenceManager.Instance.currentSkillData;

        if (saveData.skillLevels.Count == 0)
        {
            if (showDebugInfo)
                Debug.Log("저장된 스킬이 없습니다.");
            return;
        }

        if (showDebugInfo)
        {
            Debug.Log("=== 로비에서 스킬 상태 복원 시작 ===");
            Debug.Log($"복원할 스킬 개수: {saveData.skillLevels.Count}");
        }

        // ★ 스킬 매니저의 통합 복원 메서드 사용
        skillManager.RestoreSkillState(saveData.availablePoints, saveData.playerLevel, saveData.skillLevels);

        if (showDebugInfo)
        {
            Debug.Log("=== 로비 스킬 상태 복원 완료 ===");
            Debug.Log($"사용 가능한 포인트: {skillManager.availablePoints}");
            Debug.Log($"사용된 포인트: {skillManager.GetTotalPointsUsed()}");
        }
    }

    // 테스트용 - F9로 현재 스킬 상태 확인
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9) && showDebugInfo)
        {
            ShowCurrentSkillState();
        }
    }

    void ShowCurrentSkillState()
    {
        if (skillManager == null) return;

        Debug.Log("=== 현재 로비 스킬 상태 ===");
        Debug.Log($"사용 가능한 포인트: {skillManager.availablePoints}");
        Debug.Log($"사용된 포인트: {skillManager.GetTotalPointsUsed()}");

        foreach (var skill in skillManager.allSkills)
        {
            if (skill.IsLearned)
            {
                Debug.Log($"{skill.skillName}: Lv.{skill.currentLevel}");
            }
        }

        var playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats != null)
        {
            Debug.Log($"공격력 보너스: +{playerStats.SkillAttackPowerBonus}");
            Debug.Log($"크리티컬: {playerStats.criticalChance * 100f}%");
            Debug.Log($"관통: {playerStats.pierceCount}회");
        }
    }
}