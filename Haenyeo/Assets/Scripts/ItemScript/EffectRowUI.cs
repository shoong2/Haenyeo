using UnityEngine;
using TMPro;

// 상세 패널 "보유 효과" 목록의 한 줄 (알약 프리팹에 부착)
public class EffectRowUI : MonoBehaviour
{
    [SerializeField] TMP_Text labelText;   // 왼쪽 "보유 효과 설명"
    [SerializeField] TMP_Text valueText;   // 오른쪽 "+ 10" / "% 10" / "- 10"

    // 효과 하나를 받아 한 줄을 채운다
    public void Set(StatEffect effect)
    {
        labelText.text = effect.description;
        valueText.text = FormatValue(effect);
    }

    // op 종류에 따라 표시 문자열 생성
    string FormatValue(StatEffect effect)
    {
        string num = Mathf.Abs(effect.value).ToString("0.##"); // 정수면 "10", 소수면 "10.5"

        switch (effect.op)
        {
            case EffectOp.Percent:
                return $"% {num}";                              // 스크린샷 기준 "% 10"
            case EffectOp.Add:
            default:
                return effect.value >= 0 ? $"+ {num}" : $"- {num}";
        }
    }
}
