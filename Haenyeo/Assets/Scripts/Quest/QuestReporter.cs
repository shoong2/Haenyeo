using System.Linq;
using UnityEngine;

// 씬의 오브젝트가 퀘스트에 진행 상황을 보고한다.
// 트리거 충돌로 자동 보고하거나, Report()를 다른 코드/UnityEvent에서 직접 부른다.
public class QuestReporter : MonoBehaviour
{
    [SerializeField]
    ObjectiveType type;

    [Tooltip("QuestObjective의 targets에 들어있는 문자열과 정확히 같아야 한다")]
    [SerializeField]
    string targetId;

    [SerializeField]
    int count = 1;

    [Tooltip("이 태그를 가진 콜라이더가 닿으면 보고한다. 비우면 트리거 보고를 하지 않는다")]
    [SerializeField]
    string[] colliderTags;

    public void Report()
    {
        if (string.IsNullOrEmpty(targetId))
        {
            Debug.LogWarning("[QuestReporter] targetId가 비어 있습니다", this);
            return;
        }

        QuestSystem.Instance.ReceiveReport(type, targetId, count);
    }

    void OnTriggerEnter(Collider other) => ReportIfTagMatches(other);

    void OnTriggerEnter2D(Collider2D collision) => ReportIfTagMatches(collision);

    void ReportIfTagMatches(Component other)
    {
        if (colliderTags != null && colliderTags.Any(x => other.CompareTag(x)))
            Report();
    }
}
