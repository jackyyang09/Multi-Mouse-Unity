using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiMouseUnity
{
    public class MultiMouseSettings : ScriptableObject
    {
        public bool detectDevicesOnStart = true;
        public bool keepDetectingUntilFull = true;
        public bool unclampMousePosition;
        public int maxDevices;
        public int maxMouseButtons;

#if UNITY_EDITOR
        [SerializeField] protected string packagePath;
        public string PackagePath
        {
            get
            {
                if (!AssetDatabase.IsValidFolder(packagePath))
                {
                    CacheProjectPath();
                }
                return packagePath;
            }
        }

        void CacheProjectPath()
        {
            var guids = AssetDatabase.FindAssets(nameof(MultiMouseSettings));
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            packagePath = path.Remove(path.IndexOf("/Editor/" + nameof(MultiMouseSettings) + ".cs"));
        }

        public void Reset()
        {
            Undo.RecordObject(this, "Reset MultiMouse Settings");
            detectDevicesOnStart = true;
            maxDevices = 4;
            keepDetectingUntilFull = true;
            maxMouseButtons = 4;
        }
#endif

        static MultiMouseSettings settings;
        public static MultiMouseSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    var asset = Resources.Load("MultiMouseSettings");
                    settings = asset as MultiMouseSettings;
#if UNITY_EDITOR
                    if (settings == null) TryCreateNewSettingsAsset();
#endif
                }
                return settings;
            }
        }

#if UNITY_EDITOR
        static readonly string SETTINGS_PATH = "Assets/Settings/Resources/" + nameof(MultiMouseSettings) + ".asset";

        public static void TryCreateNewSettingsAsset()
        {
            if (!EditorUtility.DisplayDialog(
                "Multi Mouse First Time Setup",
                "In order to function, Multi Mouse needs a place to store settings. By default, a " +
                "Settings asset will be created at Assets/Settings/Resources/, but you may move it " +
                "elsewhere, so long as it's in a Resources folder.\n " +
                "Moving it out of the Resources folder will prompt this message to appear again erroneously!",
                "Ok Create It.", "Not Yet!")) return;

            var asset = CreateInstance<MultiMouseSettings>();
            if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");
            if (!AssetDatabase.IsValidFolder("Assets/Settings/Resources")) AssetDatabase.CreateFolder("Assets/Settings", "Resources");
            AssetDatabase.CreateAsset(asset, SETTINGS_PATH);
            asset.Reset();

            settings = asset;
        }

        static SerializedObject serializedObject;
        public static SerializedObject SerializedObject
        {
            get
            {
                if (serializedObject == null)
                {
                    serializedObject = new SerializedObject(Settings);
                    return serializedObject;
                }
                return serializedObject;
            }
        }
#endif
    }
}