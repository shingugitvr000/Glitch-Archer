using UnityEngine;

namespace SkillSystem
{
    // 스킬 트리 타입
    public enum SkillTreeType
    {
        Marksmanship,   // 마크스맨십
        Survival,       // 서바이벌
        ArcaneArcher    // 아케인 아처
    }

    // 스킬 타입
    public enum SkillType
    {
        Passive,        // 패시브 (자동 적용)
        Active,         // 액티브 (키 입력)
        Toggle          // 토글 (온/오프)
    }

    // 스킬 티어
    public enum SkillTier
    {
        Root = 1,           // 루트 스킬
        PrimaryBranch = 2,  // 1차 분기
        SecondaryBranch = 3,// 2차 분기
        Keystone = 4        // 키스톤
    }

    // 스탯 타입 (스킬이 영향을 주는 스탯들)
    public enum StatType
    {
        // 기본 스탯
        Health,             // 체력
        AttackPower,        // 공격력
        CriticalChance,     // 치명타 확률
        CriticalDamage,     // 치명타 피해
        AttackSpeed,        // 공격속도
        MoveSpeed,          // 이동속도
        Defense,            // 방어력
        HealthRegen,        // 체력 재생

        // 특수 스탯
        DodgeDistance,      // 회피 거리
        DodgeCooldown,      // 회피 쿨다운
        PierceCount,        // 관통 횟수
        MultiShotCount,     // 멀티샷 개수

        // 원소 관련
        FireDamage,         // 화염 피해
        IceDamage,          // 빙결 피해
        LightningDamage,    // 번개 피해
        ElementalChance     // 원소 발동 확률
    }

    // 효과 적용 방식
    public enum EffectApplicationType
    {
        Add,                // 더하기 (10 + 5 = 15)
        Multiply,           // 곱하기 (10 * 1.5 = 15)
        Set                 // 직접 설정 (= 15)
    }
}