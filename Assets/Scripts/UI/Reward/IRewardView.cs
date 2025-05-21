using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRewardView
{
    void Show();
    void Hide();
    void SetRewards(List<RewardInfo> rewards);
    void SetSpecialReward(RewardInfo specialReward);
    void PlayRewardAnimation();

    event Action OnContinueClicked;
    event Action<int> OnRewardClicked;
}
