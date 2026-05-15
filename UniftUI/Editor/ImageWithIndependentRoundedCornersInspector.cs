using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Nobi.UiRoundedCorners;

namespace Nobi.UiRoundedCorners.Editor {
    [CustomEditor(typeof(ImageWithIndependentRoundedCorners)), CanEditMultipleObjects]
    public class ImageWithIndependentRoundedCornersInspector : UnityEditor.Editor {
        private ImageWithIndependentRoundedCorners script;

        private void OnEnable() {
            script = (ImageWithIndependentRoundedCorners)target;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            SerializedProperty vector4Prop = serializedObject.FindProperty("_r");
            EditorGUILayout.PropertyField(vector4Prop.FindPropertyRelative("x"), new GUIContent("Top Left Corner"));
            EditorGUILayout.PropertyField(vector4Prop.FindPropertyRelative("y"), new GUIContent("Top Right Corner"));
            EditorGUILayout.PropertyField(vector4Prop.FindPropertyRelative("w"), new GUIContent("Bottom Left Corner"));
            EditorGUILayout.PropertyField(vector4Prop.FindPropertyRelative("z"), new GUIContent("Bottom Right Corner"));

            serializedObject.ApplyModifiedProperties();

            if (script != null && !script.TryGetComponent<MaskableGraphic>(out var _)) {
                EditorGUILayout.HelpBox("This script requires a MaskableGraphic (Image or RawImage) on the same GameObject.", MessageType.Warning);
            }
        }
    }
}
