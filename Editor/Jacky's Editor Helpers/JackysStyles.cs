using UnityEngine;
using UnityEditor;

namespace JackysEditorHelpers
{
    public class JackysStyles : Editor
    {
        public static GUIStyle CenteredTitle =>
            EditorStyles.boldLabel
            .ApplyBoldText()
            .ApplyTextAnchor(TextAnchor.MiddleCenter)
            .SetFontSize(20);

        public static GUIStyle CenteredLabel => 
            EditorStyles.label
            .ApplyTextAnchor(TextAnchor.MiddleCenter);

        public static GUIStyle CenteredBoldHeader =>
            new GUIStyle(EditorStyles.boldLabel)
            .ApplyTextAnchor(TextAnchor.UpperCenter)
            .SetFontSize(14);

        public static GUIStyle CenteredHeader =>
            EditorStyles.largeLabel.ApplyTextAnchor(TextAnchor.MiddleCenter);

        public static GUIStyle UpCenteredHeader =>
            EditorStyles.largeLabel.ApplyTextAnchor(TextAnchor.UpperCenter);
    }
}