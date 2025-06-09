using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SkillSystem
{
    [CreateAssetMenu(fileName = "New Skill", menuName = "Skill System/Skill")]
    public class Skill : ScriptableObject
    {
        [Header("기본 정보")]
        public int skillId;                                 // 고유 ID
        public string skillName;                            // 스킬 이름
        [TextArea(3, 5)]
        public string description;                          // 스킬 설명
        public Sprite icon;                                 // 스킬 아이콘

        [Header("스킬 분류")]
        public SkillTreeType treeType;                      // 소속 트리
        public SkillTier tier;                              // 스킬 티어
        public SkillType skillType;                         // 스킬 타입

        [Header("레벨 설정")]
        public int maxLevel = 1;                            // 최대 레벨
        [SerializeField] private int currentLevel = 0;      // 현재 레벨 (런타임에서만 변경)

        [Header("요구 조건")]
        public int requiredTreePoints = 0;                  // 해당 트리에 필요한 총 포인트
        public List<int> prerequisiteSkillIds = new List<int>(); // 선행 스킬 ID들

        [Header("스킬 효과")]
        public List<SkillEffect> effects = new List<SkillEffect>(); // 스킬 효과들

        [Header("액티브 스킬 설정")]
        public KeyCode activationKey = KeyCode.None;        // 활성화 키 (액티브 스킬용)
        public float cooldownTime = 0f;                     // 쿨다운 시간
        public float manaCost = 0f;                         // 마나 소모량

        // 프로퍼티들
        public int CurrentLevel
        {
            get => currentLevel;
            set => currentLevel = Mathf.Clamp(value, 0, maxLevel);
        }

        public bool IsLearned => currentLevel > 0;
        public bool IsMaxLevel => currentLevel >= maxLevel;
        public bool CanLevelUp => currentLevel < maxLevel;

        // 스킬 레벨업
        public bool LevelUp()
        {
            if (CanLevelUp)
            {
                currentLevel++;
                return true;
            }
            return false;
        }

        // 스킬 레벨 다운 (리셋용)
        public bool LevelDown()
        {
            if (currentLevel > 0)
            {
                currentLevel--;
                return true;
            }
            return false;
        }

        // 완전 리셋
        public void ResetSkill()
        {
            currentLevel = 0;
        }

        // 현재 레벨의 모든 효과 가져오기
        public List<SkillEffect> GetCurrentEffects()
        {
            return effects.Where(effect => currentLevel > 0).ToList();
        }

        // 특정 스탯에 영향을 주는 효과들 가져오기
        public List<SkillEffect> GetEffectsForStat(StatType statType)
        {
            return effects.Where(effect => effect.targetStat == statType && currentLevel > 0).ToList();
        }

        // 스킬 툴팁용 텍스트 생성
        public string GetTooltipText()
        {
            string tooltip = $"<b>{skillName}</b>\n";
            tooltip += $"<i>{description}</i>\n\n";

            if (currentLevel > 0)
            {
                tooltip += $"<color=green>현재 레벨: {currentLevel}/{maxLevel}</color>\n";

                foreach (var effect in effects)
                {
                    tooltip += $"• {effect.GetEffectDescription(currentLevel)}\n";
                }
            }
            else
            {
                tooltip += $"<color=gray>미습득</color>\n";

                foreach (var effect in effects)
                {
                    tooltip += $"• {effect.GetEffectDescription(1)} (1레벨 기준)\n";
                }
            }

            if (skillType == SkillType.Active)
            {
                tooltip += $"\n<color=yellow>활성화 키: {activationKey}</color>";
                if (cooldownTime > 0)
                    tooltip += $"\n<color=cyan>쿨다운: {cooldownTime}초</color>";
                if (manaCost > 0)
                    tooltip += $"\n<color=blue>마나 소모: {manaCost}</color>";
            }

            return tooltip;
        }

        // 다음 레벨 미리보기 텍스트
        public string GetNextLevelPreview()
        {
            if (IsMaxLevel) return "";

            string preview = $"<color=yellow>다음 레벨 ({currentLevel + 1}):</color>\n";
            foreach (var effect in effects)
            {
                preview += $"• {effect.GetEffectDescription(currentLevel + 1)}\n";
            }
            return preview;
        }
    }
}