using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JackysEditorHelpers
{
    public static class EditorExtensions
    {
        /// <summary>
        /// Adds a new element to the end of the array and returns the new element
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static SerializedProperty AddAndReturnNewArrayElement(this SerializedProperty prop)
        {
            int index = prop.arraySize;
            prop.InsertArrayElementAtIndex(index);
            return prop.GetArrayElementAtIndex(index);
        }

        /// <summary>
        /// Removes all null or missing elements from an array. 
        /// For missing elements, invoke this method in Update or OnGUI
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static SerializedProperty RemoveNullElementsFromArray(this SerializedProperty prop)
        {
            for (int i = prop.arraySize - 1; i > -1; i--)
            {
                if (prop.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    // A dirty hack, but Unity serialization is real messy
                    // https://answers.unity.com/questions/555724/serializedpropertydeletearrayelementatindex-leaves.html
                    prop.DeleteArrayElementAtIndex(i);
                    prop.DeleteArrayElementAtIndex(i);
                }
            }
            return prop;
        }

        public static GUIContent GUIContent(this SerializedProperty property)
        {
            return new GUIContent(property.displayName, property.tooltip);
        }

        public static string TimeToString(this float time)
        {
            time *= 1000;
            int minutes = (int)time / 60000;
            int seconds = (int)time / 1000 - 60 * minutes;
            int milliseconds = (int)time - minutes * 60000 - 1000 * seconds;
            return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }

        public static GUIStyle ApplyRichText(this GUIStyle referenceStyle)
        {
            var style = new GUIStyle(referenceStyle);
            style.richText = true;
            return style;
        }

        public static GUIStyle SetTextColor(this GUIStyle referenceStyle, Color color)
        {
            var style = new GUIStyle(referenceStyle);
            style.normal.textColor = color;
            return style;
        }

        public static GUIStyle ApplyTextAnchor(this GUIStyle referenceStyle, TextAnchor anchor)
        {
            var style = new GUIStyle(referenceStyle);
            style.alignment = anchor;
            return style;
        }

        public static GUIStyle SetFontSize(this GUIStyle referenceStyle, int fontSize)
        {
            var style = new GUIStyle(referenceStyle);
            style.fontSize = fontSize;
            return style;
        }

        public static GUIStyle ApplyBoldText(this GUIStyle referenceStyle)
        {
            var style = new GUIStyle(referenceStyle);
            style.fontStyle = FontStyle.Bold;
            return style;
        }

        public static GUIStyle ApplyWordWrap(this GUIStyle referenceStyle)
        {
            var style = new GUIStyle(referenceStyle);
            style.wordWrap = true;
            return style;
        }
    }
    
    public static class Extensions
    {
        public static T[] GetKeysCached<T, U>(this Dictionary<T, U> d)
        {
            T[] keys = new T[d.Keys.Count];
            d.Keys.CopyTo(keys, 0);
            return keys;
        }

        public static Color Add(this Color thisColor, Color otherColor)
        {
            return new Color
            {
                r = Mathf.Clamp01(thisColor.r + otherColor.r),
                g = Mathf.Clamp01(thisColor.g + otherColor.g),
                b = Mathf.Clamp01(thisColor.b + otherColor.g),
                a = Mathf.Clamp01(thisColor.a + otherColor.a)
            };
        }

        public static Color Subtract(this Color thisColor, Color otherColor)
        {
            return new Color
            {
                r = Mathf.Clamp01(thisColor.r - otherColor.r),
                g = Mathf.Clamp01(thisColor.g - otherColor.g),
                b = Mathf.Clamp01(thisColor.b - otherColor.g),
                a = Mathf.Clamp01(thisColor.a - otherColor.a)
            };
        }

        /// <summary>
        /// Helpful method by Stack Overflow user ata
        /// https://stackoverflow.com/questions/3210393/how-do-i-remove-all-non-alphanumeric-characters-from-a-string-except-dash
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ConvertToAlphanumeric(this string input)
        {
            char[] arr = input.ToCharArray();

            arr = System.Array.FindAll(arr, c => char.IsLetterOrDigit(c));

            if (arr.Length > 0)
            {
                // If the first index is a number
                while (char.IsDigit(arr[0]) || arr[0] == '.')
                {
                    List<char> newArray = new List<char>();
                    newArray = new List<char>(arr);
                    newArray.RemoveAt(0);
                    arr = newArray.ToArray();
                    if (arr.Length == 0) break; // No valid characters to use, returning empty
                }

                if (arr.Length != 0)
                {
                    // If the last index is a period
                    while (arr[arr.Length - 1] == '.')
                    {
                        List<char> newArray = new List<char>();
                        newArray = new List<char>(arr);
                        newArray.RemoveAt(newArray.Count - 1);
                        arr = newArray.ToArray();
                        if (arr.Length == 0) break; // No valid characters to use, returning empty
                    }
                }
            }

            return new string(arr);
        }
    }
}