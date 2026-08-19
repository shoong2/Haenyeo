using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// 모든 퀘스트 에셋 목록. 시트 임포터가 questId로 퀘스트를 찾을 때 쓴다.
[CreateAssetMenu(menuName = "Quest/QuestDatabase")]
public class QuestDatabase : ScriptableObject
{
    [SerializeField]
    List<Quest> quests = new List<Quest>();

    public IReadOnlyList<Quest> Quests => quests;

    public Quest FindQuestBy(string questId) => quests.FirstOrDefault(x => x != null && x.QuestId == questId);

#if UNITY_EDITOR
    [ContextMenu("프로젝트의 모든 Quest 찾기")]
    public void FindQuests()
    {
        quests = AssetDatabase.FindAssets("t:Quest")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<Quest>)
            .Where(x => x != null)
            .OrderBy(x => x.name)
            .ToList();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif
}
