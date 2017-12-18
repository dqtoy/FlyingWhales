using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace ECS {
	[CustomEditor(typeof(ItemComponent))]
    public class ItemCreator : Editor {
		ItemComponent itemComponent;

        public override void OnInspectorGUI() {
			if(itemComponent == null){
				itemComponent = (ItemComponent)target;
			}

            GUILayout.Label("Item Creator ", EditorStyles.boldLabel);
			itemComponent.itemType = (ITEM_TYPE)EditorGUILayout.EnumPopup("Item Type: ", itemComponent.itemType);
			itemComponent.itemName = EditorGUILayout.TextField("Item Name: ", itemComponent.itemName);

			switch (itemComponent.itemType) {
			case ITEM_TYPE.WEAPON:
				ShowWeaponFields();
				break;
			case ITEM_TYPE.ARMOR:
				ShowArmorFields();
				break;
			}

			itemComponent.bonusActRate = EditorGUILayout.IntField("Bonus Act Rate: ", itemComponent.bonusActRate);
			itemComponent.bonusStrength = EditorGUILayout.IntField("Bonus Strength: ", itemComponent.bonusStrength);
			itemComponent.bonusIntelligence = EditorGUILayout.IntField("Bonus Intelligence: ", itemComponent.bonusIntelligence);
			itemComponent.bonusAgility = EditorGUILayout.IntField("Bonus Agility: ",itemComponent. bonusAgility);
			itemComponent.bonusMaxHP = EditorGUILayout.IntField("Bonus Max HP: ", itemComponent.bonusMaxHP);
			itemComponent.bonusDodgeRate = EditorGUILayout.IntField("Bonus Dodge Rate: ", itemComponent.bonusDodgeRate);
			itemComponent.bonusParryRate = EditorGUILayout.IntField("Bonus Parry Rate: ", itemComponent.bonusParryRate);
			itemComponent.bonusBlockRate = EditorGUILayout.IntField("Bonus Block Rate: ", itemComponent.bonusBlockRate);

			SerializedProperty statusEffectResistance = serializedObject.FindProperty("statusEffectResistances");
			EditorGUILayout.PropertyField(statusEffectResistance, true);
			serializedObject.ApplyModifiedProperties ();

            if (GUILayout.Button("Save Item")) {
				SaveItem(itemComponent.itemName);
            }
        }

        private void ShowWeaponFields() {
			itemComponent.weaponType = (WEAPON_TYPE)EditorGUILayout.EnumPopup("Weapon Type: ", itemComponent.weaponType);
			itemComponent.skillPowerModifier = EditorGUILayout.FloatField("Skill Power Modifier: ", itemComponent.skillPowerModifier);

			SerializedProperty weaponAttribute = serializedObject.FindProperty("weaponAttributes");
			EditorGUILayout.PropertyField(weaponAttribute, true);
			serializedObject.ApplyModifiedProperties ();
        }
        private void ShowArmorFields() {
			itemComponent.armorType = (ARMOR_TYPE)EditorGUILayout.EnumPopup("Armor Type: ", itemComponent.armorType);
			itemComponent.damageMitigation = EditorGUILayout.FloatField("Damage Mitigation: ", itemComponent.damageMitigation);

			SerializedProperty armorAttribute = serializedObject.FindProperty("armorAttributes");
			EditorGUILayout.PropertyField(armorAttribute, true);
			serializedObject.ApplyModifiedProperties ();
        }

		private void ConstructItem(){
			
		}
        #region Saving
        private void SaveItem(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                EditorUtility.DisplayDialog("Error", "Please specify an Item Name", "OK");
                return;
            }
			string path = "Assets/CombatPrototype/Data/Items/" + itemComponent.itemType.ToString() + "/" + fileName + ".json";
            if (Utilities.DoesFileExist(path)) {
                if (EditorUtility.DisplayDialog("Overwrite Item", "An item with name " + fileName + " already exists. Replace with this item?", "Yes", "No")) {
                    File.Delete(path);
					SaveItemJson(path);
                }
            } else {
				SaveItemJson(path);
            }
        }
        private void SaveItemJson(string path) {
			if(itemComponent.itemType == ITEM_TYPE.WEAPON) {
				SaveWeapon(path);
			} else if (itemComponent.itemType == ITEM_TYPE.ARMOR) {
                SaveArmor(path);
            }

            //Re-import the file to update the reference in the editor
            UnityEditor.AssetDatabase.ImportAsset(path);
            Debug.Log("Successfully saved item at " + path);
        }
		private void SetCommonData(Item newItem) {
			newItem.itemName = itemComponent.itemName;
			newItem.bonusActRate = itemComponent.bonusActRate;
			newItem.bonusStrength = itemComponent.bonusStrength;
			newItem.bonusIntelligence = itemComponent.bonusIntelligence;
			newItem.bonusAgility = itemComponent.bonusAgility;
			newItem.bonusMaxHP = itemComponent.bonusMaxHP;
			newItem.bonusDodgeRate = itemComponent.bonusDodgeRate;
			newItem.bonusParryRate = itemComponent.bonusParryRate;
			newItem.bonusBlockRate = itemComponent.bonusBlockRate;
        }
        private void SaveWeapon(string path) {
			Weapon weapon = new Weapon();

			SetCommonData(weapon);

			weapon.weaponType = itemComponent.weaponType;
			weapon.skillPowerModifier = itemComponent.skillPowerModifier;
			weapon.attributes = itemComponent.weaponAttributes;

			SaveJson(weapon, path);
        }
		private void SaveArmor(string path) {
			Armor armor = new Armor();

			SetCommonData(armor);

			armor.armorType = itemComponent.armorType;
			armor.damageMitigation = itemComponent.damageMitigation;
			armor.attributes = itemComponent.armorAttributes;

			SaveJson(armor, path);
		}
		private void SaveJson(Item item, string path) {
			string jsonString = JsonUtility.ToJson(item);

            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(jsonString);
            writer.Close();
        }
        #endregion

        #region Loading
//        private void LoadItem() {
//            string filePath = EditorUtility.OpenFilePanel("Select Item Json", "Assets/CombatPrototype/Data/Items/", "json");
//            if (!string.IsNullOrEmpty(filePath)) {
//                string dataAsJson = File.ReadAllText(filePath);
//                if (filePath.Contains("WEAPON")) {
//					Weapon weapon = JsonUtility.FromJson<Weapon>(dataAsJson);
//					LoadWeapon(weapon);
//                } else if (filePath.Contains("ARMOR")) {
//					Armor armor = JsonUtility.FromJson<Armor>(dataAsJson);
//					LoadArmor(armor);
//                }
//            }
//        }
//		private void LoadCommonData(Item newItem) {
//			this.itemName = newItem.itemName;
//			this.bonusActRate = newItem.bonusActRate;
//			this.bonusStrength = newItem.bonusStrength;
//			this.bonusIntelligence = newItem.bonusIntelligence;
//			this.bonusAgility = newItem.bonusAgility;
//			this.bonusMaxHP = newItem.bonusMaxHP;
//			this.bonusDodgeRate = newItem.bonusDodgeRate;
//			this.bonusParryRate = newItem.bonusParryRate;
//			this.bonusBlockRate = newItem.bonusBlockRate;
//        }
//        private void LoadWeapon(Weapon weapon) {
//            itemType = ITEM_TYPE.WEAPON;
//			LoadCommonData(weapon);
//
//            //Weapon Fields
//			this.weaponType = weapon.weaponType;
//			this.skillPowerModifier = weapon.skillPowerModifier;
//			this.weaponAttributes = weapon.attributes;
//        }
//		private void LoadArmor(Armor armor) {
//            itemType = ITEM_TYPE.ARMOR;
//			LoadCommonData (armor);
//
//            //Armor Fields
//			this.armorType = armor.armorType;
//			this.damageMitigation = armor.damageMitigation;
//			this.armorAttributes = armor.attributes;
//		}
        #endregion
    }
}

