using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardInfo
{
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public Sprite ItemIcon { get; set; }
    public int Quantity { get; set; }
    public ItemRarity Rarity { get; set; }
}
