using UnityEngine;
using UnityEngine.UI;

public class InventoryTabController : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    [SerializeField] Toggle toggleAll;
    [SerializeField] Toggle toggleClothes;
    [SerializeField] Toggle toggleEquipment;

    void Awake()
    {
        // 리스너는 한 번만 등록
        toggleAll.onValueChanged.AddListener(on => { if (on) inventory.ShowCategory(null); });
        toggleClothes.onValueChanged.AddListener(on => { if (on) inventory.ShowCategory(Item.ItemType.Cloth); });
        toggleEquipment.onValueChanged.AddListener(on => { if (on) inventory.ShowCategory(Item.ItemType.Equipment); });
    }

    // 인벤토리를 열 때마다 "전체" 탭으로 초기화.
    // 비활성→활성 시 ToggleGroup 등록 순서에 의존하면 장비 탭이 남는 레이스가 있어,
    // 그룹 통지를 거치지 않고 세 토글 상태 + 시각(TabButton) + 내용(ShowCategory)을 직접 맞춘다.
    void OnEnable()
    {
        // 1) 논리 상태 직접 설정 (그룹 통지 없이, 전체만 on)
        toggleAll.SetIsOnWithoutNotify(true);
        toggleClothes.SetIsOnWithoutNotify(false);
        toggleEquipment.SetIsOnWithoutNotify(false);

        // 2) 시각/내용 수동 반영 (전체를 마지막에 켜서 ShowCategory(null)로 마무리)
        toggleClothes.onValueChanged.Invoke(false);
        toggleEquipment.onValueChanged.Invoke(false);
        toggleAll.onValueChanged.Invoke(true);
    }
}
