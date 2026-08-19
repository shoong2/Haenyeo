using UnityEngine;
using UnityEngine.UI;

// 버튼에 붙이면 클릭 시 UI 클릭음을 낸다.
//
// SoundManager는 Bootstrapper가 런타임에 만드는 DontDestroy 프리팹 안에 있어서
// 씬 버튼의 OnClick으로는 인스펙터에서 가리킬 수 없다. 그래서 싱글톤으로 찾는다.
[RequireComponent(typeof(Button))]
public class UIClickSound : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (SoundManager.instance != null)
            SoundManager.instance.UIClick();
    }
}
