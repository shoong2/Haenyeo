using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 옷/장비 상세 정보 패널 (슬롯 클릭 시 출력)
public class ItemDetailPanel : MonoBehaviour
{
    public static ItemDetailPanel Instance { get; private set; }

    [Header("루트 (켜고/끄는 대상)")]
    [SerializeField] GameObject go_Panel;       // 실제로 보이고 숨겨지는 카드 오브젝트

    [Header("기본 정보")]
    [SerializeField] Image iconImage;           // 아이템 아이콘(미리보기)
    [SerializeField] TMP_Text nameText;         // 의상 이름
    [SerializeField] TMP_Text descText;         // 의상 설명

    [Header("보유 효과")]
    [SerializeField] Transform effectListParent;    // 효과 줄들이 들어갈 부모 (Vertical Layout Group)
    [SerializeField] EffectRowUI effectRowPrefab;   // 효과 한 줄 프리팹

    [Header("장착/해제 버튼")]
    [SerializeField] Button equipButton;            // [장착하기] 버튼 (미장착 상태에서 표시)
    [SerializeField] Button unequipButton;          // [해제하기] 버튼 (장착 상태에서 표시)

    // 생성해 둔 효과 줄들 (다음 아이템 표시 전에 정리용)
    readonly List<EffectRowUI> spawnedRows = new List<EffectRowUI>();

    // 현재 패널에 표시 중인 아이템 (장착 버튼용)
    Item currentItem;

    void Awake()
    {
        Instance = this;
        if (equipButton != null)
            equipButton.onClick.AddListener(OnEquip);
        if (unequipButton != null)
            unequipButton.onClick.AddListener(OnUnequip);
        Hide();     // 시작 시 숨김
    }

    // 슬롯이 넘겨준 아이템 정보를 패널에 채워 출력
    public void Show(Item item)
    {
        if (item == null) return;

        currentItem = item;
        go_Panel.SetActive(true);

        nameText.text = item.itemName;
        iconImage.sprite = item.itemImage;

        // 옷/장비(EquipItem)일 때만 설명 + 보유 효과 출력
        EquipItem equip = item as EquipItem;
        if (equip != null)
        {
            descText.text = equip.description;
            BuildEffects(equip.effects);
        }
        else
        {
            descText.text = string.Empty;
            BuildEffects(null);
        }

        RefreshEquipButton();
    }

    // [장착하기] 클릭
    void OnEquip()
    {
        EquipItem equip = currentItem as EquipItem;
        if (equip == null || CharacterEquipper.Instance == null) return;

        CharacterEquipper.Instance.Equip(equip);
        RefreshEquipButton();
        go_Panel.SetActive(false);
    }

    // [해제하기] 클릭
    void OnUnequip()
    {
        EquipItem equip = currentItem as EquipItem;
        if (equip == null || CharacterEquipper.Instance == null) return;

        CharacterEquipper.Instance.Unequip(equip);
        RefreshEquipButton();
        go_Panel.SetActive(false);
    }

    // 현재 아이템의 장착 상태에 맞춰 두 버튼을 켜고 끔
    void RefreshEquipButton()
    {
        EquipItem equip = currentItem as EquipItem;
        bool isEquip = equip != null;
        bool isEquipped = isEquip && CharacterEquipper.Instance != null && CharacterEquipper.Instance.IsEquipped(equip);

        // 미장착 → [장착하기]만, 장착됨 → [해제하기]만, 옷/장비 아님 → 둘 다 숨김
        if (equipButton != null)   equipButton.gameObject.SetActive(isEquip && !isEquipped);
        if (unequipButton != null) unequipButton.gameObject.SetActive(isEquip && isEquipped);
    }

    public void Hide()
    {
        if (go_Panel != null)
            go_Panel.SetActive(false);
    }

    // 효과 목록을 프리팹으로 개수만큼 생성
    void BuildEffects(StatEffect[] effects)
    {
        ClearRows();
        if (effects == null) return;

        for (int i = 0; i < effects.Length; i++)
        {
            EffectRowUI row = Instantiate(effectRowPrefab, effectListParent);
            row.Set(effects[i]);
            spawnedRows.Add(row);
        }
    }

    // 이전에 생성한 효과 줄 제거
    void ClearRows()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (spawnedRows[i] != null)
                Destroy(spawnedRows[i].gameObject);
        }
        spawnedRows.Clear();
    }
}
