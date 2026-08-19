// 퀘스트 목표의 종류.
// 예전에는 Category ScriptableObject 에셋 8개로 표현했지만, 값이 codeName 문자열
// 하나뿐이라 오타가 조용히 무시됐다. enum으로 바꿔서 인스펙터 드롭다운 + 컴파일 검사를 받는다.
//
// 리포터(QuestReporter 등)와 QuestObjective의 값이 같아야 보고가 전달된다.
public enum ObjectiveType
{
    Dialogue,   // NPC와 대화
    Collect,    // 해산물 채집
    Location,   // 특정 장소·깊이 도달
    Wear,       // 옷/장비 착용
    Click,      // 오브젝트 클릭
    Kill        // 처치
}

public enum ObjectiveState
{
    Inactive,
    Running,
    Complete
}
