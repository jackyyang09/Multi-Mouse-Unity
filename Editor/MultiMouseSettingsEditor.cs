using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MultiMouseUnity.Editor
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MultiMouseSettings))]
    public class MultiMouseSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Settings"))
            {
                SettingsService.OpenProjectSettings(MultiMouseConstants.MENU_SETTINGS);
            }
        }
    }
#endif
}