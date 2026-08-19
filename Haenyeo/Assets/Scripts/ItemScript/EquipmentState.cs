using System.Collections.Generic;
using UnityEngine;

// 씬을 넘어 유지되는 '무엇을 착용 중인가' 데이터.
// 보이는 것(스프라이트 스왑)은 씬마다 새로 생기는 CharacterEquipper가 담당하고,
// 이쪽은 데이터만 들고 있는다.
//
// 씬에 배치하지 않는다. 처음 접근할 때 스스로 만들어지고 DontDestroyOnLoad로 살아남는다.
// (QuestSystem과 같은 방식. 씬에 배치하지 않으므로 씬을 다시 로드해도 사본이 생기지 않는다)
public class EquipmentState : MonoBehaviour
{
    static EquipmentState instance;
    static bool isApplicationQuitting;

    public static EquipmentState Instance
    {
        get
        {
            if (!isApplicationQuitting && instance == null)
            {
                instance = FindObjectOfType<EquipmentState>();
                if (instance == null)
                {
                    instance = new GameObject("Equipment State").AddComponent<EquipmentState>();
                    DontDestroyOnLoad(instance.gameObject);
                }
            }
            return instance;
        }
    }

    readonly List<EquipItem> equipped = new List<EquipItem>();

    public IReadOnlyList<EquipItem> Equipped => equipped;

    // 착용 목록이 바뀔 때 발생 (나중에 스탯 적용 등에서 쓸 수 있다)
    public event System.Action onChanged;

    void OnApplicationQuit() => isApplicationQuitting = true;

    public bool IsEquipped(EquipItem item) => item != null && equipped.Contains(item);

    // CharacterEquipper가 바뀔 때마다 현재 목록을 통째로 넘긴다.
    // (부분 갱신보다 어긋날 여지가 적다)
    public void Set(IReadOnlyList<EquipItem> items)
    {
        equipped.Clear();

        if (items != null)
        {
            foreach (var item in items)
                if (item != null)
                    equipped.Add(item);
        }

        onChanged?.Invoke();
    }

    public void Clear() => Set(null);
}
