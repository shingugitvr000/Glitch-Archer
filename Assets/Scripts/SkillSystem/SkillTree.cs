using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SkillSystem
{
    [CreateAssetMenu(fileName = "New Skill Tree", menuName = "Skill System/Skill Tree")]
    public class SkillTree : ScriptableObject
    {
        [Header("트리 정보")]
        public SkillTreeType treeType;                      // 트리 타입
        public string treeName;                             // 트리 이름
        [TextArea(2, 3)]
        public string description;                          // 트리 설명
        public Sprite treeIcon;                             // 트리 아이콘
        public Color treeColor = Color.white;               // 트리 색상 (UI용)

        [Header("스킬 목록")]
        public List<Skill> skills = new List<Skill>();     // 이 트리의 모든 스킬들

        // 현재 투자된 총 포인트
        public int TotalPointsSpent => skills.Sum(skill => skill.CurrentLevel);

        // 현재 사용 가능한 최대 티어
        public SkillTier GetMaxAvailableTier()
        {
            int totalPoints = TotalPointsSpent;

            if (totalPoints >= 20) return SkillTier.Keystone;
            if (totalPoints >= 10) return SkillTier.SecondaryBranch;
            if (totalPoints >= 5) return SkillTier.PrimaryBranch;
            return SkillTier.Root;
        }

        // 특정 티어의 스킬들 가져오기
        public List<Skill> GetSkillsByTier(SkillTier tier)
        {
            return skills.Where(skill => skill.tier == tier).ToList();
        }

        // 스킬 ID로 스킬 찾기
        public Skill GetSkillById(int skillId)
        {
            return skills.FirstOrDefault(skill => skill.skillId == skillId);
        }

        // 스킬 이름으로 스킬 찾기
        public Skill GetSkillByName(string skillName)
        {
            return skills.FirstOrDefault(skill => skill.skillName == skillName);
        }

        // 현재 습득한 스킬들
        public List<Skill> GetLearnedSkills()
        {
            return skills.Where(skill => skill.IsLearned).ToList();
        }

        // 스킬 습득 가능 여부 확인
        public bool CanLearnSkill(int skillId)
        {
            Skill skill = GetSkillById(skillId);
            if (skill == null || skill.IsMaxLevel) return false;

            // 트리 포인트 요구사항 확인
            if (TotalPointsSpent < skill.requiredTreePoints) return false;

            // 티어 요구사항 확인
            SkillTier maxAvailableTier = GetMaxAvailableTier();
            if (skill.tier > maxAvailableTier) return false;

            // 선행 스킬 확인
            foreach (int prereqId in skill.prerequisiteSkillIds)
            {
                Skill prereqSkill = GetSkillById(prereqId);
                if (prereqSkill == null || !prereqSkill.IsLearned)
                    return false;
            }

            return true;
        }

        // 특정 스탯에 영향을 주는 모든 효과 가져오기
        public List<SkillEffect> GetAllEffectsForStat(StatType statType)
        {
            List<SkillEffect> allEffects = new List<SkillEffect>();

            foreach (var skill in GetLearnedSkills())
            {
                allEffects.AddRange(skill.GetEffectsForStat(statType));
            }

            return allEffects;
        }

        // 트리 완성도 (퍼센트)
        public float GetCompletionPercentage()
        {
            int totalPossiblePoints = skills.Sum(skill => skill.maxLevel);
            if (totalPossiblePoints == 0) return 0f;

            return (float)TotalPointsSpent / totalPossiblePoints * 100f;
        }

        // 트리 리셋
        public void ResetTree()
        {
            foreach (var skill in skills)
            {
                skill.ResetSkill();
            }
        }
    }
}