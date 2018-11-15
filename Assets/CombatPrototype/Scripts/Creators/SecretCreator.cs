#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(SecretComponent))]
public class SecretCreator : Editor {
    SecretComponent currentComponent;

    public override void OnInspectorGUI() {
        if (currentComponent == null) {
            currentComponent = (SecretComponent) target;
        }

        GUILayout.Label("Secret Creator ", EditorStyles.boldLabel);
        currentComponent.id = EditorGUILayout.IntField("ID: ", currentComponent.id);
        currentComponent.thisName = EditorGUILayout.TextField("Name: ", currentComponent.thisName);
        currentComponent.description = EditorGUILayout.TextField("Description: ", currentComponent.description);
        currentComponent.intelIDToBeUnlocked = EditorGUILayout.IntField("Intel ID To Be Unlocked: ", currentComponent.intelIDToBeUnlocked);

        if (GUILayout.Button("Create Secret")) {
            SaveSecret();
        }
    }

    #region Saving
    private void SaveSecret() {
        string strID = currentComponent.id.ToString();
        if (string.IsNullOrEmpty(strID)) {
            EditorUtility.DisplayDialog("Error", "Please specify an ID", "OK");
            return;
        }
        string path = Utilities.dataPath + "Secrets/";
        string fileName = "[" + strID + "]" + currentComponent.thisName + ".json";
        string detailedPath = string.Empty;
        if (AlreadyHasSecretID(path, strID, ref detailedPath)) {
            if (EditorUtility.DisplayDialog("Overwrite Secret", "An secret with ID " + strID + " already exists. Replace with this secret?", "Yes", "No")) {
                File.Delete(detailedPath);
                SaveSecretJson(currentComponent, path + fileName);
            }
        } else {
            SaveSecretJson(currentComponent, path + fileName);
        }
    }
    private void SaveSecretJson(SecretComponent currentComponent, string path) {
        //Secret secret = new Secret();
        //secret.SetData(currentComponent);
        //string jsonString = JsonUtility.ToJson(secret);
        //System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        //writer.WriteLine(jsonString);
        //writer.Close();
        //UnityEditor.AssetDatabase.ImportAsset(path);
        //Debug.Log("Successfully saved secret " + currentComponent.id + " at " + path);
    }
    private bool AlreadyHasSecretID(string path, string id, ref string detailedPath) {
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string secretID = fileName.Substring(1, (fileName.IndexOf(']') - 1));
            if(secretID == id) {
                detailedPath = file;
                return true;
            }
        }
        return false;
    }
    #endregion
}
#endif
