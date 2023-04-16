using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JackysEditorHelpers;

namespace MultiMouseUnity.Editor
{
    public class MultiMouseInputDebugger : EditorWindow
    {
        static MultiMouseInputDebugger window;
        public static MultiMouseInputDebugger Window
        {
            get
            {
                if (window == null) window = GetWindow<MultiMouseInputDebugger>();
                return window;
            }
        }

        [MenuItem("MultiMouse/Input Debugger")]
        private static void ShowWindow()
        {
            window = GetWindow<MultiMouseInputDebugger>();
            window.titleContent = new GUIContent("MultiMouse Input Debugger");
            window.Show();
        }

        private void OnInspectorUpdate()
        {
            if (!Application.isPlaying)
            {

                return;
            }

            if (!MultiMouseWrapper.Initialized)
            {

                return;
            }

            Repaint();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Please enter Play Mode to begin receiving input!", MessageType.Info);
                return;
            }

            if (!MultiMouseWrapper.Initialized)
            {
                EditorGUILayout.HelpBox("MultiMouse not Initialized.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            for (int i = 0; i < 14; i++)
            {
                var device = MultiMouseWrapper.Instance.TryGetDeviceAtIndex(i);
                if (device != null)
                {
                    EditorGUILayout.LabelField("Device ID", i.ToString());
                    EditorGUILayout.LabelField("Name", device.DeviceID);
                    var pos = MultiMouseWrapper.Instance.GetMousePosition(i);
                    EditorGUILayout.LabelField("Position", pos.ToString());
                    EditorGUILayout.LabelField("Is Lightgun?", device.IsLightgun.ToString());

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Buttons");
                    var rect = EditorGUILayout.GetControlRect();
                    EditorGUILayout.EndHorizontal();

                    float width = 1 / 3f;

                    for (int j = 0; j < 3; j++)
                    {
                        var boxRect = new Rect(rect);
                        boxRect.xMin = Mathf.Lerp(rect.xMin, rect.xMax, width * j) + 5;
                        boxRect.xMax = Mathf.Lerp(rect.xMin, rect.xMax, width * (j + 1)) - 5;

                        var down = MultiMouseWrapper.Instance.GetMouseButton(i, j);

                        if (down) EditorHelper.BeginColourChange(Color.green);
                        GUI.Box(boxRect, j.ToString());
                        if (down) EditorHelper.EndColourChange();
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}