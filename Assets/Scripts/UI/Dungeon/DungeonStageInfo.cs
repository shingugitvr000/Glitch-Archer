using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonStageInfo
{
    public int StageId { get; set; }
    public string StageName { get; set; }
    public int Difficulty { get; set; }
    public List<RewardInfo> PossibleRewards { get; set; } = new List<RewardInfo>();
    public bool IsLocked { get; set; }
}
