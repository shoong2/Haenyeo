using System.Collections.Generic;
using UnityEngine;

// 지정한 타겟을 가진 목표가 완료되면 오브젝트를 끈다.
// (예: 특정 NPC와의 대화 목표가 끝나면 그 NPC를 비활성화)
//
// 씬을 나갔다 들어와도 계속 꺼져 있다. QuestSystem이 DontDestroyOnLoad라
// 퀘스트 진행 상태가 메모리에 남아 있고, Start에서 그걸 되짚어 보기 때문.
public class QuestTaskObjectDisabler : MonoBehaviour
{
    [Tooltip("이 타겟을 가진 목표를 감시한다")]
    [SerializeField]
    string targetId;

    [Tooltip("끌 오브젝트들. 비워두면 자기 자신")]
    [SerializeField]
    GameObject[] objectsToDisable;

    readonly List<Quest> watchedQuests = new List<Quest>();
    readonly List<QuestObjective> hookedObjectives = new List<QuestObjective>();

    void Start()
    {
        if (string.IsNullOrEmpty(targetId))
        {
            Debug.LogWarning("[QuestTaskObjectDisabler] targetId가 비어 있음", this);
            return;
        }

        // 씬에 들어온 시점에 이미 끝난 목표면 바로 끄고 감시도 걸지 않는다
        if (IsObjectiveAlreadyComplete())
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
        var questSystem = QuestSystem.Instance;
        if (questSystem != null)
            questSystem.onQuestRegistered -= TryWatch;

        foreach (var quest in watchedQuests)
            quest.onNewStep -= OnNewStep;
        foreach (var objective in hookedObjectives)
            objective.onStateChanged -= OnObjectiveStateChanged;

        watchedQuests.Clear();
        hookedObjectives.Clear();
    }

    void TryWatch(Quest quest)
    {
        if (!quest.ContainsTarget(targetId) || watchedQuests.Contains(quest))
            return;

        watchedQuests.Add(quest);
        quest.onNewStep += OnNewStep;

        HookCurrentObjective(quest);
    }

    void HookCurrentObjective(Quest quest)
    {
        var objective = quest.CurrentStep.FindObjectiveByTarget(targetId);
        if (objective == null)
            return;

        if (objective.IsComplete)
        {
            DisableObjects();
            return;
        }

        if (hookedObjectives.Contains(objective))
            return;

        hookedObjectives.Add(objective);
        objective.onStateChanged += OnObjectiveStateChanged;
    }

    void OnNewStep(Quest quest, QuestStep currentStep, QuestStep prevStep) => HookCurrentObjective(quest);

    void OnObjectiveStateChanged(QuestObjective objective, ObjectiveState currentState, ObjectiveState prevState)
    {
        if (currentState == ObjectiveState.Complete)
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

    // 이미 완료된 퀘스트에 이 타겟이 있으면 그 목표도 당연히 끝난 것.
    // 진행 중이면 현재 단계에 있는지, 지나간 단계에 있었는지 구분해야 한다.
    bool IsObjectiveAlreadyComplete()
    {
        var questSystem = QuestSystem.Instance;

        foreach (var quest in questSystem.CompletedQuests)
            if (quest.ContainsTarget(targetId))
                return true;

        foreach (var quest in questSystem.ActiveQuests)
        {
            if (!quest.ContainsTarget(targetId))
                continue;

            var objective = quest.CurrentStep.FindObjectiveByTarget(targetId);
            if (objective != null)
            {
                if (objective.IsComplete)
                    return true;
                continue;   // 현재 단계에서 진행 중 — 아직 안 끝났다
            }

            if (IsInPastStep(quest))
                return true;
        }

        return false;
    }

    bool IsInPastStep(Quest quest)
    {
        foreach (var step in quest.Steps)
        {
            // 현재 단계에 도달했다면 그 앞에는 없었다는 뜻
            if (ReferenceEquals(step, quest.CurrentStep))
                return false;

            if (step.ContainsTarget(targetId))
                return true;
        }
        return false;
    }
}
