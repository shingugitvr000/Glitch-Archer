using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatUpgradePresenter
{
    private IStatUpgradeView view;
    private StatUpgradeModel model;

    public StatUpgradePresenter(IStatUpgradeView view, StatUpgradeModel model)
    {
        this.view = view;
        this.model = model;

        // �̺�Ʈ ����
        view.OnStatUpgradeClicked += HandleStatUpgradeClicked;
        view.OnConfirmClicked += HandleConfirmClicked;
        view.OnResetClicked += HandleResetClicked;
        view.OnCloseClicked += HandleCloseClicked;
    }

    private void HandleStatUpgradeClicked(StatType statType)
    {
        // Ư�� ���� ���׷��̵� ���� ����
    }

    private void HandleConfirmClicked()
    {
        // ���׷��̵� Ȯ�� ���� ����
    }

    private void HandleResetClicked()
    {
        // �ʱ�ȭ ���� ����
    }

    private void HandleCloseClicked()
    {
        // �ݱ� ���� ����
    }
}