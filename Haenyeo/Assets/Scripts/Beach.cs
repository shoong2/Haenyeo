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
    //���ϳ�, �����, �̴ٿ�
    Vector2[] position_1 = new Vector2[]
    {
        new Vector2(-5.12f, -1.11f),
        new Vector2(-1.15f, -0.01f),
        new Vector2(2.18f, -1.07f)
    };

    //���ϳ�, �����, �̴ٿ�, ���μ�
    Vector2[] position_2 = new Vector2[]
    {
        new Vector2(-5.87f, -0.76f),
        new Vector2(-2.63f, 0.89f),
        new Vector2(0.07f, -1.07f),
        new Vector2(3.02f, 1.19f)
    };

    //���ϳ�, �����, �̴ٿ�, ���μ�, ������
    Vector2[] position_3 = new Vector2[]
    {
        new Vector2(-7.64f, -1.33f),
        new Vector2(-4.65f, 0.89f),
        new Vector2(-1.92f, -1.07f),
        new Vector2(0.99f, 1.19f),
        new Vector2(3.91f, -0.43f)
    };

    void Start()
    {
        foreach(var quest in QuestSystem.Instance.ActiveQuests)
        {
            Debug.Log("In beach");
            Debug.Log(quest.people.Length);
            for (int i = 0; i < quest.people.Length; i++)
            {
                Debug.Log(i);
                if (quest.people.Length == 3)
                {
                    foreach(var person in people)
                    {
                        if(quest.people[i] == person.name)
                        {
                            person.SetActive(true);
                            person.transform.localPosition = position_1[i];
                        }
                    }
                }
                else if (quest.people.Length == 4)
                {
                    foreach (var person in people)
                    {
                        if (quest.people[i] == person.name)
                        {
                            person.SetActive(true);
                            person.transform.localPosition = position_2[i];
                        }
                    }
                }
                else
                {
                    foreach (var person in people)
                    {
                        if (quest.people[i] == person.name)
                        {
                            person.SetActive(true);
                            person.transform.localPosition = position_3[i];
                        }
                    }
                }
            }
        }

        //for (int i = 0; i < position_1.Length; i++)
        //{
        //    people[i].gameObject.SetActive(true);
        //    people[i].transform.localPosition = position_1[i];
        //}

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
