using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

using Debug = UnityEngine.Debug;

public enum QuestState
{
    Inactive,
    Running,
    Complete,
    Cancel,
    WaitingForCompletion
}

[CreateAssetMenu(menuName ="Quest/Quest", fileName ="Quest_")]
public class Quest : ScriptableObject
{
    #region
    public delegate void TaskSuccessChangedHandler(Quest quest, Task task, int currentSuccess, int prevSuccess);
    public delegate void CompletedHandler(Quest quest);
    public delegate void CanceledHandler(Quest quest);
    public delegate void NewTaskGroupHandler(Quest quest, TaskGroup currentTaskGroup, TaskGroup prevTaskGroup);
    public delegate void RewardsGivenHandler(Quest quest, IReadOnlyList<Reward> rewards);
    #endregion
    [SerializeField]
    Category category;
    [SerializeField]
    Sprite icon;

    [Header("Text")]
    [SerializeField]
    string codeName;
    [SerializeField]
    string displayName;
    [SerializeField, TextArea]
    string description;

    [Header("Task")]
    [SerializeField]
    TaskGroup[] taskGroups;

    [Header("People")]
    public string[] people;

    [Header("Reward")]
    [SerializeField]
    Reward[] rewards;

    [Header("Option")]
    [SerializeField]
    bool useAutoComplete;
    [SerializeField]
    bool isCancelable;
    [SerializeField]
    bool isSavable;
    [SerializeField]
    bool isNotifier= true;
    [SerializeField]
    int isCount;

    [Header("Condition")]
    [SerializeField]
    Condition[] acceptionConditions;
    [SerializeField]
    Condition[] cancelConditions;

    int currentTaskGroupIndex;

    public Category Category => category;
    public Sprite Icon => icon;
    public string CodeName => codeName;
    public string DisplayName => displayName;
    public string Description => description;
    public QuestState State { get; set; }
    public TaskGroup CurrentTaskGroup => taskGroups[currentTaskGroupIndex];
    public IReadOnlyList<TaskGroup> TaskGroups => taskGroups;
    public IReadOnlyList<Reward> Rewards => rewards;
    public bool IsRegistered => State != QuestState.Inactive;
    public bool IsCompletable => State == QuestState.WaitingForCompletion;
    public bool IsComplete => State == QuestState.Complete;
    public bool IsCancel => State == QuestState.Cancel;
    public virtual bool IsCancelable => isCancelable && cancelConditions.All(x=>x.IsPass(this));
    public bool IsAcceptable=> acceptionConditions.All(x=>x.IsPass(this));
    public virtual bool IsSavable => isSavable;
    public bool IsNotifier => isNotifier;
    public int IsCount => isCount;

    public event TaskSuccessChangedHandler onTaskSuccessChanged;
    public event CompletedHandler onCompleted;
    public event CanceledHandler onCanceled;
    public event NewTaskGroupHandler onNewTaskGroup;
    public event RewardsGivenHandler onRewardsGiven;   // 단계(TaskGroup) 보상이 지급될 때 발동

    public void OnRegister()
    {
        Debug.Assert(!IsRegistered, "This quest has already been registered");

        foreach(var taskGroup in taskGroups)
        {
            taskGroup.Setup(this);
            foreach (var task in taskGroup.Tasks)
                task.onSuccessChanged += OnSuccessChanged;
        }

        State = QuestState.Running;
        CurrentTaskGroup.Start();
    }

    public void ReceiveReport(string category, object target, int successCount)
    {
        Debug.Assert(IsRegistered, "This quest has already been registered");
        Debug.Assert(!IsCancel, "This quest has canceled");

        if (IsComplete)
            return;

        if (category == "DIALOGUE")  //���⼭ ��ȭ �ε��� ������Ʈ
        {
            Debug.Log("update");
            GameManager.instance.storage.saveData.nowIndex++;
        }

        CurrentTaskGroup.RecieveReport(category, target, successCount);

        if (CurrentTaskGroup.IsAllTaskComplete)
        {
            if (currentTaskGroupIndex + 1 == taskGroups.Length)
            {
                State = QuestState.WaitingForCompletion;
                if (useAutoComplete)
                    Complete();
            }
            else
            {
                Debug.Log("here is task end");
                var prevTaskGroup = taskGroups[currentTaskGroupIndex++];
                prevTaskGroup.End();
                prevTaskGroup.GiveRewards(this);   // 방금 끝난 단계의 보상 지급 (예: 대화1 후 고무옷/수경/오리발)
                if (prevTaskGroup.Rewards.Count > 0)
                    onRewardsGiven?.Invoke(this, prevTaskGroup.Rewards);   // 알림창용 이벤트
                CurrentTaskGroup.Start();
                onNewTaskGroup?.Invoke(this, CurrentTaskGroup, prevTaskGroup);
            }
        }
        else
            State = QuestState.Running;
    }

    public void Complete()
    {
        Debug.Log("Check");
        CheckIsRunning();

        foreach (var taskGroup in taskGroups)
            taskGroup.Complete();

        State = QuestState.Complete;

        foreach (var reward in rewards)
            reward.Give(this);

        onCompleted?.Invoke(this);

        onTaskSuccessChanged = null;
        onCompleted = null;
        onCanceled = null;
        onNewTaskGroup = null;
        onRewardsGiven = null;
    }

    public virtual void Cancel()
    {
        CheckIsRunning();
        Debug.Assert(IsCancelable, "This quest cant be canceled");

        State = QuestState.Cancel;
        onCanceled?.Invoke(this);

    }

    public bool ContainsTarget(object target) => taskGroups.Any(x => x.ContainsTarget(target));

    public bool ContainsTarget(TaskTarget target) => ContainsTarget(target.Value);

    public Quest Clone()
    {
        var clone = Instantiate(this);
        clone.taskGroups = taskGroups.Select(x => new TaskGroup(x)).ToArray();

        return clone;
    }

    public QuestSavaData ToSaveData()
    {
        return new QuestSavaData
        {
            codeName = codeName,
            state = State,
            taskGroupIndex = currentTaskGroupIndex,
            taskSuccessCounts = CurrentTaskGroup.Tasks.Select(x => x.CurrentSuccess).ToArray()
        };
    }

    public void LoadFrom(QuestSavaData saveDate)
    {
        State = saveDate.state;
        currentTaskGroupIndex = saveDate.taskGroupIndex;

        for(int i=0; i <currentTaskGroupIndex; i++)
        {
            var taskGroup = taskGroups[i];
            taskGroup.Start();
            taskGroup.Complete();
        }

        for(int i=0; i<saveDate.taskSuccessCounts.Length; i++)
        {
            CurrentTaskGroup.Start();
            CurrentTaskGroup.Tasks[i].CurrentSuccess = saveDate.taskSuccessCounts[i];
        }
    }

    void OnSuccessChanged(Task task, int currentSuccess, int prevSuccess)
        => onTaskSuccessChanged?.Invoke(this, task, currentSuccess, prevSuccess);


    [Conditional("UNITY_EDITOR")]
    void CheckIsRunning()
    {
        Debug.Assert(!IsRegistered, "This quest has already been registered");
        Debug.Assert(!IsCancel, "This quest has canceled");
        Debug.Assert(!IsCompletable, "This quest has already been completed");
    }
}
