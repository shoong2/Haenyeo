using System.Collections.Generic;
using UnityEngine;

// 지정한 TaskTarget을 가진 Task가 완료되면 오브젝트를 끈다.
// (예: 특정 NPC와의 대화 태스크가 끝나면 그 NPC를 비활성화)
//
// 씬을 나갔다 들어와도 계속 꺼져 있다. QuestSystem이 DontDestroyOnLoad라
// 퀘스트 진행 상태가 메모리에 남아 있고, Start에서 그걸 되짚어 보기 때문.
// (세이브 파일과는 무관 — 게임을 재시작하면 퀘스트 자체가 초기화된다)
public class QuestTaskObjectDisabler : MonoBehaviour
{
    [SerializeField]
    TaskTarget target;                  // 이 타겟을 가진 Task를 감시한다

    [SerializeField]
    GameObject[] objectsToDisable;      // 끌 오브젝트들. 비워두면 자기 자신

    // 이벤트 해제용
    readonly List<Quest> watchedQuests = new List<Quest>();
    readonly List<Task> hookedTasks = new List<Task>();

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("[TaskDisabler] target이 비어 있음", this);
            return;
        }

        // 씬에 들어온 시점에 이미 끝난 태스크면 바로 끄고 감시도 걸지 않는다
        if (IsTaskAlreadyComplete())
        {
            DisableObjects();
            return;
        }

        QuestSystem.Instance.onQuestRegistered += TryWatch;
        foreach (var quest in QuestSystem.Instance.ActiveQuests)
            TryWatch(quest);
    }

    void OnDestroy()
    {
        // QuestSystem은 씬을 넘어 살아남으므로 반드시 끊어줘야 한다.
        // 단 게임 종료 중에는 Instance가 null을 돌려주므로 확인하고 접근한다.
        var questSystem = QuestSystem.Instance;
        if (questSystem != null)
            questSystem.onQuestRegistered -= TryWatch;

        foreach (var quest in watchedQuests)
            quest.onNewTaskGroup -= OnNewTaskGroup;
        foreach (var task in hookedTasks)
            task.onStateChanged -= OnTaskStateChanged;

        watchedQuests.Clear();
        hookedTasks.Clear();
    }

    // 이 타겟을 쓰는 퀘스트면 감시 시작
    void TryWatch(Quest quest)
    {
        if (!quest.ContainsTarget(target) || watchedQuests.Contains(quest))
            return;

        watchedQuests.Add(quest);
        quest.onNewTaskGroup += OnNewTaskGroup;

        HookCurrentTask(quest);
    }

    // 현재 단계에 내 타겟의 Task가 있으면 상태 변화를 구독
    void HookCurrentTask(Quest quest)
    {
        var task = quest.CurrentTaskGroup.FindTaskByTarget(target);
        if (task == null)
            return;

        if (task.IsComplete)
        {
            DisableObjects();
            return;
        }

        if (hookedTasks.Contains(task))
            return;

        hookedTasks.Add(task);
        task.onStateChanged += OnTaskStateChanged;
    }

    void OnNewTaskGroup(Quest quest, TaskGroup currentTaskGroup, TaskGroup prevTaskGroup)
        => HookCurrentTask(quest);

    void OnTaskStateChanged(Task task, TaskState currentState, TaskState prevState)
    {
        if (currentState != TaskState.Complete)
            return;

        DisableObjects();
    }

    void DisableObjects()
    {
        if (objectsToDisable == null || objectsToDisable.Length == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        foreach (var go in objectsToDisable)
            if (go != null)
                go.SetActive(false);
    }

    #region 이미 완료됐는지 되짚기

    bool IsTaskAlreadyComplete()
    {
        var questSystem = QuestSystem.Instance;

        // 이미 완료된 퀘스트에 이 타겟이 있으면 그 태스크도 당연히 끝난 것
        foreach (var quest in questSystem.CompletedQuests)
            if (quest.ContainsTarget(target))
                return true;

        foreach (var achievement in questSystem.CompletedAchievements)
            if (achievement.ContainsTarget(target))
                return true;

        // 진행 중인 퀘스트
        foreach (var quest in questSystem.ActiveQuests)
        {
            if (!quest.ContainsTarget(target))
                continue;

            var task = quest.CurrentTaskGroup.FindTaskByTarget(target);
            if (task != null)
            {
                if (task.IsComplete)
                    return true;
                continue;   // 현재 단계에서 진행 중 — 아직 안 끝났다
            }

            // 현재 단계에 없다. 지나간 단계에 있었으면 끝난 것,
            // 아직 오지 않은 뒷 단계면 안 끝난 것이라 구분해야 한다.
            if (IsInPastTaskGroup(quest))
                return true;
        }

        return false;
    }

    // 현재 단계보다 앞선 단계에 이 타겟이 있었는지
    bool IsInPastTaskGroup(Quest quest)
    {
        foreach (var group in quest.TaskGroups)
        {
            // 현재 단계에 도달했다면 그 앞에는 없었다는 뜻
            if (ReferenceEquals(group, quest.CurrentTaskGroup))
                return false;

            if (group.ContainsTarget(target))
                return true;
        }
        return false;
    }

    #endregion
}
