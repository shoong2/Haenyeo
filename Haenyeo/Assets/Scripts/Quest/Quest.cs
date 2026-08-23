using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

using Debug = UnityEngine.Debug;

public enum QuestState
{
    Inactive,
    Running,
    Complete
}

[CreateAssetMenu(menuName = "Quest/Quest", fileName = "Q_")]
public class Quest : ScriptableObject
{
    #region Events
    public delegate void ObjectiveCountChangedHandler(Quest quest, QuestObjective objective, int currentCount, int prevCount);
    public delegate void CompletedHandler(Quest quest);
    public delegate void NewStepHandler(Quest quest, QuestStep currentStep, QuestStep prevStep);
    public delegate void RewardsGivenHandler(Quest quest, IReadOnlyList<Reward> rewards);
    #endregion

    [Tooltip("에셋 파일명과 같게 둔다. 시트 임포터와 코드가 퀘스트를 찾는 키")]
    [SerializeField]
    string questId;

    [Tooltip("퀘스트 트래커에 표시. 최대 16자")]
    [SerializeField]
    string displayName;

    [Tooltip("기획 시트 '퀘스트 리스트'의 ID (q_tuto_1 등). 임포터가 이 값으로 행을 찾는다")]
    [SerializeField]
    string sheetId;

    [Header("진행")]
    [SerializeField]
    QuestStep[] steps;

    [Header("완료 보상")]
    [SerializeField]
    Reward[] rewards;

    [Header("옵션")]
    [Tooltip("이 퀘스트를 완료해야 수주할 수 있다. 비우면 바로 수주 가능")]
    [SerializeField]
    Quest prerequisite;

    [Tooltip("0이면 제한 없음. 0보다 크면 그 초 안에 끝내야 한다")]
    [SerializeField]
    int timeLimitSeconds;

    [Tooltip("완료 시 알림창을 띄울지")]
    [SerializeField]
    bool showNotification = true;

    int currentStepIndex;

    public string QuestId => questId;
    public string DisplayName => displayName;
    public string SheetId => sheetId;
    public IReadOnlyList<QuestStep> Steps => steps;
    public IReadOnlyList<Reward> Rewards => rewards;
    public QuestStep CurrentStep => steps[currentStepIndex];
    public int TimeLimitSeconds => timeLimitSeconds;
    public bool ShowNotification => showNotification;

    public QuestState State { get; private set; }
    public bool IsRegistered => State != QuestState.Inactive;
    public bool IsComplete => State == QuestState.Complete;

    // Register가 만든 사본은 원본 에셋을 기억한다. 재도전(Restart) 때 원본이 필요하다.
    public Quest Source { get; private set; }

    public bool IsAcceptable
        => prerequisite == null || QuestSystem.Instance.IsCompleted(prerequisite);

    public event ObjectiveCountChangedHandler onObjectiveCountChanged;
    public event CompletedHandler onCompleted;
    public event NewStepHandler onNewStep;
    public event RewardsGivenHandler onRewardsGiven;

    public void OnRegister()
    {
        Debug.Assert(!IsRegistered, $"[Quest] 이미 등록된 퀘스트입니다: {questId}");

        foreach (var step in steps)
        {
            step.Setup(this);
            foreach (var objective in step.Objectives)
                objective.onCountChanged += OnObjectiveCountChanged;
        }

        State = QuestState.Running;
        CurrentStep.Start();
    }

    public void ReceiveReport(ObjectiveType type, string target, int count)
    {
        if (!IsRegistered || IsComplete)
            return;

        CurrentStep.ReceiveReport(type, target, count);

        if (!CurrentStep.IsAllComplete)
            return;

        bool isLastStep = currentStepIndex + 1 == steps.Length;
        if (isLastStep)
        {
            Complete();
            return;
        }

        var prevStep = steps[currentStepIndex++];
        prevStep.End();
        prevStep.GiveRewards(this);

        if (prevStep.Rewards.Count > 0)
            onRewardsGiven?.Invoke(this, prevStep.Rewards);

        CurrentStep.Start();
        onNewStep?.Invoke(this, CurrentStep, prevStep);
    }

    public void Complete()
    {
        CheckIsRunning();

        foreach (var step in steps)
            step.Complete();

        State = QuestState.Complete;

        foreach (var reward in rewards)
            if (reward != null)
                reward.Give(this);

        onCompleted?.Invoke(this);

        ClearEvents();
    }

    // 제한 시간을 넘겨 재도전할 때. 이 사본은 버리고 QuestSystem이 원본을 다시 등록한다.
    public void Abandon()
    {
        foreach (var step in steps)
            step.End();

        State = QuestState.Inactive;
        ClearEvents();
    }

    public bool ContainsTarget(string target) => steps.Any(x => x.ContainsTarget(target));

    public Quest Clone()
    {
        // ScriptableObject의 Instantiate는 직렬화 기반 복사라
        // QuestStep / QuestObjective 같은 [Serializable] 클래스도 새 인스턴스로 복사된다.
        // (예전에는 Task가 ScriptableObject여서 손으로 복사해야 했다)
        var clone = Instantiate(this);
        clone.Source = Source != null ? Source : this;
        clone.name = name;
        return clone;
    }

    void OnObjectiveCountChanged(QuestObjective objective, int currentCount, int prevCount)
        => onObjectiveCountChanged?.Invoke(this, objective, currentCount, prevCount);

    void ClearEvents()
    {
        onObjectiveCountChanged = null;
        onCompleted = null;
        onNewStep = null;
        onRewardsGiven = null;
    }

    // 완료 직전에 호출된다. 진행 중이어야 정상이다.
    // (예전에는 조건이 전부 반대로 적혀 있어서 완료할 때마다 어서션이 터졌다)
    [Conditional("UNITY_EDITOR")]
    void CheckIsRunning()
    {
        Debug.Assert(IsRegistered, $"[Quest] 등록되지 않은 퀘스트를 완료하려 합니다: {questId}");
        Debug.Assert(!IsComplete, $"[Quest] 이미 완료된 퀘스트입니다: {questId}");
    }

#if UNITY_EDITOR
    // 에셋 파일명을 questId의 기본값으로 쓴다. 임포터가 이 값으로 시트 행을 찾는다.
    void OnValidate()
    {
        if (string.IsNullOrEmpty(questId))
            questId = name;
    }
#endif
}
