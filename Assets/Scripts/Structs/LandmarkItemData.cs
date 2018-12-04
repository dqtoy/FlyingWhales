using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(LandmarkItemData))]
public class LandmarkItemDataDrawer : PropertyDrawer {
    
    string[] choices;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (choices == null) {
            List<string> allChoices = GetAllItemNames();
            allChoices.AddRange(ItemManager.lootChestNames);//Add loot crate to choices
            choices = allChoices.ToArray();

        }
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var itemNameRect = new Rect(position.x, position.y, 150, position.height);
        var weightRect = new Rect(position.x + 155, position.y, 50, position.height);
        var isUnlimitedRect = new Rect(position.x + 205, position.y, 50, position.height);

        property.FindPropertyRelative("itemName");
        property.FindPropertyRelative("itemIndex").intValue = EditorGUI.Popup(itemNameRect, property.FindPropertyRelative("itemIndex").intValue, choices);
        property.FindPropertyRelative("itemName").stringValue = choices[property.FindPropertyRelative("itemIndex").intValue];
        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        //EditorGUI.PropertyField(itemNameRect, itemNameProp, GUIContent.none);
        EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("exploreWeight"), GUIContent.none);
        EditorGUI.PropertyField(isUnlimitedRect, property.FindPropertyRelative("isUnlimited"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    /*
     This is used for setting up landmark
     items. Since allItems have not been computed before runtime
         */
    public List<string> GetAllItemNames() {
        List<string> allItemNames = new List<string>();
        string path = Utilities.dataPath + "Items/";
        string[] directories = Directory.GetDirectories(path);
        for (int i = 0; i < directories.Length; i++) {
            string currDirectory = directories[i];
            string itemType = new DirectoryInfo(currDirectory).Name;
            string[] files = Directory.GetFiles(currDirectory, "*.json");
            for (int k = 0; k < files.Length; k++) {
                string currFilePath = files[k];
                string dataAsJson = File.ReadAllText(currFilePath);
                Item newItem = JsonUtility.FromJson<Item>(dataAsJson);
                allItemNames.Add(newItem.itemName);
            }
        }
        return allItemNames;
    }
}
#endif

[System.Serializable]
public class LandmarkItemData {
    public string itemName;
    public int exploreWeight;
    public bool isUnlimited; //Can this item be obtained at a landmark unlimited times?
    [SerializeField] private int itemIndex; //For custom editor
}