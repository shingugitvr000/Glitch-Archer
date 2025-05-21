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
        // 스탯 값을 UI에 업데이트하는 구현
    }

    public void UpdateStatCosts(Dictionary<StatType, int> costs)
    {
        // 스탯 업그레이드 비용을 UI에 업데이트하는 구현
    }

    public void UpdateAvailablePoints(int points)
    {
        // 사용 가능한 포인트를 UI에 업데이트하는 구현
    }

    public void SetUpgradeButtonsState()
    {
        // 업그레이드 버튼 상태를 설정하는 구현
    }
}