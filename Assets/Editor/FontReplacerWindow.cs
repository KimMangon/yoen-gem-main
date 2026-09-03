// FontReplacerWindow.cs
// 프로젝트 내 모든 씬 + 프리팹에서 레거시 UI Text 폰트를 일괄 교체하는 에디터 툴.
//
// 넣는 위치: 반드시 "Assets/Editor" 폴더 안에 넣어야 합니다.
//           (폴더 이름이 정확히 Editor 여야 유니티가 "빌드에는 포함하지 않는 에디터 전용 코드"로 인식합니다)
//
// 사용법:
// 1. Assets 밑에 Editor 폴더가 없다면 새로 만든다 (우클릭 > Create > Folder > 이름을 Editor로)
// 2. 이 파일을 Assets/Editor 폴더 안에 넣는다
// 3. 유니티가 컴파일할 때까지 잠깐 기다린다 (우측 하단에 빙글빙글 도는 아이콘)
// 4. 상단 메뉴 Tools > 폰트 일괄 교체 클릭
// 5. 새로 뜬 창에 넷마블체 Font 에셋을 드래그해서 넣고 "프로젝트 전체 교체" 클릭
//
// 주의: 씬/프리팹 파일을 직접 열고 저장하므로 되돌리기 어렵습니다.
//      실행 전 반드시 Git 커밋 등으로 백업해두세요.

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FontReplacerWindow : EditorWindow
{
    private Font newFont;
    private bool includeInactive = true;
    private bool scanScenes = true;
    private bool scanPrefabs = true;

    [MenuItem("Tools/폰트 일괄 교체")]
    public static void ShowWindow()
    {
        GetWindow<FontReplacerWindow>("폰트 일괄 교체");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "레거시 UI Text 컴포넌트를 프로젝트 전체(씬+프리팹)에서 찾아 폰트를 교체합니다.",
            MessageType.Info);

        GUILayout.Space(8);
        GUILayout.Label("새로 적용할 폰트 (Assets에 임포트된 Font 에셋)", EditorStyles.boldLabel);
        newFont = (Font)EditorGUILayout.ObjectField(newFont, typeof(Font), false);

        GUILayout.Space(12);
        scanScenes = EditorGUILayout.Toggle("빌드 세팅에 등록된 씬 전부 스캔", scanScenes);
        scanPrefabs = EditorGUILayout.Toggle("Assets 내 프리팹 전부 스캔", scanPrefabs);
        includeInactive = EditorGUILayout.Toggle("비활성화된 오브젝트도 포함", includeInactive);

        GUILayout.Space(16);

        using (new EditorGUI.DisabledScope(newFont == null))
        {
            if (GUILayout.Button("현재 열려있는 씬에서만 교체", GUILayout.Height(28)))
            {
                ReplaceInCurrentScene();
            }

            GUILayout.Space(4);

            if (GUILayout.Button("프로젝트 전체 교체 (씬 + 프리팹)", GUILayout.Height(32)))
            {
                if (EditorUtility.DisplayDialog(
                        "전체 교체 확인",
                        "프로젝트 내 모든 씬과 프리팹을 열고 저장합니다.\n" +
                        "되돌릴 수 없으니 미리 커밋/백업했는지 확인하세요. 계속할까요?",
                        "계속", "취소"))
                {
                    ReplaceInProject();
                }
            }
        }
    }

    private int ReplaceInGameObject(GameObject root)
    {
        int count = 0;

        foreach (var t in root.GetComponentsInChildren<Text>(includeInactive))
        {
            if (t.font != newFont)
            {
                Undo.RecordObject(t, "Replace Font");
                t.font = newFont;
                EditorUtility.SetDirty(t);
                count++;
            }
        }

        return count;
    }

    private void ReplaceInCurrentScene()
    {
        var scene = SceneManager.GetActiveScene();
        int total = scene.GetRootGameObjects().Sum(ReplaceInGameObject);
        if (total > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }
        Debug.Log($"[FontReplacer] 현재 씬 '{scene.name}'에서 {total}개 텍스트 컴포넌트 교체 완료.");
    }

    private void ReplaceInProject()
    {
        int total = 0;
        string originalScenePath = SceneManager.GetActiveScene().path;

        int skipped = 0;

        if (scanScenes)
        {
            var sceneGuids = AssetDatabase.FindAssets("t:Scene");
            foreach (var guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Assets 폴더 밖(Packages/, 읽기전용 패키지 캐시 등)에 있는 씬은 건드리지 않음
                if (!path.StartsWith("Assets/"))
                {
                    skipped++;
                    continue;
                }

                UnityEngine.SceneManagement.Scene scene;
                try
                {
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[FontReplacer] 씬 '{path}' 열기 실패, 건너뜀: {e.Message}");
                    skipped++;
                    continue;
                }

                int sceneCount = scene.GetRootGameObjects().Sum(ReplaceInGameObject);
                total += sceneCount;

                if (sceneCount > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
                Debug.Log($"[FontReplacer] 씬 '{path}' : {sceneCount}개 교체");
            }

            if (!string.IsNullOrEmpty(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }
        }

        if (scanPrefabs)
        {
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!path.StartsWith("Assets/"))
                {
                    skipped++;
                    continue;
                }

                GameObject prefabRoot;
                try
                {
                    prefabRoot = PrefabUtility.LoadPrefabContents(path);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[FontReplacer] 프리팹 '{path}' 열기 실패, 건너뜀: {e.Message}");
                    skipped++;
                    continue;
                }

                int prefabCount = ReplaceInGameObject(prefabRoot);

                if (prefabCount > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                    total += prefabCount;
                    Debug.Log($"[FontReplacer] 프리팹 '{path}' : {prefabCount}개 교체");
                }

                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        if (skipped > 0)
        {
            Debug.Log($"[FontReplacer] 읽기 전용 패키지 등에 있어 건너뛴 씬/프리팹: {skipped}개");
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("완료", $"총 {total}개 텍스트 컴포넌트의 폰트를 교체했습니다.\n(자세한 내역은 Console 창 참고)", "확인");
        Debug.Log($"[FontReplacer] 전체 완료: 총 {total}개 텍스트 컴포넌트 교체됨.");
    }
}
#endif
