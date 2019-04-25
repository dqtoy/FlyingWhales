using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(STCManager))]
public class STCManagerEditor : Editor {

    public override void OnInspectorGUI() {
        STCManager stc = (STCManager)target;

        DrawDefaultInspector();
        if (GUILayout.Button("Clear Tiles")) {
            stc.ClearTiles();
        }
        if (GUILayout.Button("Save Template")) {
            stc.SaveTemplate();
        }
        if (GUILayout.Button("Load Template")) {
            stc.LoadTemplate();
        }
        if (GUILayout.Button("Load Tile Assets")) {
            stc.LoadAllTilesAssets();
        }
        if (GUILayout.Button("Place Tiles At Origin")) {
            stc.PlaceAtOrigin();
        }
        if (GUILayout.Button("Create Connector")) {
            stc.CreateNewConnector();
        }
        if (GUILayout.Button("Generate Test Town")) {
            stc.GenerateTestTown();
        }
    }
}
