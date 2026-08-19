using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    //���� �θ�
    //[SerializeField] GameObject go_SlotsParent;

    // 여닫을 패널 본체. 이 스크립트는 항상 활성인 오브젝트에 두고 이것만 껐다 켠다
    // (비활성 오브젝트는 Awake가 안 돌아서 Instance가 null이 되는 함정 때문)
    [SerializeField] GameObject go_Panel;

    [SerializeField] GameObject go_AllSlotsParent;
    [SerializeField] GameObject go_ClothesSlotsParent;
    [SerializeField] GameObject go_EquipmentSlotsParent;

    //���Ե�
    //Slot[] slots;
    Slot[] allSlots;
    Slot[] clothesSlots;
    Slot[] equipmentSlots;

    public Slot[] GetSlots() { return allSlots; }

    [SerializeField] Item[] items;

    private void Awake()
    {
        Instance = this;
        allSlots = go_AllSlotsParent.GetComponentsInChildren<Slot>(true);
        clothesSlots = go_ClothesSlotsParent.GetComponentsInChildren<Slot>(true);
        equipmentSlots = go_EquipmentSlotsParent.GetComponentsInChildren<Slot>(true);
    }


    private void Start()
    {
        //SaveNLoad.instance.LoadData();
        Debug.Log("start");
    }



    public void AcquireItem(Item _item, bool quest, int _count = 1)
    {
        AddToArray(allSlots, _item, _count);

        Slot[] categorySlots = GetSlotsByType(_item.itemType);
        if (categorySlots != null)
            AddToArray(categorySlots, _item, _count);

    }

    void AddToArray(Slot[] targetSlots, Item _item, int _count)
    {
        if (Item.ItemType.Equipment != _item.itemType)
        {
            for (int i = 0; i < targetSlots.Length; i++)
            {
                if (targetSlots[i].item != null && targetSlots[i].item.itemName == _item.itemName)
                {
                    targetSlots[i].SetSlotCount(_count);
                    return;
                }
            }
        }
        for (int i = 0; i < targetSlots.Length; i++)
        {
            if (targetSlots[i].item == null)
            {
                targetSlots[i].AddItem(_item, _count);
                return;
            }
        }
    }

    Slot[] GetSlotsByType(Item.ItemType type)
    {
        switch (type)
        {
            case Item.ItemType.Cloth: return clothesSlots;
            case Item.ItemType.Equipment: return equipmentSlots;
            default: return null; // Tool ���� ī�װ��� �迭 ����
        }
    }


    // ===== �� ��ȯ =====
    public void ShowCategory(Item.ItemType? category)
    {
        go_AllSlotsParent.SetActive(category == null);
        go_ClothesSlotsParent.SetActive(category == Item.ItemType.Cloth);
        go_EquipmentSlotsParent.SetActive(category == Item.ItemType.Equipment);
    }

    // ===== 여닫기 =====
    // 씬에 있는 버튼은 이 프리팹 안을 인스펙터로 가리킬 수 없으므로,
    // Inventory.Instance를 통해 코드로 호출한다. (OpenInventoryButton 참고)
    public void Open() => SetPanel(true);

    public void Close() => SetPanel(false);

    public void Toggle() => SetPanel(go_Panel == null || !go_Panel.activeSelf);

    void SetPanel(bool on)
    {
        if (go_Panel == null)
        {
            Debug.LogWarning("[Inventory] go_Panel이 연결되지 않았습니다", this);
            return;
        }
        go_Panel.SetActive(on);
    }

    // ===== ����/�ε� ���� (���� �� ������ ȣȯ ����) =====
    public void LoadToInven(int _arrayNum, string _itemName, int _itemNum)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemName == _itemName)
            {
                allSlots[_arrayNum].AddItem(items[i], _itemNum);

                Slot[] categorySlots = GetSlotsByType(items[i].itemType);
                if (categorySlots != null)
                    AddToArray(categorySlots, items[i], _itemNum);
            }
        }
    }

}