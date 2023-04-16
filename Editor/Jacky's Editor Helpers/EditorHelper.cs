using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JackysEditorHelpers
{
    public class EditorHelper : Editor
    {
        public static Color ButtonPressedColor => GUI.color.Add(new Color(0.2f, 0.2f, 0.2f, 0));
        public static Color ButtonColor => GUI.color.Subtract(new Color(0.2f, 0.2f, 0.2f, 0));

        public static string OpenSmartSaveFileDialog<T>(out T asset, string defaultName = "New Object", string startingPath = "Assets") where T : ScriptableObject
        {
            string savePath = EditorUtility.SaveFilePanel("Designate Save Path", startingPath, defaultName, "asset");
            asset = null;
            if (savePath != "") // Make sure user didn't press "Cancel"
            {
                asset = CreateInstance<T>();
                savePath = savePath.Remove(0, savePath.IndexOf("Assets/"));
                CreateAssetSafe(asset, savePath);
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }
            return savePath;
        }

        public static string OpenSmartSaveFileDialogInProject<T>(out T asset, string defaultName = "New Object") where T : ScriptableObject
        {
            string savePath = EditorUtility.SaveFilePanelInProject("Designate Save Path", defaultName, "asset", "Choose where to save your file");
            asset = null;
            if (savePath != "") // Make sure user didn't press "Cancel"
            {
                asset = CreateInstance<T>();
                savePath = savePath.Remove(0, savePath.IndexOf("Assets/"));
                CreateAssetSafe(asset, savePath);
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }
            return savePath;
        }

        /// <summary>
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="path"></param>
        /// <returns>False if the operation was unsuccessful or was cancelled, 
        /// True if an asset was created.</returns>
        public static bool CreateAssetSafe(Object asset, string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                Debug.LogError("Error! Attempted to write an asset over a folder!");
                return false;
            }
            string folderPath = path.Substring(0, path.LastIndexOf("/"));
            if (GenerateFolderStructureAt(folderPath))
            {
                AssetDatabase.CreateAsset(asset, path);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Generates the folder structure to a specified path if it doesn't already exist. 
        /// Will perform the check itself first
        /// </summary>
        /// <param name="folderPath">The FOLDER path, this should NOT include any file names</param>
        /// <param name="ask">Asks if you want to generate the folder structure</param>
        /// <returns>False if the user cancels the operation, 
        /// True if there was no need to generate anything or if the operation was successful</returns>
        public static bool GenerateFolderStructureAt(string folderPath, bool ask = true)
        {
            // Convert slashes so we can use the Equals operator together with other file-system operations
            folderPath = folderPath.Replace("/", "\\");
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string existingPath = "Assets";
                string unknownPath = folderPath.Remove(0, existingPath.Length + 1);
                // Remove the "Assets/" at the start of the path name
                string folderName = (unknownPath.Contains("\\")) ? unknownPath.Substring(0, (unknownPath.IndexOf("\\"))) : unknownPath;
                do
                {
                    string newPath = System.IO.Path.Combine(existingPath, folderName);
                    // Begin checking down the file path to see if it's valid
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        bool createFolder = true;
                        if (ask)
                        {
                            createFolder = EditorUtility.DisplayDialog("Path does not exist!", "The folder " +
                                "\"" +
                                newPath +
                                "\" does not exist! Would you like to create this folder?", "Yes", "No");
                        }

                        if (createFolder)
                        {
                            AssetDatabase.CreateFolder(existingPath, folderName);
                        }
                        else return false;
                    }
                    existingPath = newPath;
                    // Full path still doesn't exist
                    if (!existingPath.Equals(folderPath))
                    {
                        unknownPath = unknownPath.Remove(0, folderName.Length + 1);
                        folderName = (unknownPath.Contains("\\")) ? unknownPath.Substring(0, (unknownPath.IndexOf("\\"))) : unknownPath;
                    }
                }
                while (!AssetDatabase.IsValidFolder(folderPath));
            }
            return true;
        }

        public static void RenderSmartFolderProperty(GUIContent content, SerializedProperty folderProp, bool limitToAssetFolder = true, string panelTitle = "Specify a New Folder")
        {
            EditorGUILayout.BeginHorizontal();
            SmartAssetField(content, folderProp, limitToAssetFolder);
            SmartBrowseButton(folderProp, limitToAssetFolder, panelTitle);
            EditorGUILayout.EndHorizontal();
        }

        public static void RenderSmartFileProperty(GUIContent content, SerializedProperty folderProp, string extension, bool limitToAssetFolder = true, string panelTitle = "Specify a New Folder")
        {
            EditorGUILayout.BeginHorizontal();
            SmartAssetField(content, folderProp, limitToAssetFolder);
            if (!limitToAssetFolder) 
                SmartBrowseButton(folderProp, extension, limitToAssetFolder, panelTitle);
            EditorGUILayout.EndHorizontal();
        }

        public static void SmartAssetField(GUIContent content, SerializedProperty folderProp, bool limitToAssetsFolder = true)
        {
            string folderPath = folderProp.stringValue;
            //if (folderPath == string.Empty) folderPath = Application.dataPath;
            bool touchedFolder = false;
            bool touchedString = false;

            Rect rect = EditorGUILayout.GetControlRect();
            if (DragAndDropRegion(rect, "", ""))
            {
                DefaultAsset da = DragAndDrop.objectReferences[0] as DefaultAsset;
                if (da) folderProp.stringValue = AssetDatabase.GetAssetPath(da);
                return;
            }
            if (limitToAssetsFolder) rect.width *= 2f / 3f;
            EditorGUI.BeginChangeCheck();
            folderPath = EditorGUI.TextField(rect, content, folderPath);
            touchedString = EditorGUI.EndChangeCheck();

            rect.position += new Vector2(rect.width + 5, 0);
            rect.width = rect.width / 2f - 5;

            if (limitToAssetsFolder)
            {
                DefaultAsset folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);
                EditorGUI.BeginChangeCheck();
                folderAsset = (DefaultAsset)EditorGUI.ObjectField(rect, GUIContent.none, folderAsset, typeof(DefaultAsset), false);
                if (EditorGUI.EndChangeCheck())
                {
                    touchedFolder = true;
                    folderPath = AssetDatabase.GetAssetPath(folderAsset);
                }
            }

            if (touchedString || touchedFolder)
            {
                // If the user presses "cancel"
                if (folderPath.Equals(string.Empty))
                {
                    return;
                }
                // or specifies something outside of this folder, reset filePath and don't proceed
                else if (limitToAssetsFolder)
                {
                    if (!folderPath.Contains("Assets"))
                    {
                        EditorUtility.DisplayDialog("Folder Browsing Error!", "Please choose a different folder inside the project's Assets folder.", "OK");
                        return;
                    }
                    else
                    {
                        // Fix path to be usable for AssetDatabase.FindAssets
                        if (folderPath[folderPath.Length - 1] == '/') folderPath = folderPath.Remove(folderPath.Length - 1, 1);
                    }
                }
            }
            folderProp.stringValue = folderPath;
        }

        public static void SmartBrowseButton(SerializedProperty pathProp, string extension = "", bool limitToAssetsFolder = true, string panelTitle = "Specify a New File")
        {
            GUIContent buttonContent = new GUIContent(" Browse ", "Designate a New Folder");
            if (GUILayout.Button(buttonContent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            {
                // Stop editing any fields
                EditorGUI.FocusTextInControl("");

                string filePath = pathProp.stringValue;
                filePath = EditorUtility.OpenFilePanel(panelTitle, filePath, extension);

                // If the user presses "cancel"
                if (filePath.Equals(string.Empty))
                {
                    return;
                }
                if (limitToAssetsFolder)
                {
                    // or specifies something outside of this folder, reset filePath and don't proceed
                    if (!filePath.Contains("Assets"))
                    {
                        EditorUtility.DisplayDialog("Folder Browsing Error!", "Please choose a different folder from" +
                            "inside the project's Assets folder. ", "OK");
                        return;
                    }
                    else if (filePath.Contains(Application.dataPath))
                    {
                        // Fix path to be usable for AssetDatabase.FindAssets
                        filePath = filePath.Remove(0, filePath.IndexOf("Assets"));
                    }
                }

                pathProp.stringValue = filePath;
            }
        }

        public static void SmartBrowseButton(SerializedProperty folderProp, bool limitToAssetFolder = true, string panelTitle = "Specify a New Folder")
        {
            GUIContent buttonContent = new GUIContent(" Browse ", "Designate a New Folder");
            if (GUILayout.Button(buttonContent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            {
                string filePath = folderProp.stringValue;
                filePath = EditorUtility.OpenFolderPanel(panelTitle, filePath, string.Empty);

                // If the user presses "cancel"
                if (filePath.Equals(string.Empty))
                {
                    return;
                }
                if (limitToAssetFolder)
                {
                    // or specifies something outside of this folder, reset filePath and don't proceed
                    if (!filePath.Contains("Assets"))
                    {
                        EditorUtility.DisplayDialog("Folder Browsing Error!", "AudioManager is a Unity editor tool and can only " +
                            "function inside the project's Assets folder. Please choose a different folder.", "OK");
                        return;
                    }
                    else if (filePath.Contains(Application.dataPath))
                    {
                        // Fix path to be usable for AssetDatabase.FindAssets
                        filePath = filePath.Remove(0, filePath.IndexOf("Assets"));
                    }
                }

                folderProp.stringValue = filePath;
            }
        }

        public static List<T> ImportAssetsAtPath<T>(string filePath) where T : Object
        {
            List<T> imports = new List<T>();
            List<string> importTargets = new List<string>(System.IO.Directory.GetFiles(filePath));
            for (int i = 0; i < importTargets.Count; i++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(importTargets[i]);
                if (!AssetDatabase.IsValidFolder(importTargets[i]))
                {
                    if (asset != null)
                    {
                        imports.Add(asset);
                    }
                }
            }
            return imports;
        }

        public static List<T> ImportAssetsOrFoldersAtPath<T>(string filePath) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(filePath);
            if (!AssetDatabase.IsValidFolder(filePath))
            {
                if (asset != null)
                {
                    return new List<T> { asset };
                }
            }
            else
            {
                List<T> imports = new List<T>();
                List<string> importTarget = new List<string>(System.IO.Directory.GetDirectories(filePath));
                importTarget.AddRange(System.IO.Directory.GetFiles(filePath));
                for (int i = 0; i < importTarget.Count; i++)
                {
                    imports.AddRange(ImportAssetsOrFoldersAtPath<T>(importTarget[i]));
                }
                return imports;
            }

            return new List<T>();
        }

        public static List<T> ImportSubAssetsAtPath<T>(string filePath) where T : Object
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(filePath);

            var loadedAssets = new List<T>();
            for (int i = 0; i < assets.Length; i++)
            {
                var a = assets[i] as T;
                if (a) // Was cast successful?
                {
                    loadedAssets.Add(a);
                }
            }

            return loadedAssets;
        }

        public static bool IsDragging(Rect dragRect) => dragRect.Contains(Event.current.mousePosition) && DragAndDrop.objectReferences.Length > 0;

        const int DAD_FONTSIZE = 40;
        const int DAD_BUFFER = 60;
        public static bool DragAndDropRegion(Rect dragRect, string normalLabel, string dragLabel, GUIStyle style = null)
        {
            switch (Event.current.type)
            {
                case EventType.Repaint:
                case EventType.Layout:
                    string label;

                    if (IsDragging(dragRect))
                    {
                        if (style == null) style = GUI.skin.box.SetFontSize(DAD_FONTSIZE);
                        label = dragLabel;
                    }
                    else
                    {
                        if (style == null) style = EditorStyles.label.SetFontSize(DAD_FONTSIZE);
                        label = normalLabel;
                    }

                    style = style
                        .ApplyTextAnchor(TextAnchor.MiddleCenter)
                        .SetFontSize((int)Mathf.Lerp(1f, (float)style.fontSize, dragRect.height / (float)(DAD_BUFFER)))
                        .ApplyBoldText();

                    GUI.Box(dragRect, label, style);

                    return false;
            }

            if (dragRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.DragPerform)
                {
                    Event.current.Use();
                    return true;
                }
            }
            return false;
        }

        public static bool CondensedButton(string label)
        {
            return GUILayout.Button(" " + label + " ", new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
        }

        public static void RenderSequentialIntPopup(SerializedProperty property, int firstValue, int lastValue)
        {
            int size = (lastValue + 1) - firstValue;
            var displayOptions = new GUIContent[size];
            var optionValues = new int[size];
            for (int i = 0; i < size; i++)
            {
                displayOptions[i] = new GUIContent(i.ToString());
                optionValues[i] = i;
            }

            EditorGUILayout.IntPopup(property, displayOptions, optionValues);
        }

        /// <summary>
        /// Equivalent to the FileUtil version, but works with backslashes
        /// </summary>
        /// <returns></returns>
        public static string GetProjectRelativePath(string path)
        {
            return FileUtil.GetProjectRelativePath(path.Replace('\\', '/'));
        }

        static Color guiColor;
        public static void BeginColourChange(Color color)
        {
            guiColor = GUI.color;
            GUI.color = color;
        }

        public static void EndColourChange() => GUI.color = guiColor;

        static Color guiBackgroundColor;
        public static void BeginBackgroundColourChange(Color color)
        {
            guiBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
        }

        public static void EndBackgroundColourChange() => GUI.backgroundColor = guiBackgroundColor;
    }
}