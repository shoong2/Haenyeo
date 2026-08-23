using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

// 캐릭터(플레이어)에 부착. 옷/장비 스프라이트 스왑 적용/해제 + 장착 상태 관리.
// 실제 플레이어 + (선택) 인벤토리 미리보기 복제본을 동시에 스왑한다.
public class CharacterEquipper : MonoBehaviour
{
    public static CharacterEquipper Instance { get; private set; }

    const string DefaultLabel = "default";   // 해제 시 되돌릴 기본 라벨 (clothes.spriteLib 기준)

    [Header("미리보기 (선택)")]
    [SerializeField] Transform previewRoot;  // 인벤토리 미리보기 캐릭터 복제본의 루트 (없으면 비워둠)

    // 부위 category 이름 -> 해당 SpriteResolver들 (실제 + 미리보기 함께). 스왑을 쓰는 쪽.
    readonly Dictionary<string, List<SpriteResolver>> resolvers = new Dictionary<string, List<SpriteResolver>>();
    // 부위 category 이름 -> 실제 플레이어의 SpriteResolver. 현재 라벨을 읽는 쪽 (미리보기 값이 섞이면 안 되므로 분리)
    readonly Dictionary<string, SpriteResolver> playerResolvers = new Dictionary<string, SpriteResolver>();
    // 현재 장착 중인 아이템들. 건드리는 부위가 겹치지 않으면 여러 벌이 동시에 남는다.
    readonly List<EquipItem> equipped = new List<EquipItem>();

    // 장착/해제로 캐릭터 겉모습이 바뀔 때마다 발생
    public event System.Action onEquipChanged;

    public IReadOnlyList<EquipItem> Equipped => equipped;

    // 복원 중에는 EquipmentState에 되쓰지 않는다 (순회 중 수정 방지)
    bool isRestoring;

    void Awake()
    {
        Instance = this;

        CacheResolvers(transform);                                   // 실제 플레이어
        if (previewRoot != null && !previewRoot.IsChildOf(transform))
            CacheResolvers(previewRoot);                             // 미리보기가 캐릭터 바깥에 있을 때만 따로
    }

    // 씬을 다시 들어왔을 때 이전에 입고 있던 것을 그대로 다시 입힌다
    void Start()
    {
        var state = EquipmentState.Instance;
        if (state == null || state.Equipped.Count == 0)
            return;

        var saved = new List<EquipItem>(state.Equipped);

        isRestoring = true;
        foreach (var item in saved)
            Equip(item);
        isRestoring = false;
    }

    void SyncToState()
    {
        if (isRestoring)
            return;

        var state = EquipmentState.Instance;
        if (state != null)
            state.Set(equipped);
    }

    // 루트 아래의 모든 부위 SpriteResolver를 category별로 모은다
    void CacheResolvers(Transform root)
    {
        foreach (var r in root.GetComponentsInChildren<SpriteResolver>(true))
        {
            string category = r.GetCategory();
            if (!resolvers.TryGetValue(category, out var list))
            {
                list = new List<SpriteResolver>();
                resolvers[category] = list;
            }
            list.Add(r);

            // 미리보기 복제본은 '현재 라벨 읽기' 대상에서 제외. 실제 플레이어 값만 진실로 본다.
            // (previewRoot가 캐릭터의 자식이어도 여기서 걸러진다)
            bool isPreview = previewRoot != null && r.transform.IsChildOf(previewRoot);
            if (!isPreview && !playerResolvers.ContainsKey(category))
                playerResolvers[category] = r;
        }
    }

    // 이 아이템이 현재 장착 중인지
    public bool IsEquipped(EquipItem item) => item != null && equipped.Contains(item);

    // 실제 플레이어의 해당 부위가 지금 어떤 라벨인지 (그런 부위가 없으면 null)
    public string GetLabel(string category)
        => playerResolvers.TryGetValue(category, out var r) ? r.GetLabel() : null;

    public void Equip(EquipItem item)
    {
        if (item == null || IsEquipped(item)) return;

        // 부위가 겹치는 기존 아이템만 자동 해제 (같은 자리 빠른 교체).
        // 부위가 안 겹치면(예: 상의 / 신발 / 안경) 여러 벌이 동시에 장착된 채로 남는다.
        for (int i = equipped.Count - 1; i >= 0; i--)
        {
            if (SharesPart(equipped[i], item))
            {
                ResetSwaps(equipped[i].swaps);
                equipped.RemoveAt(i);
            }
        }

        ApplySwaps(item.swaps);
        equipped.Add(item);

        // 이벤트를 받는 쪽이 일관된 상태를 보도록 먼저 반영한다
        SyncToState();
        onEquipChanged?.Invoke();
    }

    public void Unequip(EquipItem item)
    {
        if (item == null) return;
        if (!equipped.Remove(item)) return;

        ResetSwaps(item.swaps);

        SyncToState();
        onEquipChanged?.Invoke();
    }

    // 두 아이템이 같은 부위를 하나라도 건드리는지
    static bool SharesPart(EquipItem a, EquipItem b)
    {
        if (a == null || b == null || a.swaps == null || b.swaps == null)
            return false;

        foreach (var x in a.swaps)
            foreach (var y in b.swaps)
                if (x.category == y.category)
                    return true;

        return false;
    }

    // swaps대로 부위 스프라이트 교체 (실제 + 미리보기 모두)
    void ApplySwaps(PartSwap[] swaps)
    {
        if (swaps == null) return;
        foreach (var s in swaps)
            if (resolvers.TryGetValue(s.category, out var list))
                foreach (var r in list)
                    r.SetCategoryAndLabel(s.category, s.label);
    }

    // swaps에 해당하는 부위들을 default로 되돌림 (실제 + 미리보기 모두)
    void ResetSwaps(PartSwap[] swaps)
    {
        if (swaps == null) return;
        foreach (var s in swaps)
            if (resolvers.TryGetValue(s.category, out var list))
                foreach (var r in list)
                    r.SetCategoryAndLabel(s.category, DefaultLabel);
    }
}
