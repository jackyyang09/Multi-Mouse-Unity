using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MultiMouseUnity.Editor
{
    public class MultiMouseSettingsProvider : SettingsProvider
    {
        public MultiMouseSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
        {
        }

        SerializedProperty
            maxDevices,
            keepDetectingUntilFull,
            maxMouseButtons;

        protected MultiMouseSettings Settings => MultiMouseSettings.Settings;
        protected SerializedObject SerializedObject => MultiMouseSettings.SerializedObject;

        protected SerializedProperty FindProp(string prop) => SerializedObject.FindProperty(prop);
        void FindSerializedProperties()
        {
            maxDevices = FindProp(nameof(maxDevices));
            keepDetectingUntilFull = FindProp(nameof(keepDetectingUntilFull));
            maxMouseButtons = FindProp(nameof(maxMouseButtons));
        }

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            FindSerializedProperties();
        }

        public override void OnGUI(string searchContext)
        {
            // This makes prefix labels larger
            EditorGUIUtility.labelWidth += 50;

            EditorGUILayout.PropertyField(maxDevices);
            EditorGUILayout.PropertyField(keepDetectingUntilFull);
            EditorGUILayout.PropertyField(maxMouseButtons);

            if (GUILayout.Button("Reset to Default", new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            {
                Settings.Reset();
            }

            SerializedObject.ApplyModifiedProperties();

            EditorGUIUtility.labelWidth -= 50;
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new MultiMouseSettingsProvider(MultiMouseConstants.MENU_SETTINGS, SettingsScope.Project);
            provider.keywords = GetSearchKeywordsFromSerializedObject(MultiMouseSettings.SerializedObject);

            return provider;
        }
    }
}