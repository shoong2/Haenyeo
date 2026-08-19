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
        questSystem.onRewardsGiven += NotifyReward;   // 단계(TaskGroup) 보상 알림

        //gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        var questSystem = QuestSystem.Instance;
        if (questSystem != null)
        {
            questSystem.onQuestCompleted -= Notify;
            questSystem.onAchievementCompleted -= Notify;
            questSystem.onRewardsGiven -= NotifyReward;
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

        // 퀘스트 완료 시점의 보상(Quest.rewards)은 여기서 표시
        if (!rewardNoti.activeSelf && quest.Rewards.Count!=0)
        {
            rewardNoti.SetActive(true);
            StartCoroutine(ShowNoticeReward(quest.Rewards));
        }
    }

    // 단계(TaskGroup) 보상 지급 시 보상 알림창 표시
    void NotifyReward(Quest quest, IReadOnlyList<Reward> rewards)
    {
        if (rewards == null || rewards.Count == 0) return;

        if (!rewardNoti.activeSelf)
        {
            rewardNoti.SetActive(true);
            StartCoroutine(ShowNoticeReward(rewards));
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

    IEnumerator ShowNoticeReward(IReadOnlyList<Reward> rewards)
    {
        SoundManager.instance.PlaySE("Reward");
        var waitSeconds = new WaitForSeconds(showTime);

        if(rewards[0].rewardType.ToString() =="Item")
        {
            rewardText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-98);
            rewardText.text = "아이템을 획득했습니다!";
        }
        else if(rewards[0].rewardType.ToString() == "XP")
        {
            rewardText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            rewardText.text = "능력치가 상승했습니다!";
        }


        foreach (GameObject box in rewardBox)
        {
            box.SetActive(false);
        }

        for (int i = 0; i < rewards[0].item.Length; i++)
        {
            rewardBox[i].SetActive(true);
            rewardBox[i].transform.GetChild(0).GetComponent<Image>().sprite = rewards[0].item[i].itemImage;
        }


        yield return waitSeconds;

    }
}
