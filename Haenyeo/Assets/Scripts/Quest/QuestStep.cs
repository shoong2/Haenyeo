using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 퀘스트의 한 단계. 이 안의 목표를 전부 끝내면 다음 단계로 넘어간다.
// 이 게임에서는 보통 [1단계 = 대화] -> [2단계 = 실제 목표] 형태로 쓴다.
//
// 예전 이름은 TaskGroup. 하는 일이 "묶음"이 아니라 "단계"라서 이름을 바꿨다.
[System.Serializable]
public class QuestStep
{
    [SerializeField]
    QuestObjective[] objectives;

    [Tooltip("이 단계를 끝내고 다음 단계로 넘어갈 때 주는 보상")]
    [SerializeField]
    Reward[] rewards;

    public IReadOnlyList<QuestObjective> Objectives => objectives;
    public IReadOnlyList<Reward> Rewards => rewards;
    public Quest Owner { get; set; }

    public bool IsAllComplete => objectives.All(x => x.IsComplete);

    public void Setup(Quest owner)
    {
        Owner = owner;
        foreach (var objective in objectives)
            objective.Setup(owner);
    }

    public void Start()
    {
        foreach (var objective in objectives)
            objective.Start();
    }

    public void End()
    {
        foreach (var objective in objectives)
            objective.End();
    }

    public void Complete()
    {
        foreach (var objective in objectives)
            if (!objective.IsComplete)
                objective.Complete();
    }

    public void ReceiveReport(ObjectiveType type, string target, int count)
    {
        foreach (var objective in objectives)
            if (objective.IsTarget(type, target))
                objective.ReceiveReport(count);
    }

    public void GiveRewards(Quest owner)
    {
        if (rewards == null)
            return;

        foreach (var reward in rewards)
            if (reward != null)
                reward.Give(owner);
    }

    public QuestObjective FindObjectiveByTarget(string target)
        => objectives.FirstOrDefault(x => x.ContainsTarget(target));

    public bool ContainsTarget(string target) => objectives.Any(x => x.ContainsTarget(target));
}
