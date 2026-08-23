using UnityEngine;

// 기획 시트의 "퀘스트 리워드 #N - buff". 장비 아이템의 능력치를 올린다.
// 아직 게임플레이에 적용하지 않는다 — EquipItem.effects를 스탯에 반영하는 작업과 함께 붙인다.
[CreateAssetMenu(menuName = "Quest/Reward/Buff", fileName = "BuffReward_")]
public class BuffReward : Reward
{
    [SerializeField]
    StatType stat;

    [Tooltip("시트의 a값. 0.02 = 2%")]
    [SerializeField]
    float amount;

    [Tooltip("이 아이템의 능력치를 올린다")]
    [SerializeField]
    Item targetItem;

    public StatType Stat => stat;
    public float Amount => amount;
    public Item TargetItem => targetItem;

    public override void Give(Quest quest)
    {
        // TODO: 스탯 시스템이 생기면 여기서 적용한다.
        Debug.Log($"[BuffReward] 미적용 - {stat} +{amount:P0} ({(targetItem != null ? targetItem.name : "대상 없음")})");
    }
}
