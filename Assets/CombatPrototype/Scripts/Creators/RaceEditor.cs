using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace ECS {
	[CustomEditor(typeof(RaceComponent))]
    public class RaceEditor : Editor {
		RaceComponent raceComponent;

		public override void OnInspectorGUI() {
			if(raceComponent == null){
				raceComponent = (RaceComponent)target;
			}

            GUILayout.Label("Race Editor ", EditorStyles.boldLabel);
			raceComponent.race = (RACE)EditorGUILayout.EnumPopup("Race: ", raceComponent.race);

			raceComponent.baseStr = EditorGUILayout.IntField("Base Strength: ", raceComponent.baseStr);
			raceComponent.baseInt = EditorGUILayout.IntField("Base Intelligence: ", raceComponent.baseInt);
			raceComponent.baseAgi = EditorGUILayout.IntField("Base Agility: ", raceComponent.baseAgi);
			raceComponent.baseHP = EditorGUILayout.IntField("Base HP: ", raceComponent.baseHP);

			SerializedProperty serializedProperty = serializedObject.FindProperty("bodyParts");
			EditorGUILayout.PropertyField(serializedProperty, true);
			serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Save Race Settings")) {
                SaveRaceSettings();
            }
        }

        #region Body Parts

        #endregion

        #region Saving
        private void SaveRaceSettings() {
			string path = "Assets/CombatPrototype/Data/RaceSettings/" + raceComponent.race.ToString() + ".json";
            if (Utilities.DoesFileExist(path)) {
				if (EditorUtility.DisplayDialog("Overwrite Race Setting", "A race setting with name " + raceComponent.race.ToString() + " already exists. Replace with these settings?", "Yes", "No")) {
                    File.Delete(path);
                    SaveRaceJson(path);
                }
            } else {
                SaveRaceJson(path);
            }
        }
        private void SaveRaceJson(string path) {
			string jsonString = JsonUtility.ToJson(raceComponent);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(jsonString);
            writer.Close();
            Debug.Log("Successfully saved file at " + path);
        }
        #endregion

        #region Loading
//        private void LoadRaceSettings(RACE race) {
//            string path = "Assets/CombatPrototype/Data/RaceSettings/" + race.ToString() + ".json";
//            string dataAsJson = File.ReadAllText(path);
//			raceComponent = JsonUtility.FromJson<RaceSetting>(dataAsJson);
//        }
        #endregion

    }
}