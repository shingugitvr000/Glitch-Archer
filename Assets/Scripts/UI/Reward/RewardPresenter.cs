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

        // �̺�Ʈ ����
        view.OnContinueClicked += HandleContinueClicked;
        view.OnRewardClicked += HandleRewardClicked;
    }

    private void HandleContinueClicked()
    {
        // ��� ���� ���� ����
    }

    private void HandleRewardClicked(int rewardIndex)
    {
        // Ư�� ���� Ŭ�� ���� ����
    }
}
