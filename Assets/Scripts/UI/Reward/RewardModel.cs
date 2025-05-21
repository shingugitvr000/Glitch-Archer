using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardModel
{
    public List<RewardInfo> Rewards { get; set; } = new List<RewardInfo>();
    public bool HasSpecialReward { get; set; }
    public RewardInfo SpecialReward { get; set; }
}
