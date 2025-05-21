using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonInfo
{
    public int DungeonId { get; set; }
    public string DungeonName { get; set; }
    public string Description { get; set; }
    public Sprite DungeonImage { get; set; }
    public int RequiredLevel { get; set; }
    public bool IsLocked { get; set; }
    public List<DungeonStageInfo> Stages { get; set; } = new List<DungeonStageInfo>();
}
