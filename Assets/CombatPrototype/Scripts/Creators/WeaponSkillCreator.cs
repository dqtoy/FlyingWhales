using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
	[CustomEditor(typeof(WeaponSkillComponent))]
	public class WeaponSkillCreator : Editor {

		WeaponSkillComponent weaponSkillComponent;

		public override void OnInspectorGUI() {
			if(weaponSkillComponent == null){
				weaponSkillComponent = (WeaponSkillComponent)target;
			}
			weaponSkillComponent.weaponType = (WEAPON_TYPE)EditorGUILayout.EnumPopup ("Weapon Type: ", weaponSkillComponent.weaponType);

			SerializedProperty equipRequirement = serializedObject.FindProperty("equipRequirements");
			EditorGUILayout.PropertyField(equipRequirement, true);
			serializedObject.ApplyModifiedProperties ();

			weaponSkillComponent.skillsFoldout = EditorGUILayout.Foldout(weaponSkillComponent.skillsFoldout, "Skills");

			if (weaponSkillComponent.skillsFoldout && weaponSkillComponent.skills != null) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < weaponSkillComponent.skills.Count; i++) {
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
			weaponSkillComponent.skillTypeToAdd = (SKILL_TYPE)EditorGUILayout.EnumPopup("Skill Type To Add: ", weaponSkillComponent.skillTypeToAdd);
			List<string> choices = GetAllSkillsOfType(SKILL_CATEGORY.WEAPON, weaponSkillComponent.skillTypeToAdd);
			weaponSkillComponent.skillToAddIndex = EditorGUILayout.Popup("Skill To Add: ", weaponSkillComponent.skillToAddIndex, choices.ToArray());
			GUI.enabled = choices.Count > 0;
			if (GUILayout.Button("Add Skill")) {
				AddSkillToList(choices[weaponSkillComponent.skillToAddIndex]);
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();

			if (GUILayout.Button("Save Weapon Skill")) {
				SaveWeaponSkill(Utilities.NormalizeString(weaponSkillComponent.weaponType.ToString()));
			}
		}

		private void SaveWeaponSkill(string fileName) {
			if (string.IsNullOrEmpty(fileName)) {
				EditorUtility.DisplayDialog("Error", "Please specify a filename", "OK");
				return;
			}
			string path = "Assets/CombatPrototype/Data/WeaponTypeSkills/" + fileName + ".json";
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
			WeaponSkill weaponSkill = new WeaponSkill ();
			weaponSkill.weaponType = weaponSkillComponent.weaponType;
			weaponSkill.equipRequirements = weaponSkillComponent.equipRequirements;
			for (int i = 0; i < weaponSkillComponent.skills.Count; i++) {
				weaponSkill.AddSkill (weaponSkillComponent.skills [i]);
			}

			string jsonString = JsonUtility.ToJson(weaponSkill);

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
			string path = "Assets/CombatPrototype/Data/Skills/WEAPON/" + weaponSkillComponent.skillTypeToAdd.ToString() + "/" + skillName + ".json";
			string dataAsJson = File.ReadAllText(path);
			switch (weaponSkillComponent.skillTypeToAdd) {
			case SKILL_TYPE.ATTACK:
				AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill>(dataAsJson);
				weaponSkillComponent.AddSkill(attackSkill);
				break;
			case SKILL_TYPE.HEAL:
				HealSkill healSkill = JsonUtility.FromJson<HealSkill>(dataAsJson);
				weaponSkillComponent.AddSkill(healSkill);
				break;
			case SKILL_TYPE.OBTAIN_ITEM:
				ObtainSkill obtainSkill = JsonUtility.FromJson<ObtainSkill>(dataAsJson);
				weaponSkillComponent.AddSkill(obtainSkill);
				break;
			case SKILL_TYPE.FLEE:
				FleeSkill fleeSkill = JsonUtility.FromJson<FleeSkill>(dataAsJson);
				weaponSkillComponent.AddSkill(fleeSkill);
				break;
			case SKILL_TYPE.MOVE:
				MoveSkill moveSkill = JsonUtility.FromJson<MoveSkill>(dataAsJson);
				weaponSkillComponent.AddSkill(moveSkill);
				break;
			}
		}
		#endregion
	}
}
