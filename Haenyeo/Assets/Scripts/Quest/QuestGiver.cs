using UnityEngine;

// 조건이 맞는 퀘스트를 수주시킨다. DontDestroy 프리팹에 붙어 게임당 하나만 산다.
//
// 예전에는 Update에서 W키로도 등록할 수 있었고(디버그 코드), 중복 방지 가드가
// "완료 목록에 없으면"뿐이라 진행 중인 퀘스트를 다시 등록할 수 있었다.
public class QuestGiver : MonoBehaviour
{
    [SerializeField]
    Quest[] quests;

    void Start()
    {
        QuestSystem.Instance.onQuestCompleted += OnQuestCompleted;
        RegisterAvailableQuests();
    }

    void OnDestroy()
    {
        var questSystem = QuestSystem.Instance;
        if (questSystem != null)
            questSystem.onQuestCompleted -= OnQuestCompleted;
    }

    // 퀘스트가 하나 끝나면 그걸 선행으로 삼는 퀘스트가 열린다
    void OnQuestCompleted(Quest quest) => RegisterAvailableQuests();

    void RegisterAvailableQuests()
    {
        var questSystem = QuestSystem.Instance;

        foreach (var quest in quests)
        {
            if (quest == null || questSystem.IsRegisteredOrCompleted(quest))
                continue;

            if (quest.IsAcceptable)
                questSystem.Register(quest);
        }
    }
}
