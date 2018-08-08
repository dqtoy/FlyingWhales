using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using ECS;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AttributePanelUI : MonoBehaviour {
    public static AttributePanelUI Instance;

    public Dropdown categoryOptions;
    public InputField nameInput;
    public Toggle hiddenToggle;

    void Awake() {
        Instance = this;
    }
    void Start() {
        LoadAllData();
    }

    #region Utilities
    private void LoadAllData() {
        categoryOptions.ClearOptions();

        string[] categories = System.Enum.GetNames(typeof(ATTRIBUTE_CATEGORY));

        categoryOptions.AddOptions(categories.ToList());
    }
    private void ClearData() {
        categoryOptions.value = 0;
        nameInput.text = string.Empty;
        hiddenToggle.isOn = false;
    }
    private void SaveAttribute() {
#if UNITY_EDITOR
        if (nameInput.text == string.Empty) {
            EditorUtility.DisplayDialog("Error", "Please specify an Attribute Name", "OK");
            return;
        }
        string path = Utilities.dataPath + "Attributes/" + categoryOptions.options[categoryOptions.value].text + "/" + nameInput.text + ".json";
        if (Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Attribute", "An attribute with name " + nameInput.text + " already exists. Replace with this attribute?", "Yes", "No")) {
                File.Delete(path);
                SaveAttributeJson(path);
            }
        } else {
            SaveAttributeJson(path);
        }
#endif
    }
    private void SaveAttributeJson(string path) {
        Attribute newAttribute = new Attribute();

        newAttribute.SetDataFromAttributePanelUI();

        string jsonString = JsonUtility.ToJson(newAttribute);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

#if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
        Debug.Log("Successfully saved attribute at " + path);
    }
    private void LoadAttribute() {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Select Attribute", Utilities.dataPath + "Attributes/", "json");
        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);
            Attribute attribute = JsonUtility.FromJson<Attribute>(dataAsJson);
            ClearData();
            LoadAttributeDataToUI(attribute);
        }
#endif
    }
    private void LoadAttributeDataToUI(Attribute attribute) {
        nameInput.text = attribute.name;
        hiddenToggle.isOn = attribute.isHidden;
        categoryOptions.value = GetCategoryIndex(attribute.category.ToString());
    }
    private int GetCategoryIndex(string categoryName) {
        for (int i = 0; i < categoryOptions.options.Count; i++) {
            if (categoryOptions.options[i].text == categoryName) {
                return i;
            }
        }
        return 0;
    }
    #endregion

    #region Button Clicks
    public void OnClickAddNewAttribute() {
        ClearData();
    }
    public void OnClickEditAttribute() {
        LoadAttribute();
    }
    public void OnClickSaveAttribute() {
        SaveAttribute();
    }
    #endregion
}
