using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Quest/Reward/Item", fileName ="ItemReward_")]
public class ItemReward : Reward
{
    public override void Give(Quest quest)
    {
        if (Inventory.Instance == null) return;
       

        foreach (var it in item)
        {
            if (it == null) continue;
            Inventory.Instance.AcquireItem(it, true, Quantitiy);
        }
    }
}
