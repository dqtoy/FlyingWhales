#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
	[CustomEditor(typeof(AttributeSkillComponent))]
	public class AttributeSkillCreator : Editor {

		AttributeSkillComponent attributeComponent;

		public override void OnInspectorGUI() {
			if(attributeComponent == null){
				attributeComponent = (AttributeSkillComponent)target;
			}
			attributeComponent.fileName = EditorGUILayout.TextField ("File Name: ", attributeComponent.fileName);
			SerializedProperty skillRequirement = serializedObject.FindProperty("requirements");
			EditorGUILayout.PropertyField(skillRequirement, true);
			serializedObject.ApplyModifiedProperties();

			attributeComponent.skillsFoldout = EditorGUILayout.Foldout(attributeComponent.skillsFoldout, "Skills");

			if (attributeComponent.skillsFoldout && attributeComponent.skills != null) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < attributeComponent.skills.Count; i++) {
					SerializedProperty currSkill = serializedObject.FindProperty("skills").GetArrayElementAtIndex(i);
					EditorGUILayout.PropertyField(currSkill, true);
				}
				serializedObject.ApplyModifiedProperties();
				EditorGUI.indentLevel--;
			}

			//Add Skill Area
			GUILayout.Space(10);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUILayout.Label("Add Skills ", EditorStyles.boldLabel);
			attributeComponent.skillTypeToAdd = (SKILL_TYPE)EditorGUILayout.EnumPopup("Skill Type To Add: ", attributeComponent.skillTypeToAdd);
			List<string> choices = GetAllSkillsOfType(SKILL_CATEGORY.BODY_PART, attributeComponent.skillTypeToAdd);
			attributeComponent.skillToAddIndex = EditorGUILayout.Popup("Skill To Add: ", attributeComponent.skillToAddIndex, choices.ToArray());
			GUI.enabled = choices.Count > 0;
			if (GUILayout.Button("Add Skill")) {
				AddSkillToList(choices[attributeComponent.skillToAddIndex]);
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();

			if (GUILayout.Button("Save Attribute Skill")) {
				SaveAttributeSkill(attributeComponent.fileName);
			}
		}

		private void SaveAttributeSkill(string fileName) {
			if (string.IsNullOrEmpty(fileName)) {
				EditorUtility.DisplayDialog("Error", "Please specify a filename", "OK");
				return;
			}
			string path = "Assets/CombatPrototype/Data/AttributeSkills/" + fileName + ".json";
			if (Utilities.DoesFileExist(path)) {
				if (EditorUtility.DisplayDialog("Overwrite File", "A file with name " + fileName + " already exists. Replace with this file?", "Yes", "No")) {
					File.Delete(path);
					Save(path);
				}
			} else {
				Save(path);
			}
		}
		private void Save(string path){
			AttributeSkill attributeSkill = new AttributeSkill ();
			attributeSkill.requirements = attributeComponent.requirements;
			for (int i = 0; i < attributeComponent.skills.Count; i++) {
				attributeSkill.AddSkill (attributeComponent.skills [i]);
			}

			string jsonString = JsonUtility.ToJson(attributeSkill);

			System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
			writer.WriteLine(jsonString);
			writer.Close();
		}

		#region Skills
		private List<string> GetAllSkillsOfType(SKILL_CATEGORY category, SKILL_TYPE skillType) {
			List<string> allSkillsOfType = new List<string>();
			string path = "Assets/CombatPrototype/Data/Skills/" + category.ToString() + "/" + skillType.ToString() + "/";
			foreach (string file in Directory.GetFiles(path, "*.json")) {
				allSkillsOfType.Add(Path.GetFileNameWithoutExtension(file));
			}
			return allSkillsOfType;
		}
		private void AddSkillToList(string skillName) {
			string path = "Assets/CombatPrototype/Data/Skills/BODY_PART/" + attributeComponent.skillTypeToAdd.ToString() + "/" + skillName + ".json";
			string dataAsJson = File.ReadAllText(path);
			switch (attributeComponent.skillTypeToAdd) {
			case SKILL_TYPE.ATTACK:
				AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill>(dataAsJson);
				attributeComponent.AddSkill(attackSkill);
				break;
			case SKILL_TYPE.HEAL:
				HealSkill healSkill = JsonUtility.FromJson<HealSkill>(dataAsJson);
				attributeComponent.AddSkill(healSkill);
				break;
			case SKILL_TYPE.OBTAIN_ITEM:
				ObtainSkill obtainSkill = JsonUtility.FromJson<ObtainSkill>(dataAsJson);
				attributeComponent.AddSkill(obtainSkill);
				break;
			case SKILL_TYPE.FLEE:
				FleeSkill fleeSkill = JsonUtility.FromJson<FleeSkill>(dataAsJson);
				attributeComponent.AddSkill(fleeSkill);
				break;
			case SKILL_TYPE.MOVE:
				MoveSkill moveSkill = JsonUtility.FromJson<MoveSkill>(dataAsJson);
				attributeComponent.AddSkill(moveSkill);
				break;
			}
		}
		#endregion
	}
}
#endif