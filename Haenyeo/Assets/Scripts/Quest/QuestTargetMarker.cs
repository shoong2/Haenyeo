using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 이 타겟을 목표로 하는 퀘스트가 진행 중일 때만 오브젝트를 켠다. (퀘스트 마커)
public class QuestTargetMarker : MonoBehaviour
{
    [Tooltip("QuestObjective의 targets에 들어있는 문자열")]
    [SerializeField]
    string targetId;

    // 구독 장부와 표시 장부를 분리한다.
    //
    // 구독은 "타겟이 어느 단계에든 있는" 퀘스트 전부에 건다. 반면 아래 딕셔너리에는
    // "지금 단계에 목표가 있는" 퀘스트만 담긴다. 예전에는 해제를 딕셔너리 기준으로 해서,
    // 타겟이 뒷 단계에 있는 퀘스트의 구독이 영원히 남았다. QuestSystem은 씬을 넘어
    // 살아남으므로, 씬이 바뀌어 이 마커가 파괴된 뒤에도 이벤트가 죽은 객체를 때렸다.
    readonly List<Quest> subscribedQuests = new List<Quest>();
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

        foreach (var quest in subscribedQuests)
        {
            quest.onNewStep -= UpdateTargetObjective;
            quest.onCompleted -= RemoveTargetQuest;
        }
        subscribedQuests.Clear();

        foreach (var pair in targetObjectivesByQuest)
            pair.Value.onStateChanged -= OnObjectiveStateChanged;
        targetObjectivesByQuest.Clear();
    }

    void TryAddTargetQuest(Quest quest)
    {
        if (string.IsNullOrEmpty(targetId) || !quest.ContainsTarget(targetId))
            return;

        if (subscribedQuests.Contains(quest))
            return;

        subscribedQuests.Add(quest);
        quest.onNewStep += UpdateTargetObjective;
        quest.onCompleted += RemoveTargetQuest;

        UpdateTargetObjective(quest, quest.CurrentStep);
    }

    void UpdateTargetObjective(Quest quest, QuestStep currentStep, QuestStep prevStep = null)
    {
        Untrack(quest);

        var objective = currentStep.FindObjectiveByTarget(targetId);
        if (objective != null)
        {
            targetObjectivesByQuest[quest] = objective;
            objective.onStateChanged += OnObjectiveStateChanged;
        }

        RefreshMarker();
    }

    void RemoveTargetQuest(Quest quest)
    {
        Untrack(quest);

        if (subscribedQuests.Remove(quest))
        {
            quest.onNewStep -= UpdateTargetObjective;
            quest.onCompleted -= RemoveTargetQuest;
        }

        RefreshMarker();
    }

    // 표시 장부에서 빼면서 목표 구독도 함께 끊는다 (둘이 어긋나면 죽은 객체로 이벤트가 온다)
    void Untrack(Quest quest)
    {
        if (targetObjectivesByQuest.TryGetValue(quest, out var objective))
        {
            objective.onStateChanged -= OnObjectiveStateChanged;
            targetObjectivesByQuest.Remove(quest);
        }
    }

    void OnObjectiveStateChanged(QuestObjective objective, ObjectiveState currentState, ObjectiveState prevState)
        => RefreshMarker();

    // 이벤트가 올 때마다 증감을 누적하면 값이 어긋난다. 매번 실제 상태를 다시 센다.
    void RefreshMarker()
    {
        // 파괴된 뒤에 이벤트가 도달하는 경로가 또 생기더라도 예외로 터지지 않게 한다
        if (this == null)
            return;

        int runningCount = 0;
        foreach (var pair in targetObjectivesByQuest)
            if (pair.Value.State == ObjectiveState.Running)
                runningCount++;

        gameObject.SetActive(runningCount > 0);
    }
}
