using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// 기획 시트("해녀의 꿈 데이터 관리" > 퀘스트 리스트)에서 내보낸 csv를 읽어
// Quest 에셋의 밸런싱 수치를 갱신한다.
//
// 참조(타겟 문자열, 보상 에셋, 선행 퀘스트)는 건드리지 않는다. 그건 에디터에서
// 손으로 잇는 것이 맞고, 문자열로 관리하면 오타가 조용히 통과하기 때문이다.
// 런타임은 이 csv를 읽지 않는다 — 임포트 시점에 SO로 들어간다.
public class QuestSheetImporter : EditorWindow
{
    const string DefaultCsvPath = "Assets/Resources/quest.csv";

    // 시트 컬럼 (0-based). 헤더는 3번째 줄(index 2)에 있다.
    const int ColSheetId    = 3;
    const int ColDisplayName= 5;
    const int ColItemName   = 7;
    const int ColItemCount  = 8;
    const int HeaderRow     = 2;

    TextAsset csvAsset;
    Vector2 scroll;
    string report = "";

    [MenuItem("Tools/퀘스트 시트 임포터")]
    static void Open() => GetWindow<QuestSheetImporter>("퀘스트 시트 임포터");

    void OnEnable()
    {
        if (csvAsset == null)
            csvAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(DefaultCsvPath);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("기획 시트 -> Quest 에셋", EditorStyles.boldLabel);
        csvAsset = (TextAsset)EditorGUILayout.ObjectField("퀘스트 csv", csvAsset, typeof(TextAsset), false);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(csvAsset == null))
        {
            if (GUILayout.Button("1. 검증만 하기"))
                report = Run(dryRun: true, autoLink: false);

            if (GUILayout.Button("2. 이름으로 sheetId 자동 연결"))
                report = Run(dryRun: false, autoLink: true);

            if (GUILayout.Button("3. 밸런싱 값 임포트 (이름 / 채집 갯수)"))
                report = Run(dryRun: false, autoLink: false);
        }

        EditorGUILayout.Space();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.TextArea(report, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    // ------------------------------------------------------------------ 시트 행
    class SheetQuest
    {
        public string sheetId;
        public string displayName;
        public readonly List<KeyValuePair<string, int>> collects = new List<KeyValuePair<string, int>>();
    }

    List<SheetQuest> ParseSheet(string csv)
    {
        var rows = CsvUtility.Parse(csv);
        var result = new List<SheetQuest>();
        SheetQuest current = null;

        for (int r = HeaderRow + 1; r < rows.Count; r++)
        {
            var row = rows[r];
            string id = Cell(row, ColSheetId);

            // ID가 있으면 새 퀘스트, 없으면 앞 퀘스트의 추가 해산물 줄
            if (!string.IsNullOrEmpty(id))
            {
                current = new SheetQuest { sheetId = id, displayName = Cell(row, ColDisplayName) };
                result.Add(current);
            }

            if (current == null)
                continue;

            string itemName = Cell(row, ColItemName);
            if (!string.IsNullOrEmpty(itemName) && itemName != "-"
                && int.TryParse(Cell(row, ColItemCount), out int count) && count > 0)
            {
                current.collects.Add(new KeyValuePair<string, int>(Normalize(itemName), count));
            }
        }
        return result;
    }

    static string Cell(IList<string> row, int i) => i < row.Count ? row[i].Trim() : "";

    // 시트는 공백/줄바꿈이 들쭉날쭉하다. 비교할 때만 정규화한다.
    static string Normalize(string s)
        => string.IsNullOrEmpty(s) ? "" : new string(s.Where(c => !char.IsWhiteSpace(c)).ToArray());

    // ------------------------------------------------------------------ 본체
    string Run(bool dryRun, bool autoLink)
    {
        var sheet = ParseSheet(csvAsset.text);
        var quests = AssetDatabase.FindAssets("t:Quest")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<Quest>)
            .Where(x => x != null)
            .OrderBy(x => x.name)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"시트 퀘스트 {sheet.Count}개 / 프로젝트 Quest 에셋 {quests.Count}개");
        sb.AppendLine();

        // 시트 쪽 중복 ID
        foreach (var dup in sheet.GroupBy(x => x.sheetId).Where(g => g.Count() > 1))
            sb.AppendLine($"[시트] ID 중복: {dup.Key} ({dup.Count()}행)");

        var bySheetId = sheet.GroupBy(x => x.sheetId).ToDictionary(g => g.Key, g => g.First());
        var byName = sheet.GroupBy(x => Normalize(x.displayName))
                          .ToDictionary(g => g.Key, g => g.First());

        int linked = 0, updated = 0;
        var unmatchedAssets = new List<string>();

        foreach (var quest in quests)
        {
            var so = new SerializedObject(quest);
            string sheetId = so.FindProperty("sheetId").stringValue;
            string display = so.FindProperty("displayName").stringValue;

            // sheetId가 비었으면 이름으로 후보를 찾는다
            if (string.IsNullOrEmpty(sheetId))
            {
                if (!string.IsNullOrEmpty(display) && byName.TryGetValue(Normalize(display), out var guess))
                {
                    if (autoLink)
                    {
                        so.FindProperty("sheetId").stringValue = guess.sheetId;
                        so.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(quest);
                        sheetId = guess.sheetId;
                        linked++;
                        sb.AppendLine($"[연결] {quest.name} -> {guess.sheetId} (\"{guess.displayName}\")");
                    }
                    else
                    {
                        sb.AppendLine($"[연결 후보] {quest.name} -> {guess.sheetId} (\"{guess.displayName}\")  * 2번 버튼으로 연결");
                        continue;
                    }
                }
                else
                {
                    unmatchedAssets.Add($"{quest.name} (displayName: \"{display}\")");
                    continue;
                }
            }

            if (!bySheetId.TryGetValue(sheetId, out var row))
            {
                sb.AppendLine($"[없음] {quest.name}: sheetId \"{sheetId}\" 가 시트에 없음");
                continue;
            }

            if (autoLink)
                continue;   // 연결만 하는 단계

            if (ApplyRow(quest, so, row, dryRun, sb))
                updated++;
        }

        // 시트에는 있는데 에셋이 없는 퀘스트
        var assetSheetIds = new HashSet<string>(
            quests.Select(q => new SerializedObject(q).FindProperty("sheetId").stringValue)
                  .Where(x => !string.IsNullOrEmpty(x)));

        var missing = sheet.Where(x => !assetSheetIds.Contains(x.sheetId)).ToList();
        if (missing.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"--- 시트에만 있는 퀘스트 {missing.Count}개 (에셋 미제작) ---");
            foreach (var m in missing)
                sb.AppendLine($"  {m.sheetId}  {m.displayName}");
        }

        if (unmatchedAssets.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"--- 시트에 대응이 없는 에셋 {unmatchedAssets.Count}개 ---");
            foreach (var m in unmatchedAssets)
                sb.AppendLine("  " + m);
        }

        if (!dryRun)
        {
            AssetDatabase.SaveAssets();
            sb.AppendLine();
            sb.AppendLine(autoLink ? $"연결 {linked}개 완료" : $"갱신 {updated}개 완료");
        }

        return sb.ToString();
    }

    // 시트 한 행을 Quest 에셋에 반영. 바꾼 게 있으면 true.
    bool ApplyRow(Quest quest, SerializedObject so, SheetQuest row, bool dryRun, StringBuilder sb)
    {
        bool changed = false;

        var displayProp = so.FindProperty("displayName");
        if (!string.IsNullOrEmpty(row.displayName) && displayProp.stringValue != row.displayName)
        {
            sb.AppendLine($"[이름] {quest.name}: \"{displayProp.stringValue}\" -> \"{row.displayName}\"");
            if (!dryRun)
                displayProp.stringValue = row.displayName;
            changed = true;
        }

        // 채집 목표의 갯수를 맞춘다. 목표의 description(한글 해산물 이름)으로 대응시킨다.
        var wanted = row.collects.ToDictionary(x => x.Key, x => x.Value);
        var seen = new HashSet<string>();

        var steps = so.FindProperty("steps");
        for (int s = 0; s < steps.arraySize; s++)
        {
            var objectives = steps.GetArrayElementAtIndex(s).FindPropertyRelative("objectives");
            for (int o = 0; o < objectives.arraySize; o++)
            {
                var obj = objectives.GetArrayElementAtIndex(o);
                if ((ObjectiveType)obj.FindPropertyRelative("type").enumValueIndex != ObjectiveType.Collect)
                    continue;

                string key = Normalize(obj.FindPropertyRelative("description").stringValue);
                var needProp = obj.FindPropertyRelative("needCount");

                if (!wanted.TryGetValue(key, out int want))
                {
                    sb.AppendLine($"[확인] {quest.name}: 채집 목표 \"{key}\" 가 시트 행에 없음");
                    continue;
                }

                seen.Add(key);
                if (needProp.intValue != want)
                {
                    sb.AppendLine($"[갯수] {quest.name} / {key}: {needProp.intValue} -> {want}");
                    if (!dryRun)
                        needProp.intValue = want;
                    changed = true;
                }
            }
        }

        foreach (var kv in wanted.Where(x => !seen.Contains(x.Key)))
            sb.AppendLine($"[누락] {quest.name}: 시트의 \"{kv.Key}\" x{kv.Value} 에 해당하는 목표가 에셋에 없음");

        if (changed && !dryRun)
        {
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(quest);
        }
        return changed;
    }
}

// 따옴표 안의 쉼표/줄바꿈을 처리하는 최소 CSV 파서
static class CsvUtility
{
    public static List<List<string>> Parse(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else inQuotes = false;
                }
                else field.Append(c);
                continue;
            }

            switch (c)
            {
                case '"':
                    inQuotes = true;
                    break;
                case ',':
                    row.Add(field.ToString()); field.Clear();
                    break;
                case '\r':
                    break;
                case '\n':
                    row.Add(field.ToString()); field.Clear();
                    rows.Add(row); row = new List<string>();
                    break;
                default:
                    field.Append(c);
                    break;
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }
        return rows;
    }
}
