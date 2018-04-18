#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(CharacterManager))]
public class CitizenManagerEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CharacterManager cm = (CharacterManager)target;
        if (GUILayout.Button("Apply Trait Weights")) {
            cm.ApplyTraitSetup();
        }
        if (GUILayout.Button("Reset Trait Weights")) {
            cm.ResetTraitSetup();
        }
    }
}
#endif
