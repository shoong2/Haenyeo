using UnityEngine;
using UnityEngine.UI;

// 씬(Room)에 있는 버튼으로 영속 인벤토리 패널을 연다.
//
// 인벤토리는 Bootstrapper가 런타임에 만드는 DontDestroy 프리팹 안에 있어서
// 씬 버튼의 OnClick으로는 인스펙터에서 가리킬 수 없다. 그래서 코드로 연결한다.
[RequireComponent(typeof(Button))]
public class OpenInventoryButton : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        var inventory = Inventory.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("[OpenInventoryButton] Inventory.Instance가 없습니다", this);
            return;
        }

        inventory.Open();
    }
}
