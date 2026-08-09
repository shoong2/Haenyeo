using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;
using UnityEngine.UI;

public class QuestCompletionNotifier : MonoBehaviour
{

    [Header("Reward")]
    [SerializeField]
    GameObject rewardNoti;
    [SerializeField]
    GameObject[] rewardBox;
    [SerializeField]
    TextMeshProUGUI rewardText;


    [Header("Quest")]
    [SerializeField]
    GameObject questNoti;
    [SerializeField]
    string titleDescription;
    [SerializeField]
    TextMeshProUGUI titleText;

    [SerializeField]
    float showTime = 1f;

    Queue<Quest> reservedQuests = new Queue<Quest>();
    StringBuilder stringBuilder = new StringBuilder();

    private void Start()
    {
        var questSystem = QuestSystem.Instance;
        questSystem.onQuestCompleted += Notify;
        questSystem.onAchievementCompleted += Notify;

        //gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        var questSystem = QuestSystem.Instance;
        if (questSystem != null)
        {
            questSystem.onQuestCompleted -= Notify;
            questSystem.onAchievementCompleted -= Notify;
        }
    }

    void Notify(Quest quest)
    {
        Debug.Log("notify");
        reservedQuests.Enqueue(quest);

        if (!questNoti.activeSelf &&quest.IsNotifier)
        {
            questNoti.SetActive(true);
            StartCoroutine(ShowNotice());
        }

        if (!rewardNoti.activeSelf && quest.Rewards.Count!=0)
        {
            Debug.Log(quest.Rewards);
            Debug.Log("have reward");
            rewardNoti.SetActive(true);
            StartCoroutine(ShowNoticeReward(quest));
        }
    }

    IEnumerator ShowNotice()
    {
        var waitSeconds = new WaitForSeconds(showTime);

        Quest quest;

        while (reservedQuests.TryDequeue(out quest))
        {
            SoundManager.instance.PlaySE("QuestClear");
            //string updateDescription = titleDescription.Replace("%{dn}", quest.DisplayName);
            // titleText.text = $"<b>{updateDescription}</b>";
            titleText.text = titleDescription.Replace("%{dn}", quest.DisplayName);
            //foreach (var reward in quest.Rewards)
            //{
            //    stringBuilder.Append(reward.Description);
            //    stringBuilder.Append(" ");
            //    stringBuilder.Append(reward.Quantitiy);
            //    stringBuilder.Append(" ");
            //}
            //rewardText.text = stringBuilder.ToString();
            //stringBuilder.Clear();

            yield return waitSeconds;
        }

        //gameObject.SetActive(false);
    }

    IEnumerator ShowNoticeReward(Quest quest)
    {
        SoundManager.instance.PlaySE("Reward");
        var waitSeconds = new WaitForSeconds(showTime);

        if(quest.Rewards[0].rewardType.ToString() =="Item")
        {
            rewardText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-98);
            rewardText.text = "아이템을 획득했습니다!";
        }
        else if(quest.Rewards[0].rewardType.ToString() == "XP")
        {
            rewardText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            rewardText.text = "장비 능력치가 상승했습니다! ";
        }


        //Quest quest;

        foreach (GameObject box in rewardBox)
        {
            box.SetActive(false);
        }

        for (int i = 0; i < quest.Rewards[0].item.Length; i++)
        {
            Debug.Log(i);
            rewardBox[i].SetActive(true);
            rewardBox[i].transform.GetChild(0).GetComponent<Image>().sprite = quest.Rewards[0].item[i].itemImage;
        }


        yield return waitSeconds;

    }
}
