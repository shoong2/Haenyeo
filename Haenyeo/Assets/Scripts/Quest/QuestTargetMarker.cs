using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class QuestTargetMarker : MonoBehaviour
{
    //TryAddTargetQuest �Լ����� ����Ʈ�� �ִ� ��� Ÿ���� ��ȸ�� �� ��Ȱ��ȭ ���ִ� ����� ��ȸ�� �ż�
    //�и����Ѽ� �� ��� �����ϱ�


    [SerializeField]
    TaskTarget target;
    [SerializeField]
    MarkerMaterialData[] markerMaterialDatas;

    Dictionary<Quest, Task> targetTasksByQuest = new Dictionary<Quest, Task>();
    //Transform cameraTransform;
    //Renderer renderer;


    private void Awake()
    {
        //cameraTransform = Camera.main.transform;
       // renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        gameObject.SetActive(false);

        QuestSystem.Instance.onQuestRegistered += TryAddTargetQuest;
        foreach (var quest in QuestSystem.Instance.ActiveQuests)
            TryAddTargetQuest(quest);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearEvent();
    }


    private void OnDestroy()
    {
        ClearEvent();
    }

    void ClearEvent()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 게임 종료 중에는 Instance가 null을 돌려주므로 확인하고 접근한다
        var questSystem = QuestSystem.Instance;
        if (questSystem != null)
            questSystem.onQuestRegistered -= TryAddTargetQuest;

        foreach ((Quest quest, Task task) in targetTasksByQuest)
        {
            quest.onNewTaskGroup -= UpdateTargetTask;
            quest.onCompleted -= RemoveTargetQuest;
            task.onStateChanged -= UpdateRunningTargetTaskCount;
        }
    }


    void TryAddTargetQuest(Quest quest)
    {
        if(target !=null && quest.ContainsTarget(target))
        {
            quest.onNewTaskGroup += UpdateTargetTask;
            quest.onCompleted += RemoveTargetQuest;

            UpdateTargetTask(quest, quest.CurrentTaskGroup);
        }
    }

    void UpdateTargetTask(Quest quest, TaskGroup currentTaskGroup, TaskGroup prevTaskGroup = null)
    {
        targetTasksByQuest.Remove(quest);

        var task = currentTaskGroup.FindTaskByTarget(target);
        if(task!=null)
        {
            targetTasksByQuest[quest] = task;
            task.onStateChanged += UpdateRunningTargetTaskCount;

            RefreshMarker();
        }
    }

    void RemoveTargetQuest(Quest quest)
    {
        targetTasksByQuest.Remove(quest);
        RefreshMarker();
    }

    void UpdateRunningTargetTaskCount(Task task, TaskState currentState, TaskState prevState = TaskState.Inactive)
    {
        //renderer.material = markerMaterialDatas.First(x => x.category == task.Category).markerMatrial;
        RefreshMarker();
    }

    // 이벤트가 올 때마다 증감을 누적하면 값이 어긋난다.
    // (초기 상태가 Running이 아니면 -1이 되고, Task.State는 값이 안 바뀌어도 매번 이벤트를 쏘기 때문에
    //  Running -> Running 이 반복되면 계속 증가한다)
    // 그래서 델타를 쌓지 않고 매번 실제 상태를 다시 센다.
    void RefreshMarker()
    {
        int runningCount = 0;
        foreach (var pair in targetTasksByQuest)
        {
            if (pair.Value.State == TaskState.Running)
                runningCount++;
        }

        gameObject.SetActive(runningCount > 0);
    }

    [System.Serializable]
    struct MarkerMaterialData
    {
        public Category category;
        public Material markerMatrial;
    }
}
