using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class RewardView : MonoBehaviour, IRewardView
{
    public event Action OnContinueClicked;
    public event Action<int> OnRewardClicked;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetRewards(List<RewardInfo> rewards)
    {
        // ���� ����� UI�� �����ϴ� ����
    }

    public void SetSpecialReward(RewardInfo specialReward)
    {
        // Ư�� ������ UI�� �����ϴ� ����
    }

    public void PlayRewardAnimation()
    {
        // ���� �ִϸ��̼��� ����ϴ� ����
    }
}