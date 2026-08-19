using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 이 타겟을 목표로 하는 퀘스트가 진행 중일 때만 오브젝트를 켠다. (퀘스트 마커)
public class QuestTargetMarker : MonoBehaviour
{
    [Tooltip("QuestObjective의 targets에 들어있는 문자열")]
    [SerializeField]
    string targetId;

    readonly Dictionary<Quest, QuestObjective> targetObjectivesByQuest = new Dictionary<Quest, QuestObjective>();

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        gameObject.SetActive(false);

        var questSystem = QuestSystem.Instance;
        questSystem.onQuestRegistered += TryAddTargetQuest;
        questSystem.onQuestUnregistered += RemoveTargetQuest;

        foreach (var quest in questSystem.ActiveQuests)
            TryAddTargetQuest(quest);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ClearEvents();

    void OnDestroy() => ClearEvents();

    void ClearEvents()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 게임 종료 중에는 Instance가 null을 돌려주므로 확인하고 접근한다
        var questSystem = QuestSystem.Instance;
        if (questSystem != null)
        {
            questSystem.onQuestRegistered -= TryAddTargetQuest;
            questSystem.onQuestUnregistered -= RemoveTargetQuest;
        }

        foreach (var pair in targetObjectivesByQuest)
        {
            pair.Key.onNewStep -= UpdateTargetObjective;
            pair.Key.onCompleted -= RemoveTargetQuest;
            pair.Value.onStateChanged -= OnObjectiveStateChanged;
        }
        targetObjectivesByQuest.Clear();
    }

    void TryAddTargetQuest(Quest quest)
    {
        if (string.IsNullOrEmpty(targetId) || !quest.ContainsTarget(targetId))
            return;

        quest.onNewStep += UpdateTargetObjective;
        quest.onCompleted += RemoveTargetQuest;

        UpdateTargetObjective(quest, quest.CurrentStep);
    }

    void UpdateTargetObjective(Quest quest, QuestStep currentStep, QuestStep prevStep = null)
    {
        targetObjectivesByQuest.Remove(quest);

        var objective = currentStep.FindObjectiveByTarget(targetId);
        if (objective == null)
        {
            RefreshMarker();
            return;
        }

        targetObjectivesByQuest[quest] = objective;
        objective.onStateChanged += OnObjectiveStateChanged;

        RefreshMarker();
    }

    void RemoveTargetQuest(Quest quest)
    {
        targetObjectivesByQuest.Remove(quest);
        RefreshMarker();
    }

    void OnObjectiveStateChanged(QuestObjective objective, ObjectiveState currentState, ObjectiveState prevState)
        => RefreshMarker();

    // 이벤트가 올 때마다 증감을 누적하면 값이 어긋난다. 매번 실제 상태를 다시 센다.
    void RefreshMarker()
    {
        int runningCount = 0;
        foreach (var pair in targetObjectivesByQuest)
            if (pair.Value.State == ObjectiveState.Running)
                runningCount++;

        gameObject.SetActive(runningCount > 0);
    }
}
