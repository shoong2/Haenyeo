using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// 퀘스트 에셋이 실제로 어떻게 읽혔는지 그대로 뱉는다.
// 마이그레이션이나 임포트 뒤에 데이터가 제대로 들어갔는지 눈으로 확인할 때 쓴다.
public static class QuestStructureDump
{
    const string OutPath = "QuestDump.txt";   // 프로젝트 루트

    [MenuItem("Tools/퀘스트 구조 덤프")]
    static void Dump()
    {
        var quests = AssetDatabase.FindAssets("t:Quest")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<Quest>)
            .Where(x => x != null)
            .OrderBy(x => x.name)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"Quest 에셋 {quests.Count}개");
        sb.AppendLine();

        int totalObjectives = 0, problems = 0;

        foreach (var q in quests)
        {
            sb.AppendLine($"[{q.name}]");
            sb.AppendLine($"  questId={q.QuestId}  sheetId={(string.IsNullOrEmpty(q.SheetId) ? "-" : q.SheetId)}");
            sb.AppendLine($"  displayName=\"{q.DisplayName}\"  timeLimit={q.TimeLimitSeconds}  notify={q.ShowNotification}");

            var prereqProp = new SerializedObject(q).FindProperty("prerequisite").objectReferenceValue;
            sb.AppendLine($"  선행={(prereqProp != null ? prereqProp.name : "-")}  완료보상={q.Rewards.Count}개");

            if (q.Steps == null || q.Steps.Count == 0)
            {
                sb.AppendLine("  !! steps 가 비어 있음");
                problems++;
            }
            else
            {
                for (int s = 0; s < q.Steps.Count; s++)
                {
                    var step = q.Steps[s];
                    sb.AppendLine($"  단계 {s + 1} (보상 {step.Rewards.Count}개)");

                    if (step.Objectives.Count == 0)
                    {
                        sb.AppendLine("    !! 목표 없음");
                        problems++;
                    }

                    foreach (var o in step.Objectives)
                    {
                        totalObjectives++;
                        string targets = o.GetType()
                            .GetField("targets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.GetValue(o) is string[] arr && arr.Length > 0
                            ? string.Join(", ", arr)
                            : "(없음)";

                        sb.AppendLine($"    - {o.Type,-8} \"{o.Description}\"  x{o.NeedCount}  targets=[{targets}]");

                        if (targets == "(없음)")
                        {
                            sb.AppendLine("      !! targets 가 비어 있음 — 이 목표는 절대 완료되지 않는다");
                            problems++;
                        }
                        if (o.NeedCount <= 0)
                        {
                            sb.AppendLine("      !! needCount 가 0 이하");
                            problems++;
                        }
                    }
                }
            }
            sb.AppendLine();
        }

        sb.AppendLine($"목표 총 {totalObjectives}개 / 문제 {problems}건");

        File.WriteAllText(OutPath, sb.ToString(), new UTF8Encoding(false));
        Debug.Log($"[QuestStructureDump] {Path.GetFullPath(OutPath)} 에 기록. 목표 {totalObjectives}개, 문제 {problems}건");
    }
}
