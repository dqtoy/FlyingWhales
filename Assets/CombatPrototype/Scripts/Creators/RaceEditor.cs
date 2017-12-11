using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace ECS {
    public class RaceEditor : EditorWindow {

        private Vector2 scrollPos = Vector2.zero;
        [SerializeField] private RaceSetting currRaceSetting;

        private int bodyPartToAddIndex;
        // Add menu item to the Window menu
        [MenuItem("Window/Race Editor")]
        public static void ShowWindow() {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(RaceEditor));
        }

        private void OnGUI() {
            if(currRaceSetting == null) {
                currRaceSetting = new RaceSetting(RACE.NONE);
            }
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height));
            GUILayout.Label("Race Editor ", EditorStyles.boldLabel);
            currRaceSetting.race = (RACE)EditorGUILayout.EnumPopup("Race: ", currRaceSetting.race);
            string path = "Assets/CombatPrototype/Data/RaceSettings/" + currRaceSetting.race.ToString() + ".json";
            if (GUI.changed) {
                if (Utilities.DoesFileExist(path)) {
                    //show settings of current race
                    LoadRaceSettings(currRaceSetting.race);
                } else {
                    currRaceSetting = new RaceSetting(currRaceSetting.race);
                }
            }
            currRaceSetting.baseStr = EditorGUILayout.IntField("Base Strength: ", currRaceSetting.baseStr);
            currRaceSetting.baseInt = EditorGUILayout.IntField("Base Intelligence: ", currRaceSetting.baseInt);
            currRaceSetting.baseAgi = EditorGUILayout.IntField("Base Agility: ", currRaceSetting.baseAgi);
            currRaceSetting.baseHP = EditorGUILayout.IntField("Base HP: ", currRaceSetting.baseHP);

            currRaceSetting.strGain = EditorGUILayout.IntField("Strength Gain: ", currRaceSetting.strGain);
            currRaceSetting.intGain = EditorGUILayout.IntField("Intelligence Gain: ", currRaceSetting.intGain);
            currRaceSetting.agiGain = EditorGUILayout.IntField("Agility Gain: ", currRaceSetting.agiGain);
            currRaceSetting.hpGain = EditorGUILayout.IntField("HP Gain: ", currRaceSetting.hpGain);

            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty bodyPartsProperty = serializedObject.FindProperty("currRaceSetting");
            EditorGUILayout.PropertyField(bodyPartsProperty.FindPropertyRelative("bodyParts"), true);
            serializedObject.ApplyModifiedProperties();

            //GUILayout.Space(10);
            //GUILayout.BeginVertical(EditorStyles.helpBox);
            //GUILayout.Label("Add Skills ", EditorStyles.boldLabel);
            //List<string> choices = GetAllSkillsOfType(skillTypeToAdd);
            //bodyPartToAddIndex = EditorGUILayout.Popup("Body Part To Add: ", bodyPartToAddIndex, choices.ToArray());
            //GUI.enabled = choices.Count > 0;
            //if (GUILayout.Button("Add Skill")) {
            //    AddSkillToList(choices[skillToAddIndex]);
            //}
            //GUI.enabled = true;
            //GUILayout.EndHorizontal();


            if (GUILayout.Button("Save Race Settings")) {
                SaveRaceSettings();
            }
            EditorGUILayout.EndScrollView();
        }

        #region Body Parts

        #endregion

        #region Saving
        private void SaveRaceSettings() {
            string path = "Assets/CombatPrototype/Data/RaceSettings/" + currRaceSetting.race.ToString() + ".json";
            if (Utilities.DoesFileExist(path)) {
                if (EditorUtility.DisplayDialog("Overwrite Race Setting", "A race setting with name " + currRaceSetting.race.ToString() + " already exists. Replace with these settings?", "Yes", "No")) {
                    File.Delete(path);
                    SaveRaceJson(path);
                }
            } else {
                SaveRaceJson(path);
            }
        }
        private void SaveRaceJson(string path) {
            string jsonString = JsonUtility.ToJson(currRaceSetting);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(jsonString);
            writer.Close();
            Debug.Log("Successfully saved file at " + path);
        }
        #endregion

        #region Loading
        private void LoadRaceSettings(RACE race) {
            string path = "Assets/CombatPrototype/Data/RaceSettings/" + race.ToString() + ".json";
            string dataAsJson = File.ReadAllText(path);
            currRaceSetting = JsonUtility.FromJson<RaceSetting>(dataAsJson);
        }
        #endregion

    }
}