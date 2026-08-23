using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

// 퀘스트 트래커의 목표 한 줄. "전복 2/3" 처럼 진행도를 붙여 보여준다.
public class ObjectiveDescriptor : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;

    [SerializeField]
    Color normalColor;

    [FormerlySerializedAs("taskCompletionColor")]
    [SerializeField]
    Color completionColor;

    [FormerlySerializedAs("taskSuccessCountColor")]
    [SerializeField]
    Color countColor;

    [FormerlySerializedAs("strikeThrounghColor")]
    [SerializeField]
    Color strikeThroughColor;

    public void UpdateText(QuestObjective objective)
    {
        text.fontStyle = FontStyles.Normal;

        if (objective.IsComplete)
        {
            var colorCode = ColorUtility.ToHtmlStringRGB(completionColor);
            text.text = BuildText(objective, colorCode, colorCode);
        }
        else
        {
            text.text = BuildText(objective,
                ColorUtility.ToHtmlStringRGB(normalColor),
                ColorUtility.ToHtmlStringRGB(countColor));
        }
    }

    public void UpdateTextUsingStrikeThrough(QuestObjective objective)
    {
        var colorCode = ColorUtility.ToHtmlStringRGB(strikeThroughColor);
        text.fontStyle = FontStyles.Strikethrough;
        text.text = BuildText(objective, colorCode, colorCode);
    }

    // 대화/장소 목표는 세는 것이 아니라 한 번 하면 끝이므로 진행도를 붙이지 않는다.
    static bool ShowsCount(ObjectiveType type)
        => type != ObjectiveType.Dialogue && type != ObjectiveType.Location && type != ObjectiveType.Wear;

    string BuildText(QuestObjective objective, string textColorCode, string countColorCode)
    {
        if (!ShowsCount(objective.Type))
            return $"<color=#{textColorCode}>{objective.Description}</color>";

        return $"<color=#{textColorCode}>· {objective.Description} " +
               $"<color=#{countColorCode}>{objective.CurrentCount}</color>/{objective.NeedCount}</color>";
    }
}
