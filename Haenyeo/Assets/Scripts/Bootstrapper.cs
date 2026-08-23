using UnityEngine;

// 어느 씬에서 플레이를 시작하든 영속 매니저들이 먼저 준비되게 한다.
// (기존에는 Room.unity의 DontDestroy 오브젝트에 얹혀 있어서, Room을 거치지 않으면
//  GameManager / Inventory / SoundManager 가 아예 없었다)
//
// RuntimeInitializeOnLoadMethod(BeforeSceneLoad)는 플레이 세션당 딱 한 번,
// 첫 씬이 로드되기 전에 실행된다. 그래서 중복 생성이 없다.
//
// 씬을 additive로 올리지 않고 프리팹을 Instantiate 하는 이유:
// Instantiate는 동기라서 프리팹 안의 Awake가 첫 씬의 어떤 Awake보다도 먼저 끝난다.
// SceneManager.LoadScene(Additive)는 완료가 한 프레임 늦어질 수 있어서,
// GameManager.instance를 Start에서 null 확인 없이 쓰는 곳(RoomWindow 등)이 터진다.
public static class Bootstrapper
{
    // Assets/Resources/DontDestroy.prefab
    const string PrefabPath = "DontDestroy";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        var prefab = Resources.Load<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[Bootstrapper] Assets/Resources/{PrefabPath}.prefab 을 찾을 수 없습니다.");
            return;
        }

        var instance = Object.Instantiate(prefab);
        instance.name = prefab.name;            // 이름 뒤에 "(Clone)"이 붙지 않도록
        Object.DontDestroyOnLoad(instance);
    }
}
