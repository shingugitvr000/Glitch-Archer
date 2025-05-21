using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardPresenter
{
    private IRewardView view;
    private RewardModel model;

    public RewardPresenter(IRewardView view, RewardModel model)
    {
        this.view = view;
        this.model = model;

        // 이벤트 구독
        view.OnContinueClicked += HandleContinueClicked;
        view.OnRewardClicked += HandleRewardClicked;
    }

    private void HandleContinueClicked()
    {
        // 계속 진행 로직 구현
    }

    private void HandleRewardClicked(int rewardIndex)
    {
        // 특정 보상 클릭 로직 구현
    }
}
