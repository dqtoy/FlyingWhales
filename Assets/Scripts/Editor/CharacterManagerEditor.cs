using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterManager))]
public class CharacterManagerEditor : Editor {

    string x64AssetPath = "Assets/Resources/Portraits/64x64/";
    string x256AssetPath = "Assets/Resources/Portraits/256x256/";

    public override void OnInspectorGUI() {
        CharacterManager myTarget = (CharacterManager)target;

        //myTarget.experience = EditorGUILayout.IntField("Experience", myTarget.experience);
        //EditorGUILayout.LabelField("Level", myTarget.Level.ToString());

        // Show default inspector property editor
        DrawDefaultInspector();

        GUILayout.Label("64x64 Asset Path");
        x64AssetPath = EditorGUILayout.TextField(x64AssetPath);
        GUILayout.Label("256x256 Asset Path");
        x256AssetPath = EditorGUILayout.TextField(x256AssetPath);
        if (GUILayout.Button("Load Portrait Assets")) {
            myTarget.LoadPortraitAssets(IMAGE_SIZE.X64, x64AssetPath);
            myTarget.LoadPortraitAssets(IMAGE_SIZE.X256, x256AssetPath);
        }
    }
}
