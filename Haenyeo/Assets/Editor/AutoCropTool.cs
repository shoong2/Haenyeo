using System.IO;
using UnityEditor;
using UnityEngine;

public class AutoCropTool : EditorWindow
{
    private float alphaThreshold = 0.01f;

    [MenuItem("Tools/Auto Crop Tool")]
    public static void OpenWindow()
    {
        GetWindow<AutoCropTool>("Auto Crop Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("선택한 이미지의 투명 여백을 자동으로 제거합니다.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        alphaThreshold = EditorGUILayout.Slider("Alpha 기준값", alphaThreshold, 0f, 0.1f);
        EditorGUILayout.Space();

        GUI.enabled = Selection.objects.Length > 0;
        if (GUILayout.Button("선택한 이미지 크롭", GUILayout.Height(40)))
            CropSelected();
        GUI.enabled = true;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Project 창에서 이미지를 선택한 뒤 버튼을 누르세요.\n원본 파일을 덮어씁니다.", MessageType.Info);
    }

    private void CropSelected()
    {
        int success = 0;
        int fail = 0;

        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (!path.EndsWith(".png") && !path.EndsWith(".jpg") && !path.EndsWith(".jpeg"))
                continue;

            if (CropImage(path))
                success++;
            else
                fail++;
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료",
            $"크롭 완료: {success}개\n실패: {fail}개", "확인");
    }

    private bool CropImage(string assetPath)
    {
        // Read/Write 임시 활성화
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        if (importer == null) return false;

        bool wasReadable = importer.isReadable;
        TextureImporterType originalType = importer.textureType;

        importer.isReadable = true;
        importer.textureType = TextureImporterType.Default;
        importer.SaveAndReimport();

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (tex == null) return false;

        Color[] pixels = tex.GetPixels();
        int w = tex.width;
        int h = tex.height;

        int minX = w, maxX = 0, minY = h, maxY = 0;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (pixels[y * w + x].a > alphaThreshold)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (minX > maxX || minY > maxY)
        {
            Debug.LogWarning($"[AutoCrop] 불투명 픽셀 없음: {assetPath}");
            return false;
        }

        int cropW = maxX - minX + 1;
        int cropH = maxY - minY + 1;

        Color[] cropped = new Color[cropW * cropH];
        for (int y = 0; y < cropH; y++)
            for (int x = 0; x < cropW; x++)
                cropped[y * cropW + x] = pixels[(minY + y) * w + (minX + x)];

        Texture2D result = new Texture2D(cropW, cropH, tex.format, false);
        result.SetPixels(cropped);
        result.Apply();

        string fullPath = Path.GetFullPath(assetPath);
        bool isPng = assetPath.EndsWith(".png");
        byte[] bytes = isPng ? result.EncodeToPNG() : result.EncodeToJPG();
        File.WriteAllBytes(fullPath, bytes);

        DestroyImmediate(result);

        // 원래 설정 복원
        importer.isReadable = wasReadable;
        importer.textureType = originalType;
        importer.SaveAndReimport();

        Debug.Log($"[AutoCrop] {Path.GetFileName(assetPath)} → {cropW}x{cropH} (원본 {w}x{h})");
        return true;
    }
}
