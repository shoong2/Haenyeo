using UnityEngine;

// 진행 중인 퀘스트마다 트래커를 하나씩 만든다.
public class QuestTrackerView : MonoBehaviour
{
    [SerializeField]
    QuestTracker questTrackerPrefab;

    void Start()
    {
        var questSystem = QuestSystem.Instance;
        questSystem.onQuestRegistered += CreateQuestTracker;

        foreach (var quest in questSystem.ActiveQuests)
            CreateQuestTracker(quest);
    }

    void OnDestroy()
    {
        var questSystem = QuestSystem.Instance;
        if (questSystem != null)
            questSystem.onQuestRegistered -= CreateQuestTracker;
    }

    void CreateQuestTracker(Quest quest)
        => Instantiate(questTrackerPrefab, transform).Setup(quest);
}
