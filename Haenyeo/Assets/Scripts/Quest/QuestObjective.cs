using System.Linq;
using UnityEngine;

// 퀘스트의 목표 한 줄. ("재원과 대화하기", "전복 3개 모으기")
//
// 예전 Task는 ScriptableObject라 목표 하나마다 에셋 파일이 필요했다.
// 27개 Task 중 두 퀘스트가 공유하는 것이 하나도 없었으므로 인라인으로 내렸다.
// Quest.Clone()의 Instantiate가 직렬화 기반 복사를 하므로 이 클래스도 자동으로 깊은 복사된다.
[System.Serializable]
public class QuestObjective
{
    #region Events
    public delegate void StateChangedHandler(QuestObjective objective, ObjectiveState currentState, ObjectiveState prevState);
    public delegate void CountChangedHandler(QuestObjective objective, int currentCount, int prevCount);
    #endregion

    [SerializeField]
    ObjectiveType type;

    [SerializeField]
    string description;

    // 씬 태그 / 아이템 ID / 착용 라벨 등 리포터가 보내는 문자열.
    // 예전 TaskTarget 에셋(값이 문자열 하나뿐이었다)을 대체한다.
    [SerializeField]
    string[] targets;

    [SerializeField]
    int needCount = 1;

    [Tooltip("완료된 뒤에도 보고를 계속 받을지")]
    [SerializeField]
    bool acceptReportsWhenComplete;

    int currentCount;
    ObjectiveState state;

    public event StateChangedHandler onStateChanged;
    public event CountChangedHandler onCountChanged;

    public ObjectiveType Type => type;
    public string Description => description;
    public int NeedCount => needCount;
    public Quest Owner { get; set; }
    public bool IsComplete => state == ObjectiveState.Complete;

    public int CurrentCount
    {
        get => currentCount;
        set
        {
            int prevCount = currentCount;
            currentCount = Mathf.Clamp(value, 0, needCount);

            if (currentCount == prevCount)
                return;

            State = currentCount == needCount ? ObjectiveState.Complete : ObjectiveState.Running;
            onCountChanged?.Invoke(this, currentCount, prevCount);
        }
    }

    public ObjectiveState State
    {
        get => state;
        set
        {
            // 값이 안 바뀌었는데 이벤트를 쏘면 구독자가 같은 상태를 중복 처리한다.
            if (state == value)
                return;

            var prevState = state;
            state = value;
            onStateChanged?.Invoke(this, state, prevState);
        }
    }

    public void Setup(Quest owner) => Owner = owner;

    public void Start() => State = ObjectiveState.Running;

    public void End()
    {
        onStateChanged = null;
        onCountChanged = null;
    }

    public void Complete() => CurrentCount = needCount;

    public void ReceiveReport(int count) => CurrentCount = currentCount + count;

    // 이 보고가 내 목표인지. 완료된 목표는 기본적으로 더 받지 않는다.
    public bool IsTarget(ObjectiveType reportType, string target)
        => type == reportType
        && ContainsTarget(target)
        && (!IsComplete || acceptReportsWhenComplete);

    public bool ContainsTarget(string target)
        => !string.IsNullOrEmpty(target)
        && targets != null
        && targets.Any(x => x == target);
}
