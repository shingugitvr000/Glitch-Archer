using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSelectModel
{
    public List<DungeonInfo> AvailableDungeons { get; set; } = new List<DungeonInfo>();
    public int SelectedDungeonId { get; set; } = -1;
}