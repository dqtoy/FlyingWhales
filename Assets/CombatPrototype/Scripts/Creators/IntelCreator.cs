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
        string path = Utilities.dataPath + "Intels/";
        string fileName = "[" + strID + "]" + currentComponent.thisName + ".json";
        string detailedPath = string.Empty;
        if (AlreadyHasIntelID(path, strID, ref detailedPath)) {
            if (EditorUtility.DisplayDialog("Overwrite Intel", "An intel with ID " + strID + " already exists. Replace with this intel?", "Yes", "No")) {
                File.Delete(detailedPath);
                SaveIntelJson(currentComponent, path + fileName);
            }
        } else {
            SaveIntelJson(currentComponent, path + fileName);
        }
    }
    private void SaveIntelJson(IntelComponent currentComponent, string path) {
        Token intel = new Token();
        //intel.SetData(currentComponent);
        //string jsonString = JsonUtility.ToJson(intel);
        //System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        //writer.WriteLine(jsonString);
        //writer.Close();
        //UnityEditor.AssetDatabase.ImportAsset(path);
        //Debug.Log("Successfully saved intel " + currentComponent.id + " at " + path);
    }
    private bool AlreadyHasIntelID(string path, string id, ref string detailedPath) {
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string intelID = fileName.Substring(1, (fileName.IndexOf(']') - 1));
            if(intelID == id) {
                detailedPath = file;
                return true;
            }
        }
        return false;
    }
    #endregion
}
#endif
