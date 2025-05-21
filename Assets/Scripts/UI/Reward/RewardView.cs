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
        // 보상 목록을 UI에 설정하는 구현
    }

    public void SetSpecialReward(RewardInfo specialReward)
    {
        // 특별 보상을 UI에 설정하는 구현
    }

    public void PlayRewardAnimation()
    {
        // 보상 애니메이션을 재생하는 구현
    }
}