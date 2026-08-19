using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class Beach : MonoBehaviour
{
    public UnityEvent onScene;
    public GameObject[] people;

    Inventory inven;
    BeachCastTable castTable;

    void Start()
    {
        castTable = Resources.Load<BeachCastTable>("BeachCastTable");
        PlaceCast();

        foreach (var quest in QuestSystem.Instance.ActiveQuests)
        {
            if (quest.ContainsTarget("GoBeach"))
            {
                onScene.Invoke();
                break;
            }
        }

        inven = FindObjectOfType<Inventory>();
    }

    // 진행 중인 퀘스트에 해당하는 인원만 켜고 자리에 세운다
    void PlaceCast()
    {
        if (castTable == null)
        {
            Debug.LogWarning("[Beach] Resources/BeachCastTable 을 찾지 못했습니다");
            return;
        }

        foreach (var quest in QuestSystem.Instance.ActiveQuests)
        {
            var cast = castTable.FindPeople(quest.QuestId);
            if (cast == null || cast.Length == 0)
                continue;

            var positions = castTable.GetPositions(cast.Length);

            for (int i = 0; i < cast.Length && i < positions.Count; i++)
            {
                var person = FindPerson(cast[i]);
                if (person == null)
                    continue;

                person.SetActive(true);
                person.transform.localPosition = positions[i];
            }
        }
    }

    GameObject FindPerson(string personName)
    {
        foreach (var person in people)
            if (person != null && person.name == personName)
                return person;
        return null;
    }

    public void MoveSea()
    {
        SceneManager.LoadScene("Sea");
    }  
    
    public void Bag_()
    {
        if (inven != null)
            inven = FindObjectOfType<Inventory>();

        inven.transform.GetChild(0).gameObject.SetActive(true);
    }
}
