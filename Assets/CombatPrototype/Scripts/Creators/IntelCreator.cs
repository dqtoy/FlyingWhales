#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(IntelComponent))]
public class IntelCreator : Editor {
    IntelComponent currentComponent;

    public override void OnInspectorGUI() {
        if (currentComponent == null) {
            currentComponent = (IntelComponent) target;
        }

        GUILayout.Label("Intel Creator ", EditorStyles.boldLabel);
        currentComponent.id = EditorGUILayout.IntField("ID: ", currentComponent.id);
        currentComponent.thisName = EditorGUILayout.TextField("Name: ", currentComponent.thisName);
        currentComponent.description = EditorGUILayout.TextField("Description: ", currentComponent.description);

        if (GUILayout.Button("Create Intel")) {
            SaveIntel();
        }
    }

    #region Saving
    private void SaveIntel() {
        string strID = currentComponent.id.ToString();
        if (string.IsNullOrEmpty(strID)) {
            EditorUtility.DisplayDialog("Error", "Please specify an ID", "OK");
            return;
        }
        string path = Utilities.dataPath + "Intels/" + strID + ".json";
        if (Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Intel", "An intel with ID " + strID + " already exists. Replace with this intel?", "Yes", "No")) {
                File.Delete(path);
                SaveIntelJson(currentComponent, path);
            }
        } else {
            SaveIntelJson(currentComponent, path);
        }
    }
    private void SaveIntelJson(IntelComponent currentComponent, string path) {
        Intel intel = new Intel();
        intel.SetData(currentComponent);
        string jsonString = JsonUtility.ToJson(intel);
        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();
        UnityEditor.AssetDatabase.ImportAsset(path);
        Debug.Log("Successfully saved intel " + currentComponent.id + " at " + path);
    }
    #endregion
}
#endif
