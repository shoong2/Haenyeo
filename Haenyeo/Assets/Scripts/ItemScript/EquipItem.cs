using UnityEngine;

// 스탯 종류 (스테미나 = UnderSeaGameManager.maxHp, Buff는 아직 미구현/자리만)
public enum StatType { Stamina, Buff }

// 효과 연산 방식: Add = +값, Percent = %값
public enum EffectOp { Add, Percent }

// 상세 패널의 "보유 효과" 한 줄 + 실제 스탯 적용 값
[System.Serializable]
public class StatEffect
{
    public StatType stat;          // 어떤 스탯을 올릴지
    public EffectOp op;            // + 인지 % 인지
    public float value;            // 10, -10 ...
    [TextArea] public string description; // "보유 효과 설명" 표시 텍스트
}

// 장착 시 바꿀 캐릭터 부위 하나 (clothes.spriteLib 기준)
[System.Serializable]
public class PartSwap
{
    public string category;        // "body", "armL", "armR", "legL", "legR", "glassA" ...
    public string label;           // "C1", "C2", "G1", "F1" ...
}

[CreateAssetMenu(fileName = "New Equip", menuName = "New Item/equip")]
public class EquipItem : Item
{
    [Header("상세 정보")]
    [TextArea] public string description;   // 상세 패널 "내용"

    [Header("보유 효과")]
    public StatEffect[] effects;            // 상세 패널 효과 목록 + 스탯 적용

    [Header("스프라이트 스왑")]
    public PartSwap[] swaps;                // 장착 시 바꿀 부위들
}
