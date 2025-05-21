using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatUpgradeView
{
    void Show();
    void Hide();
    void UpdateStatValues(Dictionary<StatType, int> stats);
    void UpdateStatCosts(Dictionary<StatType, int> costs);
    void UpdateAvailablePoints(int points);
    void SetUpgradeButtonsState();

    event Action<StatType> OnStatUpgradeClicked;
    event Action OnConfirmClicked;
    event Action OnResetClicked;
    event Action OnCloseClicked;
}