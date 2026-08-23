using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 해변 씬에 어떤 퀘스트일 때 누가 서 있는지.
//
// 예전에는 이 정보가 Quest SO의 people 필드에 있었지만, Beach 씬에서만 쓰는
// 씬 배치 데이터라 퀘스트가 들고 있을 이유가 없었다.
// (원칙: GameObject 배치는 씬이 소유, 퀘스트는 진행 상태만 소유)
[CreateAssetMenu(menuName = "Quest/BeachCastTable", fileName = "BeachCastTable")]
public class BeachCastTable : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string questId;
        public string[] people;
    }

    [SerializeField]
    Entry[] entries;

    [Header("등장 인원 수별 서는 위치")]
    [SerializeField]
    Vector2[] positionsFor3;
    [SerializeField]
    Vector2[] positionsFor4;
    [SerializeField]
    Vector2[] positionsForOther;

    public string[] FindPeople(string questId)
        => entries?.FirstOrDefault(x => x.questId == questId)?.people;

    public IReadOnlyList<Vector2> GetPositions(int peopleCount)
    {
        if (peopleCount == 3) return positionsFor3;
        if (peopleCount == 4) return positionsFor4;
        return positionsForOther;
    }
}
