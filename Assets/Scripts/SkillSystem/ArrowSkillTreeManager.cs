// ArrowSkillTreeManager.cs - 수정본
using System.Collections.Generic;
using System;
using UnityEngine;

public class ArrowSkillTreeManager : MonoBehaviour
{
    [Header("스킬 포인트 관리")]
    public int availablePoints = 10;
    public int playerLevel = 15;

    [Header("스킬 데이터")]
    public ArrowSkillData[] allSkills;

    public static event Action<ArrowSkillData> OnSkillLevelUp;
    public static event Action OnSkillTreeChanged;

    private PlayerStats playerStats;
    private Dictionary<int, ArrowSkillData> skillDict;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        InitializeSkills();
        BuildSkillDictionary();

        Debug.Log($"ArrowSkillTreeManager 초기화 완료 - PlayerStats: {playerStats != null}");
    }

    void Start()
    {
        ApplyAllSkillEffects();
    }

    void InitializeSkills()
    {
        allSkills = new ArrowSkillData[]
        {
            // 티어 1 - 기본 스탯 (5개)
            CreateSkill(1, "힘", "공격력을 증가시킵니다.", SkillTier.Tier1, 5, 0, new int[0],
                new SkillEffectData[] { new SkillEffectData { effectType = SkillEffectType.AttackPower, baseValue = 5f, valuePerLevel = 2f } }),

            CreateSkill(2, "민첩", "크리티컬 확률을 증가시킵니다.", SkillTier.Tier1, 5, 0, new int[0],
                new SkillEffectData[] { new SkillEffectData { effectType = SkillEffectType.CriticalChance, baseValue = 2f, valuePerLevel = 1f, isPercentage = true } }),

            CreateSkill(3, "집중", "크리티컬 데미지를 증가시킵니다.", SkillTier.Tier1, 5, 0, new int[0],
                new SkillEffectData[] { new SkillEffectData { effectType = SkillEffectType.CriticalDamage, baseValue = 10f, valuePerLevel = 5f, isPercentage = true } }),

            CreateSkill(4, "체력", "최대 체력을 증가시킵니다.", SkillTier.Tier1, 3, 5, new int[0],
                new SkillEffectData[] { new SkillEffectData { effectType = SkillEffectType.Health, baseValue = 50f, valuePerLevel = 25f } }),

            CreateSkill(5, "저항", "방어력을 증가시킵니다.", SkillTier.Tier1, 3, 8, new int[0],
                new SkillEffectData[] { new SkillEffectData { effectType = SkillEffectType.Health, baseValue = 25f, valuePerLevel = 15f } }),
            
            // 티어 2 - 화살 기초 (3개)
            CreateSkill(11, "관통", "화살이 적을 관통합니다.", SkillTier.Tier2, 3, 5, new int[]{1},
                new SkillEffectData[] { new SkillEffectData { effectType = SkillEffectType.PierceCount, baseValue = 1f, valuePerLevel = 1f } }),

            CreateSkill(12, "거리데미지", "거리에 따른 데미지가 증가합니다.", SkillTier.Tier2, 3, 5, new int[]{1},
                new SkillEffectData[] { new SkillEffectData { effectType = SkillEffectType.AttackPower, baseValue = 8f, valuePerLevel = 4f } }),

            CreateSkill(13, "폭발", "화살이 폭발 효과를 가집니다.", SkillTier.Tier2, 1, 8, new int[]{11},
                new SkillEffectData[] { new SkillEffectData { effectType = SkillEffectType.ExplosiveArrow, baseValue = 1f, valuePerLevel = 0f } }),
            
            // 티어 3 - 화살 고급 (2개)
            CreateSkill(21, "크리강화", "크리티컬 데미지를 크게 강화합니다.", SkillTier.Tier3, 3, 12, new int[]{2, 12},
                new SkillEffectData[] { new SkillEffectData { effectType = SkillEffectType.CriticalDamage, baseValue = 25f, valuePerLevel = 15f, isPercentage = true } }),

            CreateSkill(22, "연사", "공격 속도를 증가시킵니다.", SkillTier.Tier3, 3, 15, new int[]{2, 11},
                new SkillEffectData[] { new SkillEffectData { effectType = SkillEffectType.AttackSpeed, baseValue = 0.2f, valuePerLevel = 0.1f } }),
            
            // 티어 4 - 궁극기 (3개 중 1개만 선택)
            CreateSkill(31, "관통 마스터", "관통 횟수가 크게 증가하고 관통 후 유도됩니다.", SkillTier.Tier4, 1, 20, new int[]{11, 22},
                new SkillEffectData[] {
                    new SkillEffectData { effectType = SkillEffectType.PierceCount, baseValue = 3f, valuePerLevel = 0f },
                    new SkillEffectData { effectType = SkillEffectType.GuidedArrow, baseValue = 1f, valuePerLevel = 0f }
                }),

            CreateSkill(32, "저격 마스터", "극도로 높은 크리티컬 확률을 가집니다.", SkillTier.Tier4, 1, 20, new int[]{21},
                new SkillEffectData[] { new SkillEffectData { effectType = SkillEffectType.CriticalChance, baseValue = 30f, valuePerLevel = 0f, isPercentage = true } }),

            CreateSkill(33, "폭발 마스터", "폭발 범위와 데미지가 크게 증가합니다.", SkillTier.Tier4, 1, 20, new int[]{13},
                new SkillEffectData[] { new SkillEffectData { effectType = SkillEffectType.ExplosiveArrow, baseValue = 2f, valuePerLevel = 0f } })
        };

        Debug.Log($"스킬 {allSkills.Length}개 초기화 완료");
    }

    ArrowSkillData CreateSkill(int id, string name, string desc, SkillTier tier, int maxLvl, int reqPoints, int[] prereqs, SkillEffectData[] effects)
    {
        return new ArrowSkillData
        {
            skillId = id,
            skillName = name,
            description = desc,
            tier = tier,
            maxLevel = maxLvl,
            requiredTreePoints = reqPoints,
            prerequisiteSkills = prereqs,
            effects = effects
        };
    }

    void BuildSkillDictionary()
    {
        skillDict = new Dictionary<int, ArrowSkillData>();
        foreach (var skill in allSkills)
        {
            skillDict[skill.skillId] = skill;
        }
        Debug.Log($"스킬 딕셔너리 구축 완료: {skillDict.Count}개");
    }

    // 스킬 업그레이드 시도
    public bool TryUpgradeSkill(int skillId)
    {
        if (!skillDict.ContainsKey(skillId))
        {
            Debug.LogWarning($"존재하지 않는 스킬 ID: {skillId}");
            return false;
        }

        var skill = skillDict[skillId];
        Debug.Log($"스킬 업그레이드 시도: {skill.skillName} (현재 레벨: {skill.currentLevel})");

        if (CanUpgradeSkill(skill))
        {
            skill.currentLevel++;
            availablePoints--;

            ApplySkillEffect(skill);
            OnSkillLevelUp?.Invoke(skill);
            OnSkillTreeChanged?.Invoke();

            Debug.Log($"✅ [스킬 업그레이드] {skill.skillName} Lv.{skill.currentLevel} (남은 포인트: {availablePoints})");
            return true;
        }
        else
        {
            Debug.Log($"❌ 스킬 업그레이드 실패: {skill.skillName}");
            return false;
        }
    }

    public bool CanUpgradeSkill(ArrowSkillData skill)
    {
        // 포인트 부족
        if (availablePoints <= 0)
        {
            Debug.Log($"포인트 부족: {availablePoints}");
            return false;
        }

        // 최대 레벨
        if (skill.IsMaxLevel)
        {
            Debug.Log($"최대 레벨: {skill.skillName}");
            return false;
        }

        // 트리 포인트 요구량
        int totalUsed = GetTotalPointsUsed();
        if (totalUsed < skill.requiredTreePoints)
        {
            Debug.Log($"트리 포인트 부족: {totalUsed}/{skill.requiredTreePoints}");
            return false;
        }

        // 선행 스킬 확인
        foreach (int prereqId in skill.prerequisiteSkills)
        {
            if (!skillDict.ContainsKey(prereqId) || !skillDict[prereqId].IsLearned)
            {
                Debug.Log($"선행 스킬 부족: {prereqId}");
                return false;
            }
        }

        return true;
    }

    // 스킬 다운그레이드 시도 (우클릭용)
    public bool TryDowngradeSkill(int skillId)
    {
        if (!skillDict.ContainsKey(skillId)) return false;

        var skill = skillDict[skillId];
        Debug.Log($"스킬 다운그레이드 시도: {skill.skillName} (현재 레벨: {skill.currentLevel})");

        if (CanDowngradeSkill(skill))
        {
            skill.currentLevel--;
            availablePoints++;

            // 전체 스킬 효과를 다시 적용 (다운그레이드된 스킬 효과 제거)
            ApplyAllSkillEffects();
            OnSkillTreeChanged?.Invoke();

            Debug.Log($"⬇️ [스킬 다운그레이드] {skill.skillName} Lv.{skill.currentLevel} (포인트 +1)");
            return true;
        }

        return false;
    }

    public bool CanDowngradeSkill(ArrowSkillData skill)
    {
        // 레벨이 0이면 더 이상 내릴 수 없음
        if (skill.currentLevel <= 0) return false;

        // 이 스킬을 선행 스킬로 요구하는 다른 스킬이 배워져 있는지 확인
        foreach (var otherSkill in allSkills)
        {
            if (otherSkill.IsLearned && System.Array.Exists(otherSkill.prerequisiteSkills, prereq => prereq == skill.skillId))
            {
                return false; // 이 스킬에 의존하는 다른 스킬이 있음
            }
        }

        return true;
    }

    void ApplySkillEffect(ArrowSkillData skill)
    {
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats가 null입니다!");
            return;
        }

        Debug.Log($"스킬 효과 적용: {skill.skillName} Lv.{skill.currentLevel}");

        foreach (var effect in skill.effects)
        {
            float value = effect.GetValue(skill.currentLevel);
            Debug.Log($"- {effect.effectType}: {value} (기본값: {effect.baseValue}, 레벨당: {effect.valuePerLevel})");

            switch (effect.effectType)
            {
                case SkillEffectType.AttackPower:
                    // 직접 PlayerStats의 기본 스탯을 수정하지 말고 별도로 관리
                    break;

                case SkillEffectType.CriticalChance:
                    playerStats.criticalChance = Mathf.Min(1f, playerStats.criticalChance + (value / 100f));
                    Debug.Log($"크리티컬 확률 적용: {playerStats.criticalChance * 100f}%");
                    break;

                case SkillEffectType.PierceCount:
                    playerStats.pierceCount += (int)value;
                    Debug.Log($"관통 횟수 적용: {playerStats.pierceCount}");
                    break;

                case SkillEffectType.ExplosiveArrow:
                    playerStats.hasExplosiveArrow = true;
                    Debug.Log("폭발 화살 활성화");
                    break;

                case SkillEffectType.GuidedArrow:
                    playerStats.hasGuidedAfterPierce = true;
                    Debug.Log("유도 화살 활성화");
                    break;
            }
        }
    }

    void ApplyAllSkillEffects()
    {
        if (playerStats == null) return;

        Debug.Log("모든 스킬 효과 재적용 시작...");

        // 먼저 모든 스탯을 기본값으로 리셋
        ResetPlayerStatsToDefault();

        // 학습한 스킬들의 효과를 다시 적용
        foreach (var skill in allSkills)
        {
            if (skill.IsLearned)
            {
                ApplySkillEffect(skill);
            }
        }

        Debug.Log("모든 스킬 효과가 플레이어에게 적용되었습니다.");
    }

    public int GetTotalPointsUsed()
    {
        int total = 0;
        foreach (var skill in allSkills)
        {
            total += skill.currentLevel;
        }
        return total;
    }

    public ArrowSkillData GetSkill(int skillId)
    {
        return skillDict.ContainsKey(skillId) ? skillDict[skillId] : null;
    }

    public ArrowSkillData[] GetSkillsByTier(SkillTier tier)
    {
        return System.Array.FindAll(allSkills, skill => skill.tier == tier);
    }

    // 스킬 리셋
    public void ResetAllSkills()
    {
        int pointsToReturn = GetTotalPointsUsed();

        Debug.Log($"리셋 전 - 사용 가능한 포인트: {availablePoints}, 사용된 포인트: {pointsToReturn}");

        // 모든 스킬 레벨을 0으로 초기화
        foreach (var skill in allSkills)
        {
            skill.currentLevel = 0;
        }

        // 사용된 포인트를 다시 사용 가능한 포인트로 되돌림
        availablePoints += pointsToReturn;

        Debug.Log($"리셋 후 - 사용 가능한 포인트: {availablePoints}, 사용된 포인트: {GetTotalPointsUsed()}");

        // 플레이어 스탯을 기본값으로 완전히 리셋
        ResetPlayerStatsToDefault();

        OnSkillTreeChanged?.Invoke();
        Debug.Log($"✅ 스킬 트리 리셋 완료! 반환된 포인트: {pointsToReturn}");
    }

    // 플레이어 스탯을 기본값으로 리셋하는 별도 메서드
    void ResetPlayerStatsToDefault()
    {
        if (playerStats == null) return;

        // 화살 특성 초기화
        playerStats.criticalChance = 0.15f; // 기본 크리티컬 확률
        playerStats.pierceCount = 0;
        playerStats.hasExplosiveArrow = false;
        playerStats.hasGuidedAfterPierce = false;
        playerStats.explosiveRadius = 5f;
        playerStats.explosiveDamage = 0.7f;

        Debug.Log("플레이어 스탯이 기본값으로 리셋되었습니다.");
    }

    // ★ 외부에서 이벤트를 발생시킬 수 있는 공개 메서드 추가
    public void NotifySkillTreeChanged()
    {
        OnSkillTreeChanged?.Invoke();
        Debug.Log("스킬 트리 변경 이벤트 발생");
    }

    // ★ 외부에서 스킬 레벨업 이벤트를 발생시킬 수 있는 메서드
    public void NotifySkillLevelUp(ArrowSkillData skill)
    {
        OnSkillLevelUp?.Invoke(skill);
        Debug.Log($"스킬 레벨업 이벤트 발생: {skill.skillName}");
    }

    // ★ 스킬 상태를 외부에서 설정할 수 있는 메서드 (복원용)
    public void SetSkillLevel(int skillId, int level)
    {
        var skill = GetSkill(skillId);
        if (skill != null)
        {
            skill.currentLevel = level;
            Debug.Log($"스킬 레벨 설정: {skill.skillName} Lv.{level}");
        }
    }

    // ★ 포인트를 외부에서 설정할 수 있는 메서드
    public void SetAvailablePoints(int points)
    {
        availablePoints = points;
        Debug.Log($"사용 가능한 포인트 설정: {points}");
    }

    // ★ 플레이어 레벨을 외부에서 설정할 수 있는 메서드
    public void SetPlayerLevel(int level)
    {
        playerLevel = level;
        Debug.Log($"플레이어 레벨 설정: {level}");
    }

    // ★ 스킬 상태 복원을 위한 통합 메서드
    public void RestoreSkillState(int availablePoints, int playerLevel, Dictionary<int, int> skillLevels)
    {
        this.availablePoints = availablePoints;
        this.playerLevel = playerLevel;

        // 모든 스킬 레벨 리셋
        foreach (var skill in allSkills)
        {
            skill.currentLevel = 0;
        }

        // 저장된 스킬 레벨 복원
        foreach (var skillData in skillLevels)
        {
            SetSkillLevel(skillData.Key, skillData.Value);
        }

        // 플레이어 스탯에 효과 적용
        ApplyAllSkillEffects();

        // UI 업데이트
        NotifySkillTreeChanged();

        Debug.Log($"✅ 스킬 상태 복원 완료 - 포인트: {availablePoints}, 스킬: {skillLevels.Count}개");
    }

    // 테스트용 메서드들
    [ContextMenu("포인트 10개 추가")]
    void AddTestPoints()
    {
        availablePoints += 10;
        Debug.Log($"포인트 추가됨. 현재: {availablePoints}");
        OnSkillTreeChanged?.Invoke();
    }

    [ContextMenu("힘 스킬 업그레이드")]
    void TestUpgradeStrength() { TryUpgradeSkill(1); }

    [ContextMenu("스킬 트리 리셋")]
    void TestResetSkills() { ResetAllSkills(); }
}