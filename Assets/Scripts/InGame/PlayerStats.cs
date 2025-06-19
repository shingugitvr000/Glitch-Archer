using UnityEngine;
using System;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [Header("기본 스탯")]
    [SerializeField] private float baseHealth = 100f;
    [SerializeField] private float baseAttackPower = 25f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseCriticalChance = 0.1f;
    [SerializeField] private float baseCriticalDamage = 1.5f;

    [Header("업그레이드 스탯")]
    [SerializeField] public int strengthLevel = 0;      // 근력 - 공격력, 관통력
    [SerializeField] public int dexterityLevel = 0;     // 기민함 - 공격속도, 크리티컬
    [SerializeField] public int focusLevel = 0;         // 집중 - 정확도, 스킬 쿨다운
    [SerializeField] public int constitutionLevel = 0;  // 체력 - HP, 방어력
    [SerializeField] public int resistanceLevel = 0;    // 저항 - 상태이상 저항

    [Header("현재 상태")]
    public float currentHealth;
    public int availablePoints = 10; // 사용 가능한 스탯 포인트

    [Header("스킬 보너스 (런타임)")]
    [SerializeField] private float skillAttackPowerBonus = 0f;
    [SerializeField] private float skillHealthBonus = 0f;
    [SerializeField] private float skillMoveSpeedBonus = 0f;

    [Header("거리 데미지 스킬")]
    public bool hasDistanceDamage = false;        // 거리 데미지 활성화 여부
    public float distanceDamageMultiplier = 0.1f; // 거리당 데미지 증가율 (10% per 10m)
    public float maxDistanceBonus = 50f;          // 최대 거리 (50m)

    // 계산된 최종 스탯들
    public float FinalHealth => baseHealth + (constitutionLevel * 20f);
    public float FinalAttackPower => baseAttackPower + (strengthLevel * 5f);
    public float FinalMoveSpeed => baseMoveSpeed + (dexterityLevel * 0.2f);
    public float FinalAttackSpeed => baseAttackSpeed + (dexterityLevel * 0.1f);
    public float FinalCriticalChance => Mathf.Clamp01(baseCriticalChance + (dexterityLevel * 0.02f));
    public float FinalCriticalDamage => baseCriticalDamage + (strengthLevel * 0.1f);
    public float PierceChance => strengthLevel * 0.05f; // 관통 확률
    public float StatusResistance => resistanceLevel * 0.1f; // 상태이상 저항

    // 스킬 보너스가 포함된 최종 스탯들
    public float FinalHealthWithSkills => FinalHealth + skillHealthBonus;
    public float FinalAttackPowerWithSkills => FinalAttackPower + skillAttackPowerBonus;
    public float FinalMoveSpeedWithSkills => FinalMoveSpeed + skillMoveSpeedBonus;

    // 이벤트
    public static event Action<float, float> OnHealthChanged; // 현재체력, 최대체력
    public static event Action OnStatsChanged;

    [Header("화살 스킬")]
    public int pierceCount = 0;                    // 관통 횟수
    public bool hasGuidedAfterPierce = false;      // 관통 후 유도 여부  
    public float guidedRange = 15f;                // 유도 범위

    [Header("폭발 스킬")]
    public bool hasExplosiveArrow = false;    // 폭발 화살 여부
    public float explosiveRadius = 5f;        // 폭발 범경
    public float explosiveDamage = 0.7f;      // 폭발 데미지 비율 (원래 데미지의 70%)

    [Header("크리티컬 ")]
    public float criticalChance = 0.15f;      // 크리티컬 확률 (15%)
    public float criticalMultiplier = 2.0f;   // 크리티컬 비율 (2배)

    // 스킬 보너스 접근자
    public float SkillAttackPowerBonus => skillAttackPowerBonus;
    public float SkillHealthBonus => skillHealthBonus;
    public float SkillMoveSpeedBonus => skillMoveSpeedBonus;

    public bool ShouldPierce()
    {
        return pierceCount > 0;
    }

    public bool ShouldCritical()
    {
        return UnityEngine.Random.value < criticalChance;
    }

    private void Awake()
    {
        currentHealth = FinalHealthWithSkills;
    }

    private void Start()
    {
        // 기존 ThirdPersonController의 이동속도 적용
        var controller = GetComponent<ThirdPersonController>();
        if (controller != null)
        {
            controller.MoveSpeed = FinalMoveSpeedWithSkills;
            controller.SprintSpeed = FinalMoveSpeedWithSkills * 1.8f;
        }

        NotifyStatsChanged();
    }

    // 스킬 보너스 설정 메서드들
    public void SetSkillAttackPowerBonus(float bonus)
    {
        skillAttackPowerBonus = bonus;
        Debug.Log($"스킬 공격력 보너스 설정: +{bonus}");
    }

    public void SetSkillHealthBonus(float bonus)
    {
        float oldMaxHealth = FinalHealthWithSkills;
        skillHealthBonus = bonus;

        // 체력 비율 유지하면서 최대 체력 업데이트
        if (oldMaxHealth > 0)
        {
            float healthRatio = currentHealth / oldMaxHealth;
            float newMaxHealth = FinalHealthWithSkills;
            currentHealth = newMaxHealth * healthRatio;
        }
        else
        {
            currentHealth = FinalHealthWithSkills;
        }

        OnHealthChanged?.Invoke(currentHealth, FinalHealthWithSkills);
        Debug.Log($"스킬 체력 보너스 설정: +{bonus} (현재: {currentHealth:F0}/{FinalHealthWithSkills:F0})");
    }

    public void SetSkillMoveSpeedBonus(float bonus)
    {
        skillMoveSpeedBonus = bonus;

        // ThirdPersonController 이동속도 업데이트
        var controller = GetComponent<ThirdPersonController>();
        if (controller != null)
        {
            controller.MoveSpeed = FinalMoveSpeedWithSkills;
            controller.SprintSpeed = FinalMoveSpeedWithSkills * 1.8f;
        }

        Debug.Log($"스킬 이동속도 보너스 설정: +{bonus}");
    }

    // 모든 스킬 보너스 리셋
    public void ResetAllSkillBonuses()
    {
        SetSkillAttackPowerBonus(0f);
        SetSkillHealthBonus(0f);
        SetSkillMoveSpeedBonus(0f);

        Debug.Log("모든 스킬 보너스가 리셋되었습니다");
    }

    // 스탯 업그레이드
    public bool UpgradeStat(StatUpgradeType statType)
    {
        if (availablePoints <= 0) return false;

        switch (statType)
        {
            case StatUpgradeType.Strength:
                strengthLevel++;
                break;
            case StatUpgradeType.Dexterity:
                dexterityLevel++;
                break;
            case StatUpgradeType.Focus:
                focusLevel++;
                break;
            case StatUpgradeType.Constitution:
                constitutionLevel++;
                break;
            case StatUpgradeType.Resistance:
                resistanceLevel++;
                break;
        }

        availablePoints--;
        ApplyStatChanges();
        NotifyStatsChanged();
        return true;
    }

    // 스탯 변경 적시 적용
    private void ApplyStatChanges()
    {
        // 체력 비율 유지하면서 최대체력 업데이트
        float healthRatio = currentHealth / (baseHealth + ((constitutionLevel - 1) * 20f));
        float newMaxHealth = FinalHealthWithSkills;
        currentHealth = newMaxHealth * healthRatio;

        // 이동속도 실시간 적용
        var controller = GetComponent<ThirdPersonController>();
        if (controller != null)
        {
            controller.MoveSpeed = FinalMoveSpeedWithSkills;
            controller.SprintSpeed = FinalMoveSpeedWithSkills * 1.8f;
        }

        OnHealthChanged?.Invoke(currentHealth, newMaxHealth);
    }

    // 데미지 받기 (기존 시스템과 연동)
    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, FinalHealthWithSkills);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 체력 회복
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(FinalHealthWithSkills, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, FinalHealthWithSkills);
    }

    // 크리티컬 히트 계산
    public float CalculateDamage(float baseDamage)
    {
        bool isCritical = UnityEngine.Random.value < FinalCriticalChance;
        float finalDamage = isCritical ? baseDamage * FinalCriticalDamage : baseDamage;

        // 스킬 공격력 보너스 추가
        finalDamage += skillAttackPowerBonus;

        // 힘 스탯 보너스 적용
        finalDamage += strengthLevel * 2f;

        return finalDamage;
    }

    private void Die()
    {
        Debug.Log("플레이어 사망!");
        // 사망 처리 로직
    }

    private void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    // 스탯 리셋
    public void ResetStats()
    {
        availablePoints += strengthLevel + dexterityLevel + focusLevel + constitutionLevel + resistanceLevel;
        strengthLevel = dexterityLevel = focusLevel = constitutionLevel = resistanceLevel = 0;

        ApplyStatChanges();
        NotifyStatsChanged();
    }

    // 디버그용 정보 표시
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 280));
        GUILayout.Label($"=== 플레이어 스탯 ===");
        GUILayout.Label($"체력: {currentHealth:F0}/{FinalHealthWithSkills:F0}");
        GUILayout.Label($"공격력: {FinalAttackPowerWithSkills:F1} (+{skillAttackPowerBonus:F1})");
        GUILayout.Label($"크리티컬: {FinalCriticalChance * 100:F1}%");
        GUILayout.Label($"관통: {pierceCount}회");
        GUILayout.Label($"이동속도: {FinalMoveSpeedWithSkills:F1}");
        GUILayout.Space(10);
        GUILayout.Label($"=== 화살 특성 ===");
        GUILayout.Label($"폭발 화살: {(hasExplosiveArrow ? "활성" : "비활성")}");
        GUILayout.Label($"유도 화살: {(hasGuidedAfterPierce ? "활성" : "비활성")}");
        if (hasExplosiveArrow)
        {
            GUILayout.Label($"폭발 반경: {explosiveRadius:F1}m");
            GUILayout.Label($"폭발 데미지: {explosiveDamage * 100:F0}%");
        }
        GUILayout.Space(10);
        GUILayout.Label($"사용가능 포인트: {availablePoints}");
        GUILayout.Label($"STR: {strengthLevel} | DEX: {dexterityLevel}");
        GUILayout.Label($"FOC: {focusLevel} | CON: {constitutionLevel}");
        GUILayout.Label($"RES: {resistanceLevel}");
        GUILayout.Space(5);
        GUILayout.Label("F1 - 스킬 상태 확인");
        GUILayout.EndArea();
    }
}

public enum StatUpgradeType
{
    Strength,      // 근력
    Dexterity,     // 기민함  
    Focus,         // 집중
    Constitution,  // 체력
    Resistance     // 저항
}