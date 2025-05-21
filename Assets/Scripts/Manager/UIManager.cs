using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 싱글톤 구현
    public static UIManager Instance { get; private set; }

    [Header("UI Prefabs")]
    [SerializeField] private DialogueView dialoguePrefab;
    [SerializeField] private DungeonSelectView dungeonSelectPrefab;
    [SerializeField] private RewardView rewardPrefab;
    [SerializeField] private StatUpgradeView statUpgradePrefab;

    // Presenters
    private DialoguePresenter dialoguePresenter;
    private DungeonSelectPresenter dungeonSelectPresenter;
    private RewardPresenter rewardPresenter;
    private StatUpgradePresenter statUpgradePresenter;

    // Views
    private DialogueView dialogueView;
    private DungeonSelectView dungeonSelectView;
    private RewardView rewardView;
    private StatUpgradeView statUpgradeView;

    // 다른 시스템에 필요한 참조들
    private PlayerDataManager playerDataManager;
    private DungeonManager dungeonManager;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // UI 인스턴스 생성 및 초기화
        InitializeUIComponents();
    }

    private void InitializeUIComponents()
    {
        // 뷰 인스턴스 생성
        dialogueView = Instantiate(dialoguePrefab, transform);
        dungeonSelectView = Instantiate(dungeonSelectPrefab, transform);
        rewardView = Instantiate(rewardPrefab, transform);
        statUpgradeView = Instantiate(statUpgradePrefab, transform);

        // 모델 생성
        DialogueModel dialogueModel = new DialogueModel();
        DungeonSelectModel dungeonSelectModel = new DungeonSelectModel();
        RewardModel rewardModel = new RewardModel();
        StatUpgradeModel statUpgradeModel = new StatUpgradeModel();

        // Presenter 생성 및 연결
        dialoguePresenter = new DialoguePresenter(dialogueView, dialogueModel);
        dungeonSelectPresenter = new DungeonSelectPresenter(dungeonSelectView, dungeonSelectModel);
        rewardPresenter = new RewardPresenter(rewardView, rewardModel);
        statUpgradePresenter = new StatUpgradePresenter(statUpgradeView, statUpgradeModel);

        // 초기에는 모든 UI 숨김
        dialogueView.Hide();
        dungeonSelectView.Hide();
        rewardView.Hide();
        statUpgradeView.Hide();
    }

    // 외부에서 사용할 공개 메서드들
    public void ShowDialogue(string character, string text, Sprite portrait = null)
    {
        // 대화 시스템 호출 메서드
    }

    public void ShowDungeonSelect()
    {
        // 던전 선택 화면 표시 메서드
    }

    public void ShowRewards(List<RewardInfo> rewards)
    {
        // 보상 화면 표시 메서드
    }

    public void ShowStatUpgrade()
    {
        // 스탯 업그레이드 화면 표시 메서드
    }

    // 필요한 경우 추가 메서드들...
}