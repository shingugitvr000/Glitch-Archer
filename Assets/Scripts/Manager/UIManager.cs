using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // �̱��� ����
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

    // �ٸ� �ý��ۿ� �ʿ��� ������
    private PlayerDataManager playerDataManager;
    private DungeonManager dungeonManager;

    private void Awake()
    {
        // �̱��� ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // UI �ν��Ͻ� ���� �� �ʱ�ȭ
        InitializeUIComponents();
    }

    private void InitializeUIComponents()
    {
        // �� �ν��Ͻ� ����
        dialogueView = Instantiate(dialoguePrefab, transform);
        dungeonSelectView = Instantiate(dungeonSelectPrefab, transform);
        rewardView = Instantiate(rewardPrefab, transform);
        statUpgradeView = Instantiate(statUpgradePrefab, transform);

        // �� ����
        DialogueModel dialogueModel = new DialogueModel();
        DungeonSelectModel dungeonSelectModel = new DungeonSelectModel();
        RewardModel rewardModel = new RewardModel();
        StatUpgradeModel statUpgradeModel = new StatUpgradeModel();

        // Presenter ���� �� ����
        dialoguePresenter = new DialoguePresenter(dialogueView, dialogueModel);
        dungeonSelectPresenter = new DungeonSelectPresenter(dungeonSelectView, dungeonSelectModel);
        rewardPresenter = new RewardPresenter(rewardView, rewardModel);
        statUpgradePresenter = new StatUpgradePresenter(statUpgradeView, statUpgradeModel);

        // �ʱ⿡�� ��� UI ����
        dialogueView.Hide();
        dungeonSelectView.Hide();
        rewardView.Hide();
        statUpgradeView.Hide();
    }

    // �ܺο��� ����� ���� �޼����
    public void ShowDialogue(string character, string text, Sprite portrait = null)
    {
        // ��ȭ �ý��� ȣ�� �޼���
    }

    public void ShowDungeonSelect()
    {
        // ���� ���� ȭ�� ǥ�� �޼���
    }

    public void ShowRewards(List<RewardInfo> rewards)
    {
        // ���� ȭ�� ǥ�� �޼���
    }

    public void ShowStatUpgrade()
    {
        // ���� ���׷��̵� ȭ�� ǥ�� �޼���
    }

    // �ʿ��� ��� �߰� �޼����...
}