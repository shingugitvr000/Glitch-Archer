// DungeonSkillApplier.cs - 개선된 버전
using UnityEngine;

public class DungeonSkillApplier : MonoBehaviour
{
    [Header("자동 연결될 컴포넌트들")]
    public PlayerStats playerStats;

    [Header("디버그")]
    public bool showDebugInfo = true;

    [Header("스킬 적용 방식")]
    public bool useSimpleApply = true; // true: 간단한 방식, false: 스킬 매니저 필요

    void Start()
    {
        // 플레이어 찾기
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        if (playerStats == null)
        {
            Debug.LogError("PlayerStats를 찾을 수 없습니다! 플레이어에 PlayerStats 컴포넌트가 있는지 확인하세요.");
            return;
        }

        // 던전에서 스킬 효과 적용
        ApplySkillsInDungeon();
    }

    void ApplySkillsInDungeon()
    {
        // SkillPersistenceManager 찾기
        if (SkillPersistenceManager.Instance == null)
        {
            Debug.LogWarning("SkillPersistenceManager가 없습니다. 스킬 효과가 적용되지 않습니다.");
            return;
        }

        if (showDebugInfo)
        {
            Debug.Log("=== 던전에서 스킬 효과 적용 시작 ===");
            Debug.Log($"적용 전 - 크리티컬: {playerStats.criticalChance * 100f}%, 관통: {playerStats.pierceCount}");
        }

        if (useSimpleApply)
        {
            // ★ 간단한 방식: SkillPersistenceManager에서 직접 적용
            ApplySkillsDirectly();
        }
        else
        {
            // 복잡한 방식: 스킬 매니저를 통해 적용
            SkillPersistenceManager.Instance.ApplySkillsToPlayer(playerStats, null);
        }

        if (showDebugInfo)
        {
            Debug.Log($"적용 후 - 크리티컬: {playerStats.criticalChance * 100f}%, 관통: {playerStats.pierceCount}");
            Debug.Log($"폭발 화살: {playerStats.hasExplosiveArrow}, 유도: {playerStats.hasGuidedAfterPierce}");
            Debug.Log("=== 던전 스킬 효과 적용 완료 ===");
        }
    }

    // ★ 간단한 방식: 저장된 스킬 데이터로 직접 적용
    void ApplySkillsDirectly()
    {
        var saveData = SkillPersistenceManager.Instance.currentSkillData;

        if (saveData.skillLevels.Count == 0)
        {
            Debug.Log("저장된 스킬이 없습니다.");
            return;
        }

        // 먼저 스탯 리셋
        ResetPlayerStatsToDefault();

        // 저장된 스킬 효과들을 직접 적용
        foreach (var skillData in saveData.skillLevels)
        {
            int skillId = skillData.Key;
            int level = skillData.Value;

            ApplySkillEffectDirect(skillId, level);
        }

        Debug.Log($"✅ 간단한 방식으로 {saveData.skillLevels.Count}개 스킬 적용 완료");
    }

    // ★ 스킬 ID와 레벨로 직접 효과 적용
    void ApplySkillEffectDirect(int skillId, int level)
    {
        // 스킬 ID별로 효과 적용 (ArrowSkillTreeManager의 스킬 정의와 동일)
        switch (skillId)
        {
            case 1: // 힘 스킬
                float attackBonus = 5f + (level - 1) * 2f; // 5 + 2*레벨
                playerStats.SetSkillAttackPowerBonus(playerStats.SkillAttackPowerBonus + attackBonus);
                Debug.Log($"힘 Lv.{level}: 공격력 +{attackBonus}");
                break;

            case 2: // 민첩 스킬
                float critBonus = 2f + (level - 1) * 1f; // 2% + 1%*레벨
                playerStats.criticalChance += critBonus / 100f;
                Debug.Log($"민첩 Lv.{level}: 크리티컬 +{critBonus}%");
                break;

            case 3: // 집중 스킬
                float critDamageBonus = 10f + (level - 1) * 5f; // 10% + 5%*레벨
                playerStats.criticalMultiplier += critDamageBonus / 100f;
                Debug.Log($"집중 Lv.{level}: 크리티컬 데미지 +{critDamageBonus}%");
                break;

            case 4: // 체력 스킬
                float healthBonus = 50f + (level - 1) * 25f; // 50 + 25*레벨
                playerStats.SetSkillHealthBonus(playerStats.SkillHealthBonus + healthBonus);
                Debug.Log($"체력 Lv.{level}: 체력 +{healthBonus}");
                break;

            case 11: // 관통 스킬
                int pierceBonus = 1 + (level - 1) * 1; // 1 + 1*레벨
                playerStats.pierceCount += pierceBonus;
                Debug.Log($"관통 Lv.{level}: 관통 횟수 +{pierceBonus}");
                break;

            case 13: // 폭발 스킬
                playerStats.hasExplosiveArrow = true;
                Debug.Log($"폭발 Lv.{level}: 폭발 화살 활성화");
                break;

            case 31: // 관통 마스터
                playerStats.pierceCount += 3;
                playerStats.hasGuidedAfterPierce = true;
                Debug.Log($"관통 마스터 Lv.{level}: 관통 +3, 유도 활성화");
                break;

            case 32: // 저격 마스터
                playerStats.criticalChance += 0.3f; // 30% 크리티컬
                Debug.Log($"저격 마스터 Lv.{level}: 크리티컬 +30%");
                break;

            case 33: // 폭발 마스터
                playerStats.hasExplosiveArrow = true;
                // ★ 기본값에서 마스터 효과 적용 (기존 값에 곱하지 말고 새로 설정)
                playerStats.explosiveRadius = 5f * 1.5f; // 7.5f
                playerStats.explosiveDamage = 0.7f * 1.3f; // 0.91f
                Debug.Log($"폭발 마스터 Lv.{level}: 폭발 반경 {playerStats.explosiveRadius}, 데미지 {playerStats.explosiveDamage * 100f}%");
                break;

            default:
                Debug.LogWarning($"알 수 없는 스킬 ID: {skillId}");
                break;
        }
    }

    // 플레이어 스탯 기본값으로 리셋
    void ResetPlayerStatsToDefault()
    {
        playerStats.ResetAllSkillBonuses();

        // 화살 특성 초기화
        playerStats.criticalChance = 0.15f;
        playerStats.criticalMultiplier = 2.0f;
        playerStats.pierceCount = 0;
        playerStats.hasExplosiveArrow = false;
        playerStats.hasGuidedAfterPierce = false;
        playerStats.explosiveRadius = 5f;
        playerStats.explosiveDamage = 0.7f;

        Debug.Log("플레이어 스탯이 기본값으로 리셋되었습니다.");
    }

    // 던전 중에 스킬 상태 확인용
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && showDebugInfo)
        {
            ShowCurrentSkillStatus();
        }

        if (Input.GetKeyDown(KeyCode.F2) && showDebugInfo)
        {
            ShowSavedSkillData();
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            ReturnToLobby();
        }
    }

    void ShowCurrentSkillStatus()
    {
        if (playerStats == null) return;

        Debug.Log("=== 현재 던전에서의 스킬 상태 ===");
        Debug.Log($"크리티컬 확률: {playerStats.criticalChance * 100f}%");
        Debug.Log($"크리티컬 배수: {playerStats.criticalMultiplier}x");
        Debug.Log($"관통 횟수: {playerStats.pierceCount}");
        Debug.Log($"폭발 화살: {playerStats.hasExplosiveArrow}");
        Debug.Log($"유도 화살: {playerStats.hasGuidedAfterPierce}");
        Debug.Log($"공격력 보너스: +{playerStats.SkillAttackPowerBonus}");
        Debug.Log($"체력 보너스: +{playerStats.SkillHealthBonus}");
    }

    void ShowSavedSkillData()
    {
        if (SkillPersistenceManager.Instance == null) return;

        var saveData = SkillPersistenceManager.Instance.currentSkillData;
        Debug.Log("=== 저장된 스킬 데이터 ===");
        Debug.Log($"저장된 스킬 개수: {saveData.skillLevels.Count}");

        foreach (var skill in saveData.skillLevels)
        {
            Debug.Log($"스킬 ID {skill.Key}: 레벨 {skill.Value}");
        }
    }

    void ReturnToLobby()
    {
        Debug.Log("F10 키 - 로비로 돌아갑니다...");

        // ★ 던전에서 변경된 스킬 상태가 있다면 저장 (현재는 던전에서 스킬 변경 불가하므로 생략 가능)
        // SaveCurrentSkillState();

        // ★ CursorLockManager를 통해 안전하게 커서 해제
        var cursorManager = FindObjectOfType<CursorLockManager>();
        if (cursorManager != null)
        {
            cursorManager.UnlockCursor();
        }
        else
        {
            // CursorLockManager가 없으면 기존 방식
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 시간 스케일 정상화
        Time.timeScale = 1f;

        // 로비 씬 로드 (씬 이름은 실제 로비 씬 이름으로 변경)
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }
}