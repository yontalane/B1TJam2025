//using B1TJam2025;
//using UnityEditor;
//using UnityEngine;

//[CustomPropertyDrawer(typeof(GameSequenceSegment))]
//public class GameSequenceSegmentDrawer : PropertyDrawer
//{
//    private const float PADDING = 5f;


//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        SerializedProperty type = property.FindPropertyRelative("type");

//        if (type.enumValueIndex == 0)
//        {
//            return EditorGUIUtility.singleLineHeight * 6 + PADDING;
//        }
//        else
//        {
//            return EditorGUIUtility.singleLineHeight * 2 + PADDING;
//        }
//    }

//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        EditorGUI.BeginProperty(position, label, property);

//        float height = EditorGUIUtility.singleLineHeight;
//        float indent = 5f;

//        Rect typeRect = new(position.x, position.y, position.width, height);
//        Rect rect2 = new(position.x + indent, position.y + PADDING + height * 1, position.width - indent, height);
//        Rect rect3 = new(position.x + indent, position.y + PADDING + height * 2, position.width - indent, height);
//        Rect rect4 = new(position.x + indent, position.y + PADDING + height * 3, position.width - indent, height);
//        Rect rect5 = new(position.x + indent, position.y + PADDING + height * 4, position.width - indent, height);
//        Rect rect6 = new(position.x + indent, position.y + PADDING + height * 5, position.width - indent, height);

//        SerializedProperty type = property.FindPropertyRelative("type");
//        SerializedProperty spawnTarget = property.FindPropertyRelative("spawnTarget");
//        SerializedProperty perp = property.FindPropertyRelative("perp");
//        SerializedProperty beatBeforeContinuing = property.FindPropertyRelative("beatBeforeContinuing");
//        SerializedProperty beatBuddyAPB = property.FindPropertyRelative("beatBuddyAPB");
//        SerializedProperty victorySoliloquy = property.FindPropertyRelative("victorySoliloquy");
//        SerializedProperty randomCount = property.FindPropertyRelative("randomCount");

//        EditorGUI.PropertyField(typeRect, type, GUIContent.none);

//        if (type.enumValueIndex == 0)
//        {
//            EditorGUI.PropertyField(rect2, spawnTarget, new GUIContent(spawnTarget.displayName, spawnTarget.tooltip));
//            EditorGUI.PropertyField(rect3, perp, new GUIContent(perp.displayName, perp.tooltip));
//            EditorGUI.PropertyField(rect4, beatBeforeContinuing, new GUIContent(beatBeforeContinuing.displayName, beatBeforeContinuing.tooltip));
//            EditorGUI.PropertyField(rect5, beatBuddyAPB, new GUIContent(beatBuddyAPB.displayName, beatBuddyAPB.tooltip));
//            EditorGUI.PropertyField(rect6, victorySoliloquy, new GUIContent(victorySoliloquy.displayName, victorySoliloquy.tooltip));
//        }
//        else
//        {
//            EditorGUI.PropertyField(rect2, randomCount, new GUIContent(randomCount.displayName, randomCount.tooltip));
//        }

//        EditorGUI.EndProperty();
//    }
//}