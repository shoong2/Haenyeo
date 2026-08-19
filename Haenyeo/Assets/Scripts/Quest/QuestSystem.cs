using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 진행 중 / 완료된 퀘스트를 들고 있으면서 보고를 각 퀘스트로 흘려보낸다.
//
// 저장/로드는 없다. 예전에는 Save()가 있었지만 호출하는 곳이 하나도 없어서
// 항상 죽은 코드였고, 새 데이터 구조에 맞춰 다시 설계할 예정이다.
public class QuestSystem : MonoBehaviour
{
    #region Events
    public delegate void QuestRegisteredHandler(Quest quest);
    public delegate void QuestCompletedHandler(Quest quest);
    public delegate void QuestUnregisteredHandler(Quest quest);
    #endregion

    static QuestSystem instance;
    static bool isApplicationQuitting;

    public static QuestSystem Instance
    {
        get
        {
            if (isApplicationQuitting)
                return null;

            if (instance == null)
            {
                instance = FindObjectOfType<QuestSystem>();
                if (instance == null)
                {
                    instance = new GameObject("Quest System").AddComponent<QuestSystem>();
                    DontDestroyOnLoad(instance.gameObject);
                }
            }
            return instance;
        }
    }

    readonly List<Quest> activeQuests = new List<Quest>();
    readonly List<Quest> completedQuests = new List<Quest>();

    public event QuestRegisteredHandler onQuestRegistered;
    public event QuestCompletedHandler onQuestCompleted;
    public event QuestUnregisteredHandler onQuestUnregistered;
    public event Quest.RewardsGivenHandler onRewardsGiven;

    public IReadOnlyList<Quest> ActiveQuests => activeQuests;
    public IReadOnlyList<Quest> CompletedQuests => completedQuests;

    void OnApplicationQuit() => isApplicationQuitting = true;

    public Quest Register(Quest quest)
    {
        if (quest == null)
            return null;

        // 같은 퀘스트를 두 번 등록하면 사본이 둘 다 살아남아 보고가 중복 처리된다.
        if (IsActive(quest))
        {
            Debug.LogWarning($"[QuestSystem] 이미 진행 중입니다: {quest.QuestId}");
            return null;
        }

        var newQuest = quest.Clone();

        newQuest.onCompleted += OnQuestCompleted;
        newQuest.onRewardsGiven += OnRewardsGiven;

        activeQuests.Add(newQuest);
        newQuest.OnRegister();
        onQuestRegistered?.Invoke(newQuest);

        return newQuest;
    }

    // 제한 시간을 넘겼을 때의 재도전.
    // 예전에는 진행 중인 사본을 놔둔 채 Register를 또 불러서 같은 퀘스트가 둘 활성화됐다.
    public Quest Restart(Quest activeQuest)
    {
        if (activeQuest == null)
            return null;

        var source = activeQuest.Source != null ? activeQuest.Source : activeQuest;

        Unregister(activeQuest);
        return Register(source);
    }

    public void Unregister(Quest activeQuest)
    {
        if (activeQuest == null || !activeQuests.Remove(activeQuest))
            return;

        activeQuest.Abandon();
        onQuestUnregistered?.Invoke(activeQuest);

        Destroy(activeQuest, Time.deltaTime);
    }

    public void ReceiveReport(ObjectiveType type, string target, int count)
    {
        // 보고 처리 중에 퀘스트가 완료돼 목록이 바뀔 수 있으므로 사본을 순회한다.
        foreach (var quest in activeQuests.ToArray())
            quest.ReceiveReport(type, target, count);
    }

    public bool IsActive(Quest quest)
        => quest != null && activeQuests.Any(x => x.QuestId == quest.QuestId);

    public bool IsCompleted(Quest quest)
        => quest != null && completedQuests.Any(x => x.QuestId == quest.QuestId);

    public bool IsRegisteredOrCompleted(Quest quest) => IsActive(quest) || IsCompleted(quest);

    void OnQuestCompleted(Quest quest)
    {
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        onQuestCompleted?.Invoke(quest);
    }

    void OnRewardsGiven(Quest quest, IReadOnlyList<Reward> rewards)
        => onRewardsGiven?.Invoke(quest, rewards);
}
