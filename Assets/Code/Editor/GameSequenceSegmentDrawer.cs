using B1TJam2025;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameSequenceSegment))]
public class GameSequenceSegmentDrawer : PropertyDrawer
{
    private const float PADDING = 5f;


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty type = property.FindPropertyRelative("type");

        if (type.enumValueIndex == 0)
        {
            return EditorGUIUtility.singleLineHeight * 4 + PADDING;
        }
        else
        {
            return EditorGUIUtility.singleLineHeight * 2 + PADDING;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float height = EditorGUIUtility.singleLineHeight;
        float indent = 5f;

        Rect typeRect = new(position.x, position.y, position.width, height);
        Rect rect2 = new(position.x + indent, position.y + PADDING + height * 1, position.width - indent, height);
        Rect rect3 = new(position.x + indent, position.y + PADDING + height * 2, position.width - indent, height);
        Rect rect4 = new(position.x + indent, position.y + PADDING + height * 3, position.width - indent, height);

        SerializedProperty type = property.FindPropertyRelative("type");
        SerializedProperty spawnTarget = property.FindPropertyRelative("spawnTarget");
        SerializedProperty perp = property.FindPropertyRelative("perp");
        SerializedProperty beatBeforeContinuing = property.FindPropertyRelative("beatBeforeContinuing");
        SerializedProperty randomCount = property.FindPropertyRelative("randomCount");

        EditorGUI.PropertyField(typeRect, type, GUIContent.none);

        if (type.enumValueIndex == 0)
        {
            EditorGUI.PropertyField(rect2, spawnTarget, new GUIContent(spawnTarget.displayName, spawnTarget.tooltip));
            EditorGUI.PropertyField(rect3, perp, new GUIContent(perp.displayName, perp.tooltip));
            EditorGUI.PropertyField(rect4, beatBeforeContinuing, new GUIContent(beatBeforeContinuing.displayName, beatBeforeContinuing.tooltip));
        }
        else
        {
            EditorGUI.PropertyField(rect2, randomCount, new GUIContent(randomCount.displayName, randomCount.tooltip));
        }

        EditorGUI.EndProperty();
    }
}