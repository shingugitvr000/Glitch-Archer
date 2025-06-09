using UnityEngine;
using System;

namespace SkillSystem
{
    [Serializable]
    public class SkillEffect
    {
        [Header("기본 정보")]
        public string effectName;                           // 효과 이름
        [TextArea(2, 3)]
        public string description;                          // 효과 설명

        [Header("효과 설정")]
        public StatType targetStat;                         // 영향을 주는 스탯
        public EffectApplicationType applicationType;       // 적용 방식
        public float baseValue;                             // 기본 값
        public float valuePerLevel;                         // 레벨당 증가량
        public bool isPercentage;                           // 퍼센트 여부

        [Header("조건")]
        public bool hasCondition;                           // 조건부 효과인지
        public string conditionDescription;                 // 조건 설명 (예: "연속 공격시")

        // 현재 레벨에서의 효과 값 계산
        public float GetEffectValue(int currentLevel)
        {
            return baseValue + (valuePerLevel * (currentLevel - 1));
        }

        // 효과 설명 텍스트 생성
        public string GetEffectDescription(int currentLevel)
        {
            float value = GetEffectValue(currentLevel);
            string percentSymbol = isPercentage ? "%" : "";

            if (hasCondition)
            {
                return $"{conditionDescription}: {targetStat} {(applicationType == EffectApplicationType.Add ? "+" : "")}{value}{percentSymbol}";
            }
            else
            {
                return $"{targetStat} {(applicationType == EffectApplicationType.Add ? "+" : "")}{value}{percentSymbol}";
            }
        }
    }
}