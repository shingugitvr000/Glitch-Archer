// SkillPersistenceManager.cs - ApplySkillEffect 메서드 수정
using UnityEngine;
using System.Collections.Generic;

public class SkillPersistenceManager : MonoBehaviour
{
    public static SkillPersistenceManager Instance;

    [Header("저장된 스킬 데이터")]
    public SkillSaveData currentSkillData = new SkillSaveData();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("SkillPersistenceManager 초기화");
    }

    public void SaveSkillData(ArrowSkillTreeManager skillManager)
    {
        if (skillManager == null) return;

        currentSkillData.availablePoints = skillManager.availablePoints;
        currentSkillData.playerLevel = skillManager.playerLevel;
        currentSkillData.skillLevels.Clear();

        foreach (var skill in skillManager.allSkills)
        {
            if (skill.currentLevel > 0)
            {
                currentSkillData.skillLevels[skill.skillId] = skill.currentLevel;
            }
        }

        Debug.Log($"✅ 스킬 데이터 저장 완료 - 학습한 스킬: {currentSkillData.skillLevels.Count}개");
    }

    public void ApplySkillsToPlayer(PlayerStats playerStats, ArrowSkillTreeManager skillManager = null)
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats가 null입니다!");
            return;
        }

        if (skillManager == null)
        {
            skillManager = FindObjectOfType<ArrowSkillTreeManager>();
            if (skillManager == null)
            {
                GameObject tempObj = new GameObject("TempSkillManager");
                skillManager = tempObj.AddComponent<ArrowSkillTreeManager>();
            }
        }

        Debug.Log($"던전에서 스킬 효과 적용 시작... 저장된 스킬: {currentSkillData.skillLevels.Count}개");

        RestoreSkillLevels(skillManager);
        ResetPlayerStatsToDefault(playerStats);
        ApplyAllSkillEffects(playerStats, skillManager);

        Debug.Log("✅ 던전에서 스킬 효과 적용 완료!");
    }

    private void RestoreSkillLevels(ArrowSkillTreeManager skillManager)
    {
        foreach (var skillData in currentSkillData.skillLevels)
        {
            int skillId = skillData.Key;
            int level = skillData.Value;

            var skill = skillManager.GetSkill(skillId);
            if (skill != null)
            {
                skill.currentLevel = level;
                Debug.Log($"스킬 복원: {skill.skillName} Lv.{level}");
            }
        }
    }

    private void ResetPlayerStatsToDefault(PlayerStats playerStats)
    {
        // 스킬 보너스 리셋
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

    private void ApplyAllSkillEffects(PlayerStats playerStats, ArrowSkillTreeManager skillManager)
    {
        foreach (var skillData in currentSkillData.skillLevels)
        {
            int skillId = skillData.Key;
            int level = skillData.Value;

            var skill = skillManager.GetSkill(skillId);
            if (skill != null && skill.currentLevel > 0)
            {
                ApplySkillEffect(playerStats, skill);
            }
        }
    }

    private void ApplySkillEffect(PlayerStats playerStats, ArrowSkillData skill)
    {
        Debug.Log($"스킬 효과 적용: {skill.skillName} Lv.{skill.currentLevel}");

        foreach (var effect in skill.effects)
        {
            float value = effect.GetValue(skill.currentLevel);

            switch (effect.effectType)
            {
                case SkillEffectType.AttackPower:
                    // ★ 접근자 사용해서 현재 보너스 가져오기
                    float currentAttackBonus = playerStats.SkillAttackPowerBonus;
                    playerStats.SetSkillAttackPowerBonus(currentAttackBonus + value);
                    Debug.Log($"공격력 +{value} 적용 (총 보너스: +{currentAttackBonus + value})");
                    break;

                case SkillEffectType.CriticalChance:
                    playerStats.criticalChance = Mathf.Min(1f, playerStats.criticalChance + (value / 100f));
                    Debug.Log($"크리티컬 확률: {playerStats.criticalChance * 100f}%");
                    break;

                case SkillEffectType.CriticalDamage:
                    playerStats.criticalMultiplier += (value / 100f);
                    Debug.Log($"크리티컬 데미지: {playerStats.criticalMultiplier * 100f}%");
                    break;

                case SkillEffectType.PierceCount:
                    playerStats.pierceCount += (int)value;
                    Debug.Log($"관통 횟수: {playerStats.pierceCount}");
                    break;

                case SkillEffectType.ExplosiveArrow:
                    playerStats.hasExplosiveArrow = true;
                    if (value > 1f) // 마스터 레벨이면 강화 (value가 2라면)
                    {
                        // ★ 기본값에서 마스터 효과 적용 (곱하지 말고 직접 설정)
                        playerStats.explosiveRadius = 5f * 1.5f; // 7.5f
                        playerStats.explosiveDamage = 0.7f * 1.3f; // 0.91f
                        Debug.Log("폭발 마스터 효과 적용!");
                    }
                    Debug.Log($"폭발 화살 활성화 (반경: {playerStats.explosiveRadius})");
                    break;

                case SkillEffectType.GuidedArrow:
                    playerStats.hasGuidedAfterPierce = true;
                    Debug.Log("유도 화살 활성화");
                    break;

                case SkillEffectType.Health:
                    // ★ 접근자 사용해서 현재 보너스 가져오기
                    float currentHealthBonus = playerStats.SkillHealthBonus;
                    playerStats.SetSkillHealthBonus(currentHealthBonus + value);
                    Debug.Log($"체력 +{value} 적용 (총 보너스: +{currentHealthBonus + value})");
                    break;

                case SkillEffectType.MoveSpeed:
                    // ★ 접근자 사용해서 현재 보너스 가져오기  
                    float currentSpeedBonus = playerStats.SkillMoveSpeedBonus;
                    playerStats.SetSkillMoveSpeedBonus(currentSpeedBonus + value);
                    Debug.Log($"이동속도 +{value} 적용");
                    break;
            }
        }
    }
}

[System.Serializable]
public class SkillSaveData
{
    public int availablePoints = 0;
    public int playerLevel = 1;
    public Dictionary<int, int> skillLevels = new Dictionary<int, int>();
}