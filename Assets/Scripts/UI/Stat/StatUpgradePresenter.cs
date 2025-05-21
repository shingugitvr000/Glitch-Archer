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

        // 이벤트 구독
        view.OnStatUpgradeClicked += HandleStatUpgradeClicked;
        view.OnConfirmClicked += HandleConfirmClicked;
        view.OnResetClicked += HandleResetClicked;
        view.OnCloseClicked += HandleCloseClicked;
    }

    private void HandleStatUpgradeClicked(StatType statType)
    {
        // 특정 스탯 업그레이드 로직 구현
    }

    private void HandleConfirmClicked()
    {
        // 업그레이드 확정 로직 구현
    }

    private void HandleResetClicked()
    {
        // 초기화 로직 구현
    }

    private void HandleCloseClicked()
    {
        // 닫기 로직 구현
    }
}