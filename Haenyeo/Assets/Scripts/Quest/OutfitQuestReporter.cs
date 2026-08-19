using UnityEngine;

// 캐릭터가 지정한 부위들을 전부 특정 라벨로 입고 있는지 검사해서 퀘스트에 보고한다.
// (예: body/armL/armR/legL/legR 이 전부 C2 = 고무옷 한 벌을 다 입은 상태)
// CharacterEquipper와 같은 오브젝트에 붙인다.
[RequireComponent(typeof(CharacterEquipper))]
public class OutfitQuestReporter : MonoBehaviour
{
    [Header("Report")]
    [SerializeField]
    Category category;      // Task의 category와 같아야 함
    [SerializeField]
    TaskTarget target;      // Task의 targets에 들어있어야 함 (StringTarget 등)

    [Header("완료로 볼 착용 상태")]
    [SerializeField]
    PartSwap[] required;    // 전부 일치해야 완료. 예: body=C2, armL=C2, glassB=G1, footL=F1 ...

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

    // 장착/해제로 겉모습이 바뀔 때마다 호출
    void OnEquipChanged()
    {
        // 해당 퀘스트를 진행 중이 아니면 스프라이트 검사조차 하지 않는다
        if (!IsQuestWatching())
            return;

        if (!IsWearingAll())
            return;

        QuestSystem.Instance.ReceiveReport(category, target, 1);
    }

    // 지금 진행 중인 단계에서 이 타겟을 기다리는 퀘스트가 하나라도 있는지.
    // Quest.ContainsTarget은 아직 오지 않은 뒷 단계까지 훑기 때문에 현재 단계만 본다.
    bool IsQuestWatching()
    {
        if (category == null || target == null)
            return false;

        foreach (var quest in QuestSystem.Instance.ActiveQuests)
        {
            var group = quest.CurrentTaskGroup;
            if (group != null && group.ContainsTarget(target))
                return true;
        }
        return false;
    }

    // required의 부위가 전부 지정한 라벨인지
    bool IsWearingAll()
    {
        if (required == null || required.Length == 0)
            return false;

        foreach (var need in required)
        {
            if (equipper.GetLabel(need.category) != need.label)
                return false;
        }
        return true;
    }
}
