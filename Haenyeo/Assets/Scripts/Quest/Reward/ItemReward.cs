using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Reward/Item", fileName = "ItemReward_")]
public class ItemReward : Reward
{
    [SerializeField]
    Item[] items;

    [SerializeField]
    int quantity = 1;

    public IReadOnlyList<Item> Items => items;
    public int Quantity => quantity;

    public override void Give(Quest quest)
    {
        if (Inventory.Instance == null || items == null)
            return;

        foreach (var item in items)
            if (item != null)
                Inventory.Instance.AcquireItem(item, true, quantity);
    }

    public override IReadOnlyList<Sprite> GetIcons()
    {
        if (items == null)
            return System.Array.Empty<Sprite>();

        var icons = new List<Sprite>(items.Length);
        foreach (var item in items)
            if (item != null && item.itemImage != null)
                icons.Add(item.itemImage);

        return icons;
    }
}
