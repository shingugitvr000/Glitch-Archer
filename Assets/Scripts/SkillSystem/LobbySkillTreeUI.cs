using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class LobbySkillTreeUI : MonoBehaviour
{
    [Header("UI 패널")]
    public GameObject skillTreePanel;
    public GameObject lobbyPanel;
    public Button openSkillTreeButton;
    public Button closeSkillTreeButton;
    public Button resetSkillsButton;
    public Button enterDungeonButton;

    [Header("포인트 표시")]
    public TextMeshProUGUI availablePointsText;
    public TextMeshProUGUI usedPointsText;
    public TextMeshProUGUI playerLevelText;

    [Header("티어 컨테이너")]
    public Transform tier1Container;
    public Transform tier2Container;
    public Transform tier3Container;
    public Transform tier4Container;

    [Header("스킬 노드 프리팹")]
    public GameObject skillNodePrefab;

    [Header("던전 씬")]
    public string dungeonSceneName = "DungeonScene";

    private ArrowSkillTreeManager skillManager;
    private Dictionary<int, SkillNodeUI> skillNodes = new Dictionary<int, SkillNodeUI>();

    void Start()
    {
        skillManager = FindObjectOfType<ArrowSkillTreeManager>();
        if (skillManager == null)
        {
            Debug.LogError("ArrowSkillTreeManager를 찾을 수 없습니다!");
            return;
        }

        SetupUI();
        CreateSkillNodes();
        UpdateUI();

        // 이벤트 구독
        ArrowSkillTreeManager.OnSkillTreeChanged += UpdateUI;
    }

    void OnDestroy()
    {
        ArrowSkillTreeManager.OnSkillTreeChanged -= UpdateUI;
    }

    void SetupUI()
    {
        if (openSkillTreeButton != null)
        {
            openSkillTreeButton.onClick.RemoveAllListeners();
            openSkillTreeButton.onClick.AddListener(OpenSkillTree);
        }

        if (closeSkillTreeButton != null)
        {
            closeSkillTreeButton.onClick.RemoveAllListeners();
            closeSkillTreeButton.onClick.AddListener(CloseSkillTree);
        }

        if (resetSkillsButton != null)
        {
            resetSkillsButton.onClick.RemoveAllListeners();
            resetSkillsButton.onClick.AddListener(ResetSkills);
        }

        if (enterDungeonButton != null)
        {
            enterDungeonButton.onClick.RemoveAllListeners();
            enterDungeonButton.onClick.AddListener(EnterDungeon);
        }

        // 초기 상태
        if (skillTreePanel != null)
            skillTreePanel.SetActive(false);

        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);
    }

    void CreateSkillNodes()
    {
        if (skillManager == null || skillNodePrefab == null) return;

        CreateNodesForTier(SkillTier.Tier1, tier1Container);
        CreateNodesForTier(SkillTier.Tier2, tier2Container);
        CreateNodesForTier(SkillTier.Tier3, tier3Container);
        CreateNodesForTier(SkillTier.Tier4, tier4Container);
    }

    void CreateNodesForTier(SkillTier tier, Transform container)
    {
        if (container == null) return;

        var skillsInTier = skillManager.GetSkillsByTier(tier);

        foreach (var skill in skillsInTier)
        {
            GameObject nodeObj = Instantiate(skillNodePrefab, container);
            SkillNodeUI nodeUI = nodeObj.GetComponent<SkillNodeUI>();

            if (nodeUI != null)
            {
                nodeUI.Initialize(skill, this);
                skillNodes[skill.skillId] = nodeUI;
            }
        }
    }

    public void OnSkillNodeClicked(ArrowSkillData skill)
    {
        Debug.Log($"★★★ 스킬 업그레이드 요청: {skill.skillName} ★★★");

        if (skillManager.TryUpgradeSkill(skill.skillId))
        {
            Debug.Log($"✅ 업그레이드 성공!");
            UpdateUI();
            PlayUpgradeEffect(skill);
        }
        else
        {
            Debug.Log($"❌ 업그레이드 실패!");
            ShowUpgradeFailReason(skill);
        }
    }

    public void OnSkillNodeRightClicked(ArrowSkillData skill)
    {
        Debug.Log($"★★★ 스킬 다운그레이드 요청: {skill.skillName} ★★★");

        if (skillManager.TryDowngradeSkill(skill.skillId))
        {
            Debug.Log($"✅ 다운그레이드 성공!");
            UpdateUI();
            PlayDowngradeEffect(skill);
        }
        else
        {
            Debug.Log($"❌ 다운그레이드 실패!");
            ShowDowngradeFailReason(skill);
        }
    }

    public bool CanUpgradeSkill(ArrowSkillData skill)
    {
        return skillManager.CanUpgradeSkill(skill);
    }

    void UpdateUI()
    {
        if (skillManager == null) return;

        // 포인트 정보 업데이트
        if (availablePointsText != null)
            availablePointsText.text = skillManager.availablePoints.ToString();

        if (usedPointsText != null)
            usedPointsText.text = skillManager.GetTotalPointsUsed().ToString();

        if (playerLevelText != null)
            playerLevelText.text = skillManager.playerLevel.ToString();

        // 모든 스킬 노드 시각적 업데이트
        foreach (var nodeKVP in skillNodes)
        {
            nodeKVP.Value.UpdateVisuals();
        }
    }

    void PlayUpgradeEffect(ArrowSkillData skill)
    {
        if (skillNodes.ContainsKey(skill.skillId))
        {
            var nodeObj = skillNodes[skill.skillId].gameObject;
            var backgroundImage = nodeObj.GetComponentInChildren<Image>();

            // 스케일 펀치 효과
            LeanTween.scale(nodeObj, Vector3.one * 1.3f, 0.15f)
                     .setEaseOutBack()
                     .setOnComplete(() => {
                         LeanTween.scale(nodeObj, Vector3.one, 0.2f).setEaseOutQuad();
                     });

            // 색상 플래시 효과 (배경이 있다면)
            if (backgroundImage != null)
            {
                Color originalColor = backgroundImage.color;
                LeanTween.value(nodeObj, 0f, 1f, 0.1f)
                         .setOnUpdate((float t) => {
                             backgroundImage.color = Color.Lerp(originalColor, Color.green, t * 0.5f);
                         })
                         .setOnComplete(() => {
                             LeanTween.value(nodeObj, 1f, 0f, 0.2f)
                                      .setOnUpdate((float t) => {
                                          backgroundImage.color = Color.Lerp(originalColor, Color.green, t * 0.5f);
                                      });
                         });
            }
        }

        Debug.Log($"✨ {skill.skillName} 레벨업!");
    }

    void PlayDowngradeEffect(ArrowSkillData skill)
    {
        if (skillNodes.ContainsKey(skill.skillId))
        {
            var nodeObj = skillNodes[skill.skillId].gameObject;
            var backgroundImage = nodeObj.GetComponentInChildren<Image>();

            // 다운그레이드 전용 애니메이션: 빨갛게 깜빡이면서 작아졌다 커지기
            LeanTween.scale(nodeObj, Vector3.one * 0.6f, 0.1f)
                     .setEaseInQuad()
                     .setOnComplete(() => {
                         LeanTween.scale(nodeObj, Vector3.one * 1.1f, 0.1f)
                                  .setEaseOutBack()
                                  .setOnComplete(() => {
                                      LeanTween.scale(nodeObj, Vector3.one, 0.15f).setEaseOutQuad();
                                  });
                     });

            // 빨간색 강한 플래시 (다운그레이드는 더 눈에 띄게)
            if (backgroundImage != null)
            {
                Color originalColor = backgroundImage.color;

                // 빠르게 빨갛게
                LeanTween.value(nodeObj, 0f, 1f, 0.05f)
                         .setOnUpdate((float t) => {
                             backgroundImage.color = Color.Lerp(originalColor, Color.red, t * 0.7f);
                         })
                         .setOnComplete(() => {
                             // 천천히 원래 색으로
                             LeanTween.value(nodeObj, 1f, 0f, 0.25f)
                                      .setOnUpdate((float t) => {
                                          backgroundImage.color = Color.Lerp(originalColor, Color.red, t * 0.7f);
                                      });
                         });
            }
        }

        Debug.Log($"⬇️ {skill.skillName} 레벨 다운!");
    }

    void ShowDowngradeFailReason(ArrowSkillData skill)
    {
        string reason = "";

        if (skill.currentLevel <= 0)
        {
            reason = "더 이상 내릴 수 없습니다.";
        }
        else
        {
            foreach (var otherSkill in skillManager.allSkills)
            {
                if (otherSkill.IsLearned && System.Array.Exists(otherSkill.prerequisiteSkills, prereq => prereq == skill.skillId))
                {
                    reason = $"'{otherSkill.skillName}' 스킬이 이 스킬에 의존합니다.";
                    break;
                }
            }
        }

        // 팝업 대신 Debug.Log 사용
        Debug.LogWarning($"❌ 스킬 다운그레이드 불가: {reason}");
    }

    void ShowUpgradeFailReason(ArrowSkillData skill)
    {
        string reason = "";

        if (skillManager.availablePoints <= 0)
            reason = "사용 가능한 포인트가 없습니다.";
        else if (skill.IsMaxLevel)
            reason = "이미 최대 레벨입니다.";
        else if (skillManager.GetTotalPointsUsed() < skill.requiredTreePoints)
            reason = $"트리에 {skill.requiredTreePoints}포인트가 필요합니다.";
        else
        {
            foreach (int prereqId in skill.prerequisiteSkills)
            {
                var prereqSkill = skillManager.GetSkill(prereqId);
                if (prereqSkill == null || !prereqSkill.IsLearned)
                {
                    reason = $"선행 스킬 '{prereqSkill?.skillName}'이 필요합니다.";
                    break;
                }
            }
        }

        // 팝업 대신 Debug.Log 사용
        Debug.LogWarning($"❌ 스킬 습득 불가: {reason}");
    }

    public void OpenSkillTree()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);

        if (skillTreePanel != null)
        {
            skillTreePanel.SetActive(true);
            UpdateUI();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("스킬 트리 열림");
    }

    public void CloseSkillTree()
    {
        if (skillTreePanel != null)
            skillTreePanel.SetActive(false);

        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);

        Debug.Log("스킬 트리 닫힘");
    }

    public void ResetSkills()
    {
        skillManager.ResetAllSkills();
        UpdateUI();
        Debug.Log("✅ 스킬 리셋 완료");
    }

    public void EnterDungeon()
    {
        // ★ 던전 진입 전에 스킬 데이터 저장
        SaveSkillsBeforeEnteringDungeon();
        LoadDungeonScene();
    }

    // ★ 던전 진입 전 스킬 데이터 저장
    void SaveSkillsBeforeEnteringDungeon()
    {
        if (skillManager == null) return;

        // SkillPersistenceManager가 없으면 생성
        if (SkillPersistenceManager.Instance == null)
        {
            GameObject persistObj = new GameObject("SkillPersistenceManager");
            persistObj.AddComponent<SkillPersistenceManager>();
        }

        // 현재 스킬 상태 저장
        SkillPersistenceManager.Instance.SaveSkillData(skillManager);

        Debug.Log("✅ 던전 진입 전 스킬 데이터 저장 완료");
    }

    void LoadDungeonScene()
    {
        // ★ 기존 스킬 매니저는 DontDestroyOnLoad 하지 않음 (중복 방지)
        Debug.Log("던전 씬으로 이동합니다...");
        SceneManager.LoadScene(dungeonSceneName);
    }

}