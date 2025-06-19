using UnityEngine;
using System.Collections.Generic;
using System;

// 화살 스킬 데이터 클래스
[System.Serializable]
public class ArrowSkillData
{
    public int skillId;
    public string skillName;
    public string description;
    public SkillTier tier;
    public int maxLevel;
    public int currentLevel = 0;
    public int requiredTreePoints; // 트리에 투자된 총 포인트 요구량
    public int[] prerequisiteSkills = new int[0]; // 선행 스킬 ID들
    public Sprite skillIcon;

    // 스킬 효과 데이터
    public SkillEffectData[] effects = new SkillEffectData[0];

    public bool IsLearned => currentLevel > 0;
    public bool IsMaxLevel => currentLevel >= maxLevel;
    public bool CanLevelUp => currentLevel < maxLevel;
}

// 스킬 효과 데이터
[System.Serializable]
public class SkillEffectData
{
    public SkillEffectType effectType;
    public float baseValue;
    public float valuePerLevel;
    public bool isPercentage;

    public float GetValue(int level)
    {
        return baseValue + (valuePerLevel * (level - 1));
    }
}

// 스킬 효과 타입
public enum SkillEffectType
{
    AttackPower,        // 공격력 증가
    CriticalChance,     // 크리티컬 확률 증가
    CriticalDamage,     // 크리티컬 데미지 증가
    PierceCount,        // 관통 횟수
    ExplosiveArrow,     // 폭발 화살
    GuidedArrow,        // 유도 화살
    AttackSpeed,        // 공격 속도
    Health,             // 체력
    MoveSpeed          // 이동 속도
}

// 스킬 티어
public enum SkillTier
{
    Tier1 = 1,  // 기본 스탯
    Tier2 = 2,  // 화살 기초
    Tier3 = 3,  // 화살 고급
    Tier4 = 4   // 궁극기
}