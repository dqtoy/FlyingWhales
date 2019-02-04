using UnityEngine;
using System.Collections;
using Steamworks;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class FPSDisplay : MonoBehaviour {
    float deltaTime = 0.0f;

    void Update() {
        if (GameManager.Instance.displayFPS) {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }
    }

    void OnGUI() {
        if (GameManager.Instance.displayFPS) {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            //style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
            style.normal.textColor = Color.white;

            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
      
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            //text += "\nAgents: " + PathfindingManager.Instance.allAgents.Count.ToString();
            //text += "\nCharacters: " + CharacterManager.Instance.allCharacters.Count.ToString();
            if (SteamManager.Initialized) {
                string name = SteamFriends.GetPersonaName();
                text += "\nSteam Name: " + name;
                text += "\nCharacters Snatched: " + AchievementManager.Instance.charactersSnatched;
            }

            if (GameManager.Instance.showFullDebug) {
                text += FullDebugInfo();
            }
            GUI.Label(rect, text, style);
        }
    }

    private string FullDebugInfo() {
        string text = string.Empty;
        if (UIManager.Instance != null && UIManager.Instance.characterInfoUI.isShowing) {
            text += "\n" + GetCharacterInfo();
        }

        return text;
    }

    private string GetCharacterInfo() {
        Character character = UIManager.Instance.characterInfoUI.activeCharacter;
        string text = character.name + "'s info:";
        text += "\nRelationships: ";
        foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in character.relationships) {
            text += "\n" + kvp.Key.name + ": ";
            for (int i = 0; i < kvp.Value.rels.Count; i++) {
                text += "|" + kvp.Value.rels[i].name + "|";
            }
            text += "\nLast Encounter: " + kvp.Value.lastEncounter.ToString();
            text += "\nEncounter Multiplier: " + kvp.Value.encounterMultiplier.ToString();
            text += "\nIs Missing?: " + kvp.Value.isCharacterMissing.ToString();
            text += "\nIs Located?: " + kvp.Value.isCharacterLocated.ToString();
            text += "\nKnown Structure: " + kvp.Value.knownStructure.ToString();
            text += "\nTrouble: " + kvp.Value.trouble?.ToString() ?? "None";
        }
        return text;
        
    }
}