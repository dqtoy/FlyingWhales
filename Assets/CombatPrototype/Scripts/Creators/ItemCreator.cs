using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace ECS {
    public class ItemCreator : EditorWindow {

        private Vector2 scrollPos = Vector2.zero;

        private ITEM_TYPE itemType;
        private string itemName;
		private int bonusActRate;
		private int bonusStrength;
		private int bonusIntelligence;
		private int bonusAgility;
		private int bonusMaxHP;
		private int bonusDodgeRate;
		private int bonusParryRate;
		private int bonusBlockRate;
        private int durability;
        private Dictionary<STATUS_EFFECT, int> statusEffectResistances = new Dictionary<STATUS_EFFECT, int>();

        //Weapon Fields
        private WEAPON_TYPE weaponType;
		private float weaponPower;
        private int durabilityDamage;
        public List<IBodyPart.ATTRIBUTE> equipRequirements;
        public List<IBodyPart.ATTRIBUTE> weaponAttributes = new List<IBodyPart.ATTRIBUTE>();

		//Armor Fields
		private ARMOR_TYPE armorType;
        private BODY_PART armorBodyType;
		private int hitPoints;
		public List<IBodyPart.ATTRIBUTE> armorAttributes = new List<IBodyPart.ATTRIBUTE>();

        // Add menu item to the Window menu
        [MenuItem("Window/Item Creator")]
        public static void ShowWindow() {
            //Show existing window instance. If one doesn't exist, make one.
			EditorWindow.GetWindow(typeof(ItemCreator));
        }

        private void OnGUI() {
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height));
            GUILayout.Label("Item Creator ", EditorStyles.boldLabel);
			itemType = (ITEM_TYPE)EditorGUILayout.EnumPopup("Item Type: ", itemType);
			itemName = EditorGUILayout.TextField("Item Name: ", itemName);

			switch (itemType) {
			case ITEM_TYPE.WEAPON:
				ShowWeaponFields();
				break;
			case ITEM_TYPE.ARMOR:
				ShowArmorFields();
				break;
			}

			bonusActRate = EditorGUILayout.IntField("Bonus Act Rate: ", bonusActRate);
			bonusStrength = EditorGUILayout.IntField("Bonus Strength: ", bonusStrength);
			bonusIntelligence = EditorGUILayout.IntField("Bonus Intelligence: ", bonusIntelligence);
			bonusAgility = EditorGUILayout.IntField("Bonus Agility: ", bonusAgility);
			bonusMaxHP = EditorGUILayout.IntField("Bonus Max HP: ", bonusMaxHP);
			bonusDodgeRate = EditorGUILayout.IntField("Bonus Dodge Rate: ", bonusDodgeRate);
			bonusParryRate = EditorGUILayout.IntField("Bonus Parry Rate: ", bonusParryRate);
			bonusBlockRate = EditorGUILayout.IntField("Bonus Block Rate: ", bonusBlockRate);
            durability = EditorGUILayout.IntField("Durability :", durability);

            if (GUILayout.Button("Save Item")) {
                SaveItem(itemName);
            }

            if (GUILayout.Button("Load Item")) {
                LoadItem();
            }

            EditorGUILayout.EndScrollView();
        }

        private void ShowWeaponFields() {
			weaponType = (WEAPON_TYPE)EditorGUILayout.EnumPopup("Weapon Type: ", weaponType);
			weaponPower = EditorGUILayout.FloatField("Weapon Power: ", weaponPower);
            durabilityDamage = EditorGUILayout.IntField("Durability Damage: ", durabilityDamage);

            SerializedObject serializedObject = new SerializedObject(this);
			SerializedProperty weaponAttribute = serializedObject.FindProperty("weaponAttributes");
			EditorGUILayout.PropertyField(weaponAttribute, true);
			serializedObject.ApplyModifiedProperties ();

            SerializedProperty equipRequirements = serializedObject.FindProperty("equipRequirements");
            EditorGUILayout.PropertyField(equipRequirements, true);
            serializedObject.ApplyModifiedProperties();
        }
        private void ShowArmorFields() {
			armorType = (ARMOR_TYPE)EditorGUILayout.EnumPopup("Armor Type: ", armorType);
            armorBodyType = (BODY_PART)EditorGUILayout.EnumPopup("Body Armor Type: ", armorBodyType);
            hitPoints = EditorGUILayout.IntField("Hitpoints: ", hitPoints);

			SerializedObject serializedObject = new SerializedObject(this);
			SerializedProperty armorAttribute = serializedObject.FindProperty("armorAttributes");
			EditorGUILayout.PropertyField(armorAttribute, true);
			serializedObject.ApplyModifiedProperties ();
        }


        #region Saving
        private void SaveItem(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                EditorUtility.DisplayDialog("Error", "Please specify an Item Name", "OK");
                return;
            }
            string path = "Assets/CombatPrototype/Data/Items/" + itemType.ToString() + "/" + fileName + ".json";
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
            if(itemType == ITEM_TYPE.WEAPON) {
				SaveWeapon(path);
            } else if (itemType == ITEM_TYPE.ARMOR) {
                SaveArmor(path);
            }

            //Re-import the file to update the reference in the editor
            UnityEditor.AssetDatabase.ImportAsset(path);
            Debug.Log("Successfully saved item at " + path);
        }
		private void SetCommonData(Item newItem) {
			newItem.itemName = this.itemName;
			newItem.bonusActRate = this.bonusActRate;
			newItem.bonusStrength = this.bonusStrength;
			newItem.bonusIntelligence = this.bonusIntelligence;
			newItem.bonusAgility = this.bonusAgility;
			newItem.bonusMaxHP = this.bonusMaxHP;
			newItem.bonusDodgeRate = this.bonusDodgeRate;
			newItem.bonusParryRate = this.bonusParryRate;
			newItem.bonusBlockRate = this.bonusBlockRate;
            newItem.durability = this.durability;
        }
        private void SaveWeapon(string path) {
			Weapon weapon = new Weapon();

			SetCommonData(weapon);

			weapon.weaponType = this.weaponType;
			weapon.weaponPower = this.weaponPower;
            weapon.durabilityDamage = this.durabilityDamage;
			weapon.attributes = this.weaponAttributes;
            weapon.equipRequirements = this.equipRequirements;

            SaveJson(weapon, path);
        }
		private void SaveArmor(string path) {
			Armor armor = new Armor();

			SetCommonData(armor);

			armor.armorType = this.armorType;
            armor.armorBodyType = this.armorBodyType;
			armor.hitPoints = this.hitPoints;
			armor.attributes = this.armorAttributes;

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
        private void LoadItem() {
            string filePath = EditorUtility.OpenFilePanel("Select Item Json", "Assets/CombatPrototype/Data/Items/", "json");
            if (!string.IsNullOrEmpty(filePath)) {
                string dataAsJson = File.ReadAllText(filePath);
                if (filePath.Contains("WEAPON")) {
					Weapon weapon = JsonUtility.FromJson<Weapon>(dataAsJson);
					LoadWeapon(weapon);
                } else if (filePath.Contains("ARMOR")) {
					Armor armor = JsonUtility.FromJson<Armor>(dataAsJson);
					LoadArmor(armor);
                }
            }
        }
		private void LoadCommonData(Item newItem) {
			this.itemName = newItem.itemName;
			this.bonusActRate = newItem.bonusActRate;
			this.bonusStrength = newItem.bonusStrength;
			this.bonusIntelligence = newItem.bonusIntelligence;
			this.bonusAgility = newItem.bonusAgility;
			this.bonusMaxHP = newItem.bonusMaxHP;
			this.bonusDodgeRate = newItem.bonusDodgeRate;
			this.bonusParryRate = newItem.bonusParryRate;
			this.bonusBlockRate = newItem.bonusBlockRate;
            this.durability = newItem.durability;
        }
        private void LoadWeapon(Weapon weapon) {
            itemType = ITEM_TYPE.WEAPON;
			LoadCommonData(weapon);

            //Weapon Fields
			this.weaponType = weapon.weaponType;
			this.weaponPower = weapon.weaponPower;
            this.durabilityDamage = weapon.durabilityDamage;
			this.weaponAttributes = weapon.attributes;
            this.equipRequirements = weapon.equipRequirements;
        }
		private void LoadArmor(Armor armor) {
            itemType = ITEM_TYPE.ARMOR;
			LoadCommonData (armor);

            //Armor Fields
			this.armorType = armor.armorType;
            this.armorBodyType = armor.armorBodyType;
			this.hitPoints = armor.hitPoints;
			this.armorAttributes = armor.attributes;
		}
        #endregion
    }
}

