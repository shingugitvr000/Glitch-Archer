using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatUpgradeModel
{
    public Dictionary<StatType, int> CurrentStats { get; set; } = new Dictionary<StatType, int>();
    public Dictionary<StatType, int> StatCosts { get; set; } = new Dictionary<StatType, int>();
    public int AvailablePoints { get; set; }
}
