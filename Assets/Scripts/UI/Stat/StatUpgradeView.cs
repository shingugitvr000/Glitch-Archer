using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatUpgradeView : MonoBehaviour, IStatUpgradeView
{
    public event Action<StatType> OnStatUpgradeClicked;
    public event Action OnConfirmClicked;
    public event Action OnResetClicked;
    public event Action OnCloseClicked;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdateStatValues(Dictionary<StatType, int> stats)
    {
        // ���� ���� UI�� ������Ʈ�ϴ� ����
    }

    public void UpdateStatCosts(Dictionary<StatType, int> costs)
    {
        // ���� ���׷��̵� ����� UI�� ������Ʈ�ϴ� ����
    }

    public void UpdateAvailablePoints(int points)
    {
        // ��� ������ ����Ʈ�� UI�� ������Ʈ�ϴ� ����
    }

    public void SetUpgradeButtonsState()
    {
        // ���׷��̵� ��ư ���¸� �����ϴ� ����
    }
}