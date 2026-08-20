using UnityEngine;
using UnityEngine.Serialization;

// 장착 상태가 조건을 만족하면 퀘스트에 보고한다.
//
// 조건이 두 종류다. 옷은 입으면 캐릭터 스프라이트가 바뀌므로 "부위가 이 라벨인가"로 보고,
// 도구는 겉모습이 바뀌지 않으므로 "이 아이템이 장착 목록에 있는가"로 본다.
// 지정한 조건만 검사하고, 지정된 것이 전부 통과해야 보고한다.
//
// 한 오브젝트에 여러 개 붙일 수 있다. (예: 해녀복 한 벌 / 도구 3종을 각각 감시)
[RequireComponent(typeof(CharacterEquipper))]
public class EquipQuestReporter : MonoBehaviour
{
    [Header("Report")]
    [SerializeField]
    ObjectiveType type = ObjectiveType.Wear;

    [Tooltip("QuestObjective의 targets에 들어있어야 한다")]
    [SerializeField]
    string targetId;

    [Header("조건 A - 부위 라벨 (옷처럼 겉모습이 바뀌는 것)")]
    [Tooltip("이 부위들이 전부 지정한 라벨이어야 한다. 예: body=C2, glassB=G1")]
    [FormerlySerializedAs("required")]
    [SerializeField]
    PartSwap[] requiredParts;

    [Header("조건 B - 장착 아이템 (도구처럼 겉모습이 안 바뀌는 것)")]
    [Tooltip("이 아이템들이 전부 장착 중이어야 한다")]
    [SerializeField]
    EquipItem[] requiredItems;

    CharacterEquipper equipper;

    void Awake() => equipper = GetComponent<CharacterEquipper>();

    void OnEnable()
    {
        if (equipper != null)
            equipper.onEquipChanged += OnEquipChanged;
    }

    void OnDisable()
    {
        if (equipper != null)
            equipper.onEquipChanged -= OnEquipChanged;
    }

    // 장착/해제로 상태가 바뀔 때마다 호출
    void OnEquipChanged()
    {
        // 해당 퀘스트를 진행 중이 아니면 검사조차 하지 않는다
        if (!IsQuestWatching())
            return;

        if (!IsSatisfied())
            return;

        QuestSystem.Instance.ReceiveReport(type, targetId, 1);
    }

    // 지금 진행 중인 단계에서 이 타겟을 기다리는 퀘스트가 하나라도 있는지.
    // Quest.ContainsTarget은 아직 오지 않은 뒷 단계까지 훑기 때문에 현재 단계만 본다.
    bool IsQuestWatching()
    {
        if (string.IsNullOrEmpty(targetId))
            return false;

        foreach (var quest in QuestSystem.Instance.ActiveQuests)
        {
            var step = quest.CurrentStep;
            if (step != null && step.ContainsTarget(targetId))
                return true;
        }
        return false;
    }

    bool IsSatisfied()
    {
        bool hasParts = requiredParts != null && requiredParts.Length > 0;
        bool hasItems = requiredItems != null && requiredItems.Length > 0;

        // 조건을 하나도 안 걸어두면 장착만 해도 완료돼 버린다. 그런 실수를 막는다.
        if (!hasParts && !hasItems)
        {
            Debug.LogWarning($"[EquipQuestReporter] 조건이 비어 있습니다 (targetId: {targetId})", this);
            return false;
        }

        if (hasParts)
        {
            foreach (var need in requiredParts)
                if (equipper.GetLabel(need.category) != need.label)
                    return false;
        }

        if (hasItems)
        {
            foreach (var item in requiredItems)
                if (item == null || !equipper.IsEquipped(item))
                    return false;
        }

        return true;
    }
}
