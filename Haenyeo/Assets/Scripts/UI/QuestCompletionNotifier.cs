using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 퀘스트 완료 / 보상 지급 알림창.
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

    readonly Queue<Quest> reservedQuests = new Queue<Quest>();

    void Start()
    {
        var questSystem = QuestSystem.Instance;
        questSystem.onQuestCompleted += Notify;
        questSystem.onRewardsGiven += NotifyReward;   // 단계 보상 알림
    }

    void OnDestroy()
    {
        var questSystem = QuestSystem.Instance;
        if (questSystem != null)
        {
            questSystem.onQuestCompleted -= Notify;
            questSystem.onRewardsGiven -= NotifyReward;
        }
    }

    void Notify(Quest quest)
    {
        reservedQuests.Enqueue(quest);

        if (!questNoti.activeSelf && quest.ShowNotification)
        {
            questNoti.SetActive(true);
            StartCoroutine(ShowNotice());
        }

        // 퀘스트 완료 시점의 보상은 여기서 표시
        if (!rewardNoti.activeSelf && quest.Rewards.Count != 0)
        {
            rewardNoti.SetActive(true);
            StartCoroutine(ShowNoticeReward(quest.Rewards));
        }
    }

    // 단계 보상 지급 시
    void NotifyReward(Quest quest, IReadOnlyList<Reward> rewards)
    {
        if (rewards == null || rewards.Count == 0 || rewardNoti.activeSelf)
            return;

        rewardNoti.SetActive(true);
        StartCoroutine(ShowNoticeReward(rewards));
    }

    IEnumerator ShowNotice()
    {
        var waitSeconds = new WaitForSeconds(showTime);

        while (reservedQuests.TryDequeue(out var quest))
        {
            SoundManager.instance.PlaySE("QuestClear");
            titleText.text = titleDescription.Replace("%{dn}", quest.DisplayName);

            yield return waitSeconds;
        }
    }

    IEnumerator ShowNoticeReward(IReadOnlyList<Reward> rewards)
    {
        SoundManager.instance.PlaySE("Reward");

        var reward = rewards[0];
        bool isItem = reward is ItemReward;

        rewardText.GetComponent<RectTransform>().anchoredPosition = isItem
            ? new Vector2(0, -98)
            : new Vector2(0, 0);

        rewardText.text = !string.IsNullOrEmpty(reward.Description)
            ? reward.Description
            : (isItem ? "아이템을 획득했습니다!" : "능력치가 상승했습니다!");

        foreach (var box in rewardBox)
            box.SetActive(false);

        // 보상 종류와 무관하게 아이콘 목록만 받아서 채운다
        var icons = reward.GetIcons();
        for (int i = 0; i < icons.Count && i < rewardBox.Length; i++)
        {
            rewardBox[i].SetActive(true);
            rewardBox[i].transform.GetChild(0).GetComponent<Image>().sprite = icons[i];
        }

        yield return new WaitForSeconds(showTime);
    }
}
