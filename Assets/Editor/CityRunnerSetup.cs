#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Creates Assets/Scenes/GamePlay.unity once (the game also runs from any empty scene).
public static class CityRunnerSetup
{
    const string ScenePath = "Assets/Scenes/GamePlay.unity";

    [InitializeOnLoadMethod]
    static void AutoCreate()
    {
        EditorApplication.delayCall += () =>
        {
            if (Application.isPlaying || File.Exists(ScenePath)) return;
            if (Directory.Exists("Assets/Scenes") && Directory.GetFiles("Assets/Scenes", "*.unity").Length > 0) return;
            try { CreateScene(); } catch (System.Exception) { }
        };
    }

    [MenuItem("CityRunner/Create GamePlay Scene")]
    public static void CreateScene()
    {
        Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.Refresh();
    }
}
#endif
