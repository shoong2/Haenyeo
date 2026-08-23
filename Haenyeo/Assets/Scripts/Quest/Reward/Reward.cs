using System.Collections.Generic;
using UnityEngine;

// 퀘스트/단계 완료 보상.
// 기획 시트의 리워드 타입은 단순획득(Item) / buff / Recipe 세 가지다. XP는 기획에 없다.
public abstract class Reward : ScriptableObject
{
    [SerializeField]
    Sprite icon;

    [Tooltip("알림창에 띄울 문구")]
    [SerializeField]
    string description;

    public Sprite Icon => icon;
    public string Description => description;

    public abstract void Give(Quest quest);

    // 알림창에 띄울 아이콘들. 아이템 보상은 아이템별 아이콘을 돌려준다.
    public virtual IReadOnlyList<Sprite> GetIcons()
        => icon != null ? new[] { icon } : System.Array.Empty<Sprite>();
}
