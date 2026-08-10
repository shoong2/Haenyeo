using UnityEngine;
using UnityEngine.UI;

public class InventoryTabController : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    [SerializeField] Toggle toggleAll;
    [SerializeField] Toggle toggleClothes;
    [SerializeField] Toggle toggleEquipment;

    void Start()
    {
        toggleAll.onValueChanged.AddListener(on => { if (on) inventory.ShowCategory(null); });
        toggleClothes.onValueChanged.AddListener(on => { if (on) inventory.ShowCategory(Item.ItemType.Cloth); });
        toggleEquipment.onValueChanged.AddListener(on => { if (on) inventory.ShowCategory(Item.ItemType.Equipment); });

        toggleAll.isOn = true; // 기본 선택: 전체
    }
}
