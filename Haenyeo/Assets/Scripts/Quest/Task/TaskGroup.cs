using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum TaskGroupState
{
    Inactive,
    Running,
    Complete
}

[System.Serializable]
public class TaskGroup
{
    [SerializeField]
    Task[] tasks;

    [Header("Reward")]
    [SerializeField]
    Reward[] rewards;   // 이 단계(TaskGroup)를 완료하고 다음 단계로 넘어갈 때 지급되는 보상

    public IReadOnlyList<Task> Tasks => tasks;
    public IReadOnlyList<Reward> Rewards => rewards;
    public Quest Owner { get; set; }
    public bool IsAllTaskComplete => tasks.All(x => x.IsComplete);
    public bool IsComplete => State == TaskGroupState.Complete;
    public TaskGroupState State { get; set; }

    public TaskGroup(TaskGroup copytarget)
    {
        tasks = copytarget.Tasks.Select(x => Object.Instantiate(x)).ToArray();
        rewards = copytarget.rewards;   // Reward는 SO 에셋이라 참조 공유 (Give는 상태를 바꾸지 않음)
    }
    public void Setup(Quest owner)
    {
        Owner = owner;
        foreach (var task in tasks)
            task.Setup(owner);
    }

    public void Start()
    {
        State = TaskGroupState.Running;
        foreach (var task in tasks)
            task.Start();
    }

    public void End()
    {
        State = TaskGroupState.Complete;
        foreach (var task in tasks)
            task.End();
    }

    public void RecieveReport(string catecory, object target, int successCount)
    {
        foreach(var task in tasks)
        {
            if (task.IsTarget(catecory, target))
                task.ReceieveReport(successCount);
        }
    }

    public void Complete()
    {
        if (IsComplete)
            return;
        State = TaskGroupState.Complete;

        foreach(var task in tasks)
        {
            if(!task.IsComplete)
                task.complete();
        }
    }

    // 이 단계 완료 시 보상 지급 (다음 단계로 넘어갈 때 Quest가 호출)
    public void GiveRewards(Quest owner)
    {
        if (rewards == null) return;
        foreach (var reward in rewards)
            if (reward != null)
                reward.Give(owner);
    }

    public Task FindTaskByTarget(object target) => tasks.FirstOrDefault(x => x.ContainsTarget(target));

    public Task FindTaskByTarget(TaskTarget target) => FindTaskByTarget(target.Value);

    public bool ContainsTarget(object target) => tasks.Any(x => x.ContainsTarget(target));

    public bool ContainsTarget(TaskTarget target) => ContainsTarget(target.Value);
}
