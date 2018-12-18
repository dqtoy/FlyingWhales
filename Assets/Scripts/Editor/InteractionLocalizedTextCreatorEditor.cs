#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class InteractionLocalizedTextCreatorEditor : EditorWindow {
    public TrelloCard cardData;

    string trelloJSONStr = "Place trello json here...";
    Vector2 scrollPos = Vector2.zero;

    string textAreaStr;
    Vector2 textAreaScroll = Vector2.zero;

    [MenuItem("Window/Interaction Localized Text Creator")]
    static void Init() {
        EditorWindow.GetWindow(typeof(InteractionLocalizedTextCreatorEditor)).Show();
    }

    private void OnGUI() {
        textAreaScroll = EditorGUILayout.BeginScrollView(textAreaScroll);
        EditorGUILayout.LabelField(textAreaStr, GUILayout.Height(position.height));
        EditorGUILayout.EndScrollView();

        //this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height));
        if (cardData != null) {
            if (GUILayout.Button("Save As Localized Text")) {
                SaveAsLocalizedText();
            }
        }
        if (GUILayout.Button("Load JSON data")) {
            string path = EditorUtility.OpenFilePanel("Pick json file", "", "json");
            if (path.Length != 0) {
                trelloJSONStr = File.ReadAllText(path);
            }
            LoadJSONData();
        }
        //EditorGUILayout.EndScrollView();
    }
    private void LoadJSONData() {
        if (!string.IsNullOrEmpty(trelloJSONStr)) {
            cardData = JsonUtility.FromJson<TrelloCard>(trelloJSONStr);
            Debug.Log(cardData.ToString());
            textAreaStr = cardData.ToString();
        } else {
            cardData = null;
        }
    }
    private void SaveAsLocalizedText() {
        cardData = JsonUtility.FromJson<TrelloCard>(trelloJSONStr);
        LocalizationData localizationData = new LocalizationData();
        localizationData.items = new List<LocalizationItem>();
        for (int i = 0; i < cardData.checklists.Count; i++) {
            Checklist currChecklist = cardData.checklists[i];
            string stateName = string.Empty;
            if (currChecklist.name.Equals("State 1")){
                stateName = "start";
            } else if (currChecklist.name.Contains("State") || currChecklist.name.Contains("Result")) {
                stateName = currChecklist.name.Substring(currChecklist.name.IndexOf(": ") + 1);
                stateName = stateName.Trim().ToLower();
            } else {
                continue; //skip
            }

            int logCount = 1;
            for (int j = 0; j < currChecklist.checkItems.Count; j++) {
                ChecklistItem currChecklistItem = currChecklist.checkItems[j];
                if (currChecklistItem.name.Contains("Text Description")) {
                    string description = currChecklistItem.name.Substring(currChecklistItem.name.IndexOf(": ") + 1);
                    description = ConvertToLogFillers(description.TrimStart());
                    localizationData.items.Add(new LocalizationItem() {
                        key = stateName + "_description",
                        value = description,
                    });
                } else if (currChecklistItem.name.Contains("Log") || currChecklistItem.name.Contains("Logs")) {
                    string log = currChecklistItem.name.Substring(currChecklistItem.name.IndexOf(": ") + 1);
                    log = ConvertToLogFillers(log.TrimStart());
                    localizationData.items.Add(new LocalizationItem() {
                        key = stateName + "_log" + logCount,
                        value = log,
                    });
                    logCount++;
                }
            }
        }

        var path = EditorUtility.SaveFilePanel(
                "Save data as Localized Text JSON",
                Application.streamingAssetsPath + "/ENGLISH",
                cardData.name + ".json",
                "json");

        if (path.Length != 0) {
            var jsonData = JsonUtility.ToJson(localizationData);
            if (jsonData != null)
                File.WriteAllText(path, jsonData);
        }
    }

    private string ConvertToLogFillers(string source) {
        string newString = source;
        newString = newString.Replace("[Demon Name]", "%113%");
        newString = newString.Replace("[Demon]", "%113%");
        newString = newString.Replace("[Minion Name]", "%113%");
        newString = newString.Replace("[Character Name]", "%00@");
        newString = newString.Replace("[Character Name 1]", "%00@");
        newString = newString.Replace("[Character Name 2]", "%10@");
        newString = newString.Replace("[Location Name]", "%04@");
        newString = newString.Replace("[Faction Name]", "%01@");
        newString = newString.Replace("[Location Name 2]", "%14@");
        return newString;
    }
}

[System.Serializable]
public class TrelloCard {
    public string name;
    public List<Checklist> checklists;

    public override string ToString() {
        string summary = name + "\n";
        for (int i = 0; i < checklists.Count; i++) {
            Checklist currList = checklists[i];
            summary += currList.name + "\n";
            for (int j = 0; j < currList.checkItems.Count; j++) {
                ChecklistItem currListItem = currList.checkItems[j];
                summary += "    " + currListItem.name + "\n";
            }
        }
        return summary;
    }
}
[System.Serializable]
public class Checklist {
    public string name;
    public List<ChecklistItem> checkItems;
}
[System.Serializable]
public class ChecklistItem {
    public string name;
}
