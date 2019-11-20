using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(CharacterManager))]
public class CharacterManagerEditor : Editor {


    public override void OnInspectorGUI() {
        CharacterManager myTarget = (CharacterManager)target;

        //myTarget.experience = EditorGUILayout.IntField("Experience", myTarget.experience);
        //EditorGUILayout.LabelField("Level", myTarget.Level.ToString());

        // Show default inspector property editor
        DrawDefaultInspector();

        if (GUILayout.Button("Load Character Marker Assets")) {
            myTarget.LoadCharacterMarkerAssets();
        }
    }
}

#endif