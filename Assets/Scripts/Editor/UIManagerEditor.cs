using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(UIManager))]
public class UIManagerEditor : Editor {

    public override void OnInspectorGUI() {
        UIManager myTarget = (UIManager)target;

        //myTarget.experience = EditorGUILayout.IntField("Experience", myTarget.experience);
        //EditorGUILayout.LabelField("Level", myTarget.Level.ToString());

        // Show default inspector property editor
        DrawDefaultInspector();
        //if (GUILayout.Button("Apply Unified Selectable")) {
        //    myTarget.UnifySelectables();
        //}
    }
}

#endif
