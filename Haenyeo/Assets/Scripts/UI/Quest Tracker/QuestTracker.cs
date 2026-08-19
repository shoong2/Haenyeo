using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 퀘스트 하나의 진행 상황을 화면에 띄운다. 단계가 넘어가면 지난 목표에 취소선을 긋는다.
public class QuestTracker : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI questTitleText;

    [SerializeField]
    ObjectiveDescriptor objectiveDescriptorPrefab;

    readonly Dictionary<QuestObjective, ObjectiveDescriptor> descriptorsByObjective
        = new Dictionary<QuestObjective, ObjectiveDescriptor>();

    Quest targetQuest;

    void OnDestroy()
    {
        if (targetQuest != null)
        {
            targetQuest.onNewStep -= AddDescriptors;
            targetQuest.onCompleted -= DestroySelf;
        }

        foreach (var pair in descriptorsByObjective)
            pair.Key.onCountChanged -= OnObjectiveCountChanged;
    }

    public void Setup(Quest quest)
    {
        targetQuest = quest;

        if (questTitleText != null)
            questTitleText.text = quest.DisplayName;

        quest.onNewStep += AddDescriptors;
        quest.onCompleted += DestroySelf;

        // 이미 진행 중인 퀘스트를 뒤늦게 붙잡는 경우(씬 전환 등)를 위해
        // 첫 단계부터 현재 단계까지 훑으면서 지난 단계는 취소선으로 표시한다.
        var steps = quest.Steps;
        AddDescriptors(quest, steps[0]);

        for (int i = 1; i < steps.Count; i++)
        {
            if (ReferenceEquals(steps[i - 1], quest.CurrentStep))
                break;

            AddDescriptors(quest, steps[i], steps[i - 1]);

            if (ReferenceEquals(steps[i], quest.CurrentStep))
                break;
        }
    }

    void AddDescriptors(Quest quest, QuestStep currentStep, QuestStep prevStep = null)
    {
        foreach (var objective in currentStep.Objectives)
        {
            // 대화 목표는 트래커에 띄우지 않는다 (대사창이 이미 안내한다)
            if (objective.Type == ObjectiveType.Dialogue)
                continue;

            if (descriptorsByObjective.ContainsKey(objective))
                continue;

            var descriptor = Instantiate(objectiveDescriptorPrefab, transform);
            descriptor.UpdateText(objective);
            objective.onCountChanged += OnObjectiveCountChanged;

            descriptorsByObjective.Add(objective, descriptor);
        }

        if (prevStep == null)
            return;

        foreach (var objective in prevStep.Objectives)
        {
            if (descriptorsByObjective.TryGetValue(objective, out var descriptor))
                descriptor.UpdateTextUsingStrikeThrough(objective);
        }
    }

    void OnObjectiveCountChanged(QuestObjective objective, int currentCount, int prevCount)
    {
        if (descriptorsByObjective.TryGetValue(objective, out var descriptor))
            descriptor.UpdateText(objective);
    }

    void DestroySelf(Quest quest) => Destroy(gameObject);
}
