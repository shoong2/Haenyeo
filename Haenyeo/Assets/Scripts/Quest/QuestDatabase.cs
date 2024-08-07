using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName ="Quest/QuestDatabase")]
public class QuestDatabase : ScriptableObject
{
    [SerializeField]
    List<Quest> quests;

    public IReadOnlyList<Quest> Quests => quests;

    public Quest FindQuestBy(string codeName) => quests.FirstOrDefault(x => x.CodeName == codeName);

#if UNITY_EDITOR
    [ContextMenu("FindQuests")]
    void FindQuests()
    {
        FindQuestsBy<Quest>();
    }

    [ContextMenu("FindAchievements")]
    void FindAchievements()
    {
        FindQuestsBy<Achievement>();
    }
    void FindQuestsBy<T>() where T: Quest
    {
        quests = new List<Quest>();
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");

        foreach(var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var quest = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (quest.GetType() == typeof(T))
                quests.Add(quest);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
#endif
}
