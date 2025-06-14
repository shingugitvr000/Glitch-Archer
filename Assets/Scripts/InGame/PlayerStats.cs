using UnityEngine;
using System;
using StarterAssets;

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
    [SerializeField] public int dexterityLevel = 0;     // 기민함 - 공격속도, 크리율
    [SerializeField] public int focusLevel = 0;         // 집중 - 정확도, 스킬 쿨다운
    [SerializeField] public int constitutionLevel = 0;  // 체력 - HP, 방어력
    [SerializeField] public int resistanceLevel = 0;    // 저항 - 상태이상 저항

    [Header("현재 상태")]
    public float currentHealth;
    public int availablePoints = 10; // 사용 가능한 스탯 포인트

    // 계산된 최종 스탯들
    public float FinalHealth => baseHealth + (constitutionLevel * 20f);
    public float FinalAttackPower => baseAttackPower + (strengthLevel * 5f);
    public float FinalMoveSpeed => baseMoveSpeed + (dexterityLevel * 0.2f);
    public float FinalAttackSpeed => baseAttackSpeed + (dexterityLevel * 0.1f);
    public float FinalCriticalChance => Mathf.Clamp01(baseCriticalChance + (dexterityLevel * 0.02f));
    public float FinalCriticalDamage => baseCriticalDamage + (strengthLevel * 0.1f);
    public float PierceChance => strengthLevel * 0.05f; // 관통 확률
    public float StatusResistance => resistanceLevel * 0.1f; // 상태이상 저항

    // 이벤트
    public static event Action<float, float> OnHealthChanged; // 현재체력, 최대체력
    public static event Action OnStatsChanged;

    private void Awake()
    {
        currentHealth = FinalHealth;
    }

    private void Start()
    {
        // 기존 ThirdPersonController의 이동속도 적용
        var controller = GetComponent<ThirdPersonController>();
        if (controller != null)
        {
            controller.MoveSpeed = FinalMoveSpeed;
            controller.SprintSpeed = FinalMoveSpeed * 1.8f;
        }

        NotifyStatsChanged();
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

    // 스탯 변경 즉시 적용
    private void ApplyStatChanges()
    {
        // 체력 비율 유지하며 최대체력 업데이트
        float healthRatio = currentHealth / (baseHealth + ((constitutionLevel - 1) * 20f));
        float newMaxHealth = FinalHealth;
        currentHealth = newMaxHealth * healthRatio;

        // 이동속도 실시간 적용
        var controller = GetComponent<ThirdPersonController>();
        if (controller != null)
        {
            controller.MoveSpeed = FinalMoveSpeed;
            controller.SprintSpeed = FinalMoveSpeed * 1.8f;
        }

        OnHealthChanged?.Invoke(currentHealth, newMaxHealth);
    }

    // 데미지 받기 (기존 시스템과 연동)
    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, FinalHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 체력 회복
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(FinalHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, FinalHealth);
    }

    // 크리티컬 히트 계산
    public float CalculateDamage(float baseDamage)
    {
        bool isCritical = UnityEngine.Random.value < FinalCriticalChance;
        float finalDamage = isCritical ? baseDamage * FinalCriticalDamage : baseDamage;

        // 힘 스탯 보너스 적용
        finalDamage += strengthLevel * 2f;

        return finalDamage;
    }

    // 관통 여부 확인
    public bool ShouldPierce()
    {
        return UnityEngine.Random.value < PierceChance;
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

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"=== 플레이어 스탯 ===");
        GUILayout.Label($"체력: {currentHealth:F0}/{FinalHealth:F0}");
        GUILayout.Label($"공격력: {FinalAttackPower:F1}");
        GUILayout.Label($"크리티컬: {FinalCriticalChance * 100:F1}%");
        GUILayout.Label($"관통률: {PierceChance * 100:F1}%");
        GUILayout.Label($"이동속도: {FinalMoveSpeed:F1}");
        GUILayout.Space(10);
        GUILayout.Label($"사용가능 포인트: {availablePoints}");
        GUILayout.Label($"STR: {strengthLevel} | DEX: {dexterityLevel}");
        GUILayout.Label($"FOC: {focusLevel} | CON: {constitutionLevel}");
        GUILayout.Label($"RES: {resistanceLevel}");
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