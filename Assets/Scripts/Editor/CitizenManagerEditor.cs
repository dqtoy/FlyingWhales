using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CitizenManager))]
public class CitizenManagerEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CitizenManager cm = (CitizenManager)target;
        if (GUILayout.Button("Apply Trait Weights")) {
            cm.ApplyTraitSetup();
        }
        if (GUILayout.Button("Reset Trait Weights")) {
            cm.ResetTraitSetup();
        }
    }
}
