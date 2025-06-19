using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillNodeUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public Image skillIcon;
    public Image backgroundCircle;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI levelText;
    public Button skillButton; // 이건 비활성화할 예정
    public GameObject levelBadge;
    public GameObject requirementBadge;
    public TextMeshProUGUI requirementText;

    [Header("다음 레벨 미리보기 (NEW)")]
    public GameObject nextLevelPreview;           // 다음 레벨 미리보기 패널
    public TextMeshProUGUI nextLevelText;        // "다음: +5 공격력" 같은 텍스트
    public Image previewArrow;                   // 다음 레벨을 가리키는 화살표
    public GameObject canUpgradeIndicator;       // 업그레이드 가능 표시 (초록 테두리, 반짝임 등)
    public GameObject blockedIndicator;          // 업그레이드 불가능 표시 (빨간 X, 자물쇠 등)

    [Header("상태 색상")]
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
    public Color availableColor = new Color(1f, 1f, 0f, 1f);
    public Color learnedColor = new Color(0f, 1f, 0f, 1f);

    [Header("미리보기 색상 (NEW)")]
    public Color canUpgradeGlowColor = new Color(0f, 1f, 0f, 0.5f);   // 초록 반짝임
    public Color blockedColor = new Color(1f, 0f, 0f, 0.8f);         // 빨간 차단 표시

    private ArrowSkillData linkedSkill;
    private LobbySkillTreeUI uiManager;
    private EventTrigger eventTrigger;
    private ArrowSkillTreeManager skillManager;

    public void Initialize(ArrowSkillData skill, LobbySkillTreeUI manager)
    {
        linkedSkill = skill;
        uiManager = manager;
        skillManager = FindObjectOfType<ArrowSkillTreeManager>();

        Debug.Log($"SkillNodeUI 초기화: {skill.skillName}");

        // UI 기본 설정
        if (skillNameText != null)
            skillNameText.text = skill.skillName;

        if (requirementText != null && requirementBadge != null)
        {
            if (skill.requiredTreePoints > 0)
            {
                requirementText.text = skill.requiredTreePoints.ToString();
                requirementBadge.SetActive(true);
            }
            else
            {
                requirementBadge.SetActive(false);
            }
        }

        // 기존 Button 비활성화
        if (skillButton != null)
        {
            skillButton.interactable = false;
            skillButton.enabled = false;
        }

        // EventTrigger 설정
        SetupEventTrigger();

        UpdateVisuals();
    }

    void SetupEventTrigger()
    {
        // EventTrigger 컴포넌트 추가
        eventTrigger = gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        // 기존 이벤트 정리
        eventTrigger.triggers.Clear();

        // 좌클릭 이벤트
        EventTrigger.Entry leftClickEntry = new EventTrigger.Entry();
        leftClickEntry.eventID = EventTriggerType.PointerClick;
        leftClickEntry.callback.AddListener((data) => {
            PointerEventData pointerData = (PointerEventData)data;
            if (pointerData.button == PointerEventData.InputButton.Left)
            {
                OnLeftClick();
            }
            else if (pointerData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick();
            }
        });
        eventTrigger.triggers.Add(leftClickEntry);

        // ★ 마우스 호버 이벤트 추가 (툴팁용)
        EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
        hoverEntry.eventID = EventTriggerType.PointerEnter;
        hoverEntry.callback.AddListener((data) => { OnHoverEnter(); });
        eventTrigger.triggers.Add(hoverEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnHoverExit(); });
        eventTrigger.triggers.Add(exitEntry);

        Debug.Log($"EventTrigger 설정 완료: {linkedSkill.skillName}");
    }

    void OnLeftClick()
    {
        if (linkedSkill == null || uiManager == null) return;

        Debug.Log($"👆 좌클릭: {linkedSkill.skillName}");
        uiManager.OnSkillNodeClicked(linkedSkill);
    }

    void OnRightClick()
    {
        if (linkedSkill == null || uiManager == null) return;

        Debug.Log($"👆 우클릭: {linkedSkill.skillName}");
        uiManager.OnSkillNodeRightClicked(linkedSkill);
    }

    // ★ 마우스 호버 시 다음 레벨 미리보기 표시
    void OnHoverEnter()
    {
        ShowNextLevelPreview(true);
    }

    void OnHoverExit()
    {
        ShowNextLevelPreview(false);
    }

    public void UpdateVisuals()
    {
        if (linkedSkill == null || uiManager == null) return;

        bool canUpgrade = uiManager.CanUpgradeSkill(linkedSkill);
        bool isLearned = linkedSkill.IsLearned;
        bool canDowngrade = linkedSkill.currentLevel > 0;

        Debug.Log($"UpdateVisuals: {linkedSkill.skillName} - 학습: {isLearned}, 업그레이드: {canUpgrade}");

        // 배경 색상
        if (backgroundCircle != null)
        {
            if (isLearned)
                backgroundCircle.color = learnedColor;
            else if (canUpgrade)
                backgroundCircle.color = availableColor;
            else
                backgroundCircle.color = lockedColor;
        }

        // 레벨 표시
        if (levelText != null && levelBadge != null)
        {
            if (isLearned)
            {
                levelText.text = linkedSkill.currentLevel.ToString();
                levelBadge.SetActive(true);
            }
            else
            {
                levelBadge.SetActive(false);
            }
        }

        // 아이콘 투명도
        if (skillIcon != null)
        {
            Color iconColor = skillIcon.color;
            iconColor.a = isLearned ? 1f : (canUpgrade ? 0.8f : 0.4f);
            skillIcon.color = iconColor;
        }

        // ★ 업그레이드 가능 표시 업데이트
        UpdateUpgradeIndicators(canUpgrade, isLearned);

        // ★ 다음 레벨 정보 준비
        UpdateNextLevelInfo();
    }

    // ★ 업그레이드 가능/불가능 표시 업데이트
    void UpdateUpgradeIndicators(bool canUpgrade, bool isLearned)
    {
        // 업그레이드 가능 표시 (초록 반짝임)
        if (canUpgradeIndicator != null)
        {
            bool shouldShowUpgrade = canUpgrade && !linkedSkill.IsMaxLevel;
            canUpgradeIndicator.SetActive(shouldShowUpgrade);

            if (shouldShowUpgrade)
            {
                // 부드러운 반짝임 애니메이션
                StartGlowAnimation();
            }
        }

        // 업그레이드 불가능 표시 (자물쇠, X 표시 등)
        if (blockedIndicator != null)
        {
            bool shouldShowBlocked = !canUpgrade && !linkedSkill.IsMaxLevel && skillManager.availablePoints > 0;
            blockedIndicator.SetActive(shouldShowBlocked);
        }
    }

    // ★ 다음 레벨 정보 텍스트 업데이트
    void UpdateNextLevelInfo()
    {
        if (nextLevelText == null || linkedSkill.IsMaxLevel) return;

        string nextLevelInfo = GetNextLevelDescription();
        nextLevelText.text = nextLevelInfo;
    }

    // ★ 다음 레벨 설명 생성
    string GetNextLevelDescription()
    {
        if (linkedSkill.IsMaxLevel) return "최대 레벨";

        int nextLevel = linkedSkill.currentLevel + 1;
        string description = $"다음 레벨 ({nextLevel}):\n";

        foreach (var effect in linkedSkill.effects)
        {
            float nextValue = effect.GetValue(nextLevel);
            string effectDesc = GetEffectDescription(effect.effectType, nextValue, effect.isPercentage);
            description += $"• {effectDesc}\n";
        }

        return description.TrimEnd('\n');
    }

    // ★ 이펙트 설명 생성 헬퍼
    string GetEffectDescription(SkillEffectType effectType, float value, bool isPercentage)
    {
        string unit = isPercentage ? "%" : "";

        switch (effectType)
        {
            case SkillEffectType.AttackPower:
                return $"공격력 +{value}{unit}";
            case SkillEffectType.CriticalChance:
                return $"크리티컬 확률 +{value}{unit}";
            case SkillEffectType.CriticalDamage:
                return $"크리티컬 데미지 +{value}{unit}";
            case SkillEffectType.PierceCount:
                return $"관통 횟수 +{(int)value}";
            case SkillEffectType.ExplosiveArrow:
                return "폭발 화살 활성화";
            case SkillEffectType.GuidedArrow:
                return "유도 화살 활성화";
            case SkillEffectType.Health:
                return $"체력 +{value}{unit}";
            default:
                return $"{effectType} +{value}{unit}";
        }
    }

    // ★ 다음 레벨 미리보기 표시/숨김
    void ShowNextLevelPreview(bool show)
    {
        if (nextLevelPreview != null)
        {
            nextLevelPreview.SetActive(show && !linkedSkill.IsMaxLevel);
        }

        if (previewArrow != null)
        {
            previewArrow.gameObject.SetActive(show && !linkedSkill.IsMaxLevel);
        }
    }

    // ★ 반짝임 애니메이션
    void StartGlowAnimation()
    {
        if (canUpgradeIndicator == null) return;

        // LeanTween으로 부드러운 반짝임
        LeanTween.cancel(canUpgradeIndicator);

        Image glowImage = canUpgradeIndicator.GetComponent<Image>();
        if (glowImage != null)
        {
            // 반복되는 페이드 인/아웃 애니메이션
            LeanTween.value(canUpgradeIndicator, 0.3f, 1f, 0.8f)
                     .setOnUpdate((float alpha) => {
                         Color color = canUpgradeGlowColor;
                         color.a = alpha;
                         glowImage.color = color;
                     })
                     .setEaseInOutSine()
                     .setLoopPingPong();
        }
    }

    void OnDestroy()
    {
        if (eventTrigger != null)
        {
            eventTrigger.triggers.Clear();
        }

        // LeanTween 정리
        if (canUpgradeIndicator != null)
        {
            LeanTween.cancel(canUpgradeIndicator);
        }
    }
}