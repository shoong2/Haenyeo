using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;

public class QuestSystem : MonoBehaviour
{
    #region Save Path
    const string kSaveRootPath = "questSystem";
    const string kActiveQuestsSavePath = "activeQuests";
    const string kCompletedQuestsSavePath = "completedQuests";
    const string kActiveAchievementsSavePath = "activeAchievements";
    const string kCompletedAchievementsPath = "completedAchievements";
    #endregion


    #region Events
    public delegate void QuestRegisteredHandler(Quest newQuest);
    public delegate void QuestCompletedHandler(Quest quest);
    public delegate void QuestCanceledHandler(Quest quest);
    #endregion


    static QuestSystem instance;
    static bool isApplicationQuitting;

    public static QuestSystem Instance
    {
        get
        {
            if(!isApplicationQuitting && instance ==null)
            {
                instance = FindObjectOfType<QuestSystem>();
                if(instance ==null)
                {
                    instance = new GameObject("Quest System").AddComponent<QuestSystem>();
                    DontDestroyOnLoad(instance.gameObject);
                }
            }
            return instance;
        } 
    }

    List<Quest> activeQuests = new List<Quest>();
    List<Quest> completedQuests = new List<Quest>();

    List<Quest> activeAchievements = new List<Quest>();
    List<Quest> completedAchievements = new List<Quest>();

    QuestDatabase questDatabase;
    QuestDatabase achievementDatabase;

    public event QuestRegisteredHandler onQuestRegistered;
    public event QuestCompletedHandler onQuestCompleted;
    public event QuestCanceledHandler onQuestCanceled;
    public event Quest.RewardsGivenHandler onRewardsGiven;   // 단계 보상 지급 알림 중계

    public event QuestRegisteredHandler onAchievementRegistered;
    public event QuestCompletedHandler onAchievementCompleted;

    public IReadOnlyList<Quest> ActiveQuests => activeQuests;
    public IReadOnlyList<Quest> CompletedQuests => completedQuests;
    public IReadOnlyList<Quest> ActiveAchivements => activeAchievements;
    public IReadOnlyList<Quest> CompletedAchievements => completedAchievements;

    private void Awake()
    {
        questDatabase = Resources.Load<QuestDatabase>("QuestDatabase");
        achievementDatabase = Resources.Load<QuestDatabase>("AchievementDatabase");

        foreach (var achievement in achievementDatabase.Quests)
            Register(achievement);

        if(!Load())
        {
            foreach (var achievement in achievementDatabase.Quests)
                Register(achievement);
        }
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    public Quest Register(Quest quest)
    {
        Debug.Log("register!!!");
        var newQuest = quest.Clone();

        if(newQuest is Achievement)
        {
            newQuest.onCompleted += OnAchievementCompleted;
            newQuest.onRewardsGiven += OnRewardsGiven;

            activeAchievements.Add(newQuest);

            newQuest.OnRegister();

            onAchievementRegistered?.Invoke(newQuest);
        }
        else
        {
            newQuest.onCompleted += OnQuestCompleted;
            newQuest.onCanceled += OnQuestCanceled;
            newQuest.onRewardsGiven += OnRewardsGiven;

            activeQuests.Add(newQuest);
            Debug.Log("Register Quest kkkk");
            newQuest.OnRegister();
            onQuestRegistered?.Invoke(newQuest);
           // if (newQuest.Category == "DIALOGUE")
              //  GameManager.instance.storage.saveData.nowIndex++;
        }

        return newQuest;
    }

    public void ReceiveReport(string category, object target, int successCount)
    {
        ReceiveReport(activeQuests, category, target, successCount);
        ReceiveReport(activeAchievements, category, target, successCount);
    }

    public void ReceiveReport(Category category, TaskTarget target, int successCount)
        => ReceiveReport(category.CodeName, target.Value, successCount);

    void ReceiveReport(List<Quest> quests, string category, object target, int successCount)
    {
        foreach(var quest in quests.ToArray())
        {
            quest.ReceiveReport(category, target, successCount);
        }
    }

    public void CompleteWaitingQuests()
    {
        foreach(var quest in activeQuests.ToList())
        {
            if (quest.IsCompletable)
                quest.Complete();
        }
    }

    public bool ContainsInactiveQuests(Quest quest) => activeQuests.Any(x => x.CodeName == quest.CodeName);
    public bool ContainsInCompletedQuests(Quest quest) => completedQuests.Any(x => x.CodeName == quest.CodeName);
    public bool ContainsInactiveAchievements(Quest quest) => activeAchievements.Any(x => x.CodeName == quest.CodeName);
    public bool ContainsInCompletedAchievements(Quest quest) => completedAchievements.Any(x => x.CodeName == quest.CodeName);

    public void Save()
    {
        var root = new JObject();
        root.Add(kActiveQuestsSavePath, CreateSaveDatas(activeQuests));
        root.Add(kCompletedQuestsSavePath, CreateSaveDatas(completedQuests));
        root.Add(kActiveAchievementsSavePath, CreateSaveDatas(activeAchievements));
        root.Add(kCompletedAchievementsPath, CreateSaveDatas(completedAchievements));

        PlayerPrefs.SetString(kSaveRootPath, root.ToString());
        PlayerPrefs.Save();
    }

    public bool Load()
    {
        if (PlayerPrefs.HasKey(kSaveRootPath))
        {
            var root = JObject.Parse(PlayerPrefs.GetString(kSaveRootPath));

            LoadSaveDatas(root[kActiveQuestsSavePath], questDatabase, LoadActiveQuest);
            LoadSaveDatas(root[kCompletedQuestsSavePath], questDatabase, LoadCompleteQuest);

            LoadSaveDatas(root[kActiveAchievementsSavePath], questDatabase, LoadActiveQuest);
            LoadSaveDatas(root[kCompletedAchievementsPath], questDatabase, LoadCompleteQuest);

            return true;
        }
        else
            return false;
    }

    JArray CreateSaveDatas(IReadOnlyList<Quest> quests)
    {
        var saveDatas = new JArray();
        foreach(var quest in quests)
        {
            if(quest.IsSavable)
                saveDatas.Add(JObject.FromObject(quest.ToSaveData()));
        }
        return saveDatas;
    }

    void LoadSaveDatas(JToken datasToken, QuestDatabase database, System.Action<QuestSavaData, Quest> onSuccess)
    {
        var datas = datasToken as JArray;
        foreach(var data in datas)
        {
            var saveData = data.ToObject<QuestSavaData>();
            var quest = database.FindQuestBy(saveData.codeName);
            onSuccess.Invoke(saveData, quest);
        }
    }

    void LoadActiveQuest(QuestSavaData savedata, Quest quest)
    {
        var newQuest = Register(quest);
        newQuest.LoadFrom(savedata);
    }

    void LoadCompleteQuest(QuestSavaData savedata, Quest quest)
    {
        var newQuest = quest.Clone();
        newQuest.LoadFrom(savedata);

        if (newQuest is Achievement)
            completedAchievements.Add(newQuest);
        else
            completedQuests.Add(newQuest);
    }


    #region Callback
    void OnQuestCompleted(Quest quest)
    {
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        onQuestCompleted?.Invoke(quest);
    }

    void OnRewardsGiven(Quest quest, IReadOnlyList<Reward> rewards)
        => onRewardsGiven?.Invoke(quest, rewards);

    void OnQuestCanceled(Quest quest)
    {
        activeQuests.Remove(quest);
        onQuestCanceled?.Invoke(quest);

        Destroy(quest, Time.deltaTime);
    }

    void OnAchievementCompleted(Quest achievement)
    {
        activeAchievements.Remove(achievement);
        completedAchievements.Add(achievement);

        onAchievementCompleted?.Invoke(achievement);
    }
    #endregion
}
