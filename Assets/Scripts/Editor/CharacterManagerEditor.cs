using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(CharacterManager))]
public class CharacterManagerEditor : Editor {

    string assetPath = "Assets/Textures/Portraits/";

    public override void OnInspectorGUI() {
        CharacterManager myTarget = (CharacterManager)target;

        //myTarget.experience = EditorGUILayout.IntField("Experience", myTarget.experience);
        //EditorGUILayout.LabelField("Level", myTarget.Level.ToString());

        // Show default inspector property editor
        DrawDefaultInspector();

        GUILayout.Label("Asset Path");
        assetPath = EditorGUILayout.TextField(assetPath);
        if (GUILayout.Button("Load Portrait Assets")) {
            //myTarget.LoadPortraitAssets(assetPath);
        }
    }
}

#endif