using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneLoader : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] SceneAsset[] sceneAssets;

    // Start is called before the first frame update
    void Start()
    {
        var scenes = new List<EditorBuildSettingsScene>();
        var paths = new List<string>();
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            scenes.Add(EditorBuildSettings.scenes[i]);
            paths.Add(EditorBuildSettings.scenes[i].path);
        }

        foreach (var sceneAsset in sceneAssets)
        {
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            if (!string.IsNullOrEmpty(scenePath) && !paths.Contains(scenePath))
            {
                paths.Add(scenePath);
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
        }

        // Set the Build Settings window Scene list
        EditorBuildSettings.scenes = scenes.ToArray();
    }
#endif
}