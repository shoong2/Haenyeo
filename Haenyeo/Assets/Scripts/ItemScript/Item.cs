using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "New Item/item")]
public class Item : ScriptableObject
{
    public string itemName; //아이템 이름
    public ItemType itemType; // 아이템의 유형
    public Sprite itemImage; // 아이템의 이미지
    public GameObject itemPrefab; //아이템의 프리팹
    public int per;  // 출현 확률
    public int spawnTime; // 스폰시간

    public string weaponType;

    public enum ItemType
    {
        Cloth,
        Equipment,
        item
    }

}
