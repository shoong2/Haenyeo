using UnityEngine;
using UnityEngine.UI;

// 옷장(인벤토리) 좌측 상단의 도구 자리.
// 장착 중인 Equipment 아이템 아이콘을 왼쪽부터 순서대로 채운다.
//
// 프레임(Box)의 Image가 아니라 그 안의 아이콘 Image를 넘겨야 한다.
// 프레임 스프라이트를 덮어쓰면 테두리가 사라진다.
public class ToolSlotBar : MonoBehaviour
{
    [Tooltip("각 도구 칸(Box) 안에 있는 아이콘 Image. 프레임 자체가 아니다")]
    [SerializeField]
    Image[] icons;

    void OnEnable()
    {
        var state = EquipmentState.Instance;
        if (state != null)
            state.onChanged += Refresh;

        Refresh();
    }

    void OnDisable()
    {
        var state = EquipmentState.Instance;
        if (state != null)
            state.onChanged -= Refresh;
    }

    void Refresh()
    {
        if (icons == null || icons.Length == 0)
        {
            Debug.LogWarning("[ToolSlotBar] icons 가 비어 있습니다", this);
            return;
        }

        int filled = 0;
        var state = EquipmentState.Instance;

        if (state != null)
        {
            foreach (var item in state.Equipped)
            {
                if (item == null || item.itemType != Item.ItemType.Equipment)
                    continue;

                if (filled >= icons.Length)
                    break;

                SetIcon(icons[filled], item.itemImage);
                filled++;
            }
        }

        // 남은 칸은 비운다
        for (int i = filled; i < icons.Length; i++)
            SetIcon(icons[i], null);
    }

    static void SetIcon(Image image, Sprite sprite)
    {
        if (image == null)
            return;

        image.sprite = sprite;
        image.enabled = sprite != null;   // 아이콘만 숨긴다. 프레임은 그대로 남는다
    }
}
