using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ActionFilterData {
    public ACTION_FILTER_TYPE filterType;
    public ACTION_FILTER_CONDITION condition;
    public List<ACTION_FILTER> objects;
}

//[CustomPropertyDrawer(typeof(ActionFilterData))]
//public class ActionFilterDrawer : PropertyDrawer {
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
//        EditorGUI.BeginProperty(position, label, property);

//        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

//        var indent = EditorGUI.indentLevel;
//        EditorGUI.indentLevel = 0;

//        SerializedProperty filterTypeProp = property.FindPropertyRelative("filterType");
//        SerializedProperty conditionProp = property.FindPropertyRelative("condition");

//        // Set indent back to what it was
//        EditorGUI.indentLevel = indent;
//        EditorGUI.EndProperty();
//    }
//}
