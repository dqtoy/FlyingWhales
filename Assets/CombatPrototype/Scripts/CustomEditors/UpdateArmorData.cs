using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace ECS{
	public class UpdateArmorData : EditorWindow{
		[MenuItem ("Window/Update Armor Data")]
		static void Init()
		{
			UpdateData ();
		}

		private static void UpdateData(){
			string materialPath = "Assets/CombatPrototype/Data/ArmorMaterials/";
			foreach (string materialFile in System.IO.Directory.GetFiles(materialPath, "*.json")) {
				ArmorMaterial armorMaterial = JsonUtility.FromJson<ArmorMaterial> (System.IO.File.ReadAllText (materialFile));
				string armorTypePath = "Assets/CombatPrototype/Data/ArmorTypes/";
				foreach (string armorTypeFile in System.IO.Directory.GetFiles(armorTypePath, "*.json")) {
					ArmorType armorType = JsonUtility.FromJson<ArmorType> (System.IO.File.ReadAllText (armorTypeFile));
					Armor armor = CreateArmor (armorMaterial, armorType);
					SaveArmor (armor);
				}
			}
		}

		private static Armor CreateArmor(ArmorMaterial armorMaterial, ArmorType armorType){
			Armor armor = new Armor ();
			armor.armorType = armorType.armorType;
			armor.armorBodyType = armorType.armorBodyType;
			armor.material = armorMaterial.material;
			armor.quality = QUALITY.NORMAL;
			armor.baseDamageMitigation = armorMaterial.baseDamageMitigation;
			armor.damageNullificationChance = armorMaterial.damageNullificationChance;
			armor.ineffectiveAttackTypes = new List<ATTACK_TYPE> (armorMaterial.ineffectiveAttackTypes);
			armor.effectiveAttackTypes = new List<ATTACK_TYPE> (armorMaterial.effectiveAttackTypes);
			armor.durability = armorMaterial.durability;
			armor.itemType = ITEM_TYPE.ARMOR;
			armor.itemName = Utilities.NormalizeString (armor.material.ToString ()) + " " + Utilities.NormalizeString (armor.armorType.ToString ());
			armor.description = armor.itemName;
			return armor;
		}

		private static void SaveArmor(Armor armor){
			string path = "Assets/CombatPrototype/Data/Items/ARMOR/" + armor.itemName + ".json";
			if (Utilities.DoesFileExist(path)) {
				File.Delete(path);
				Save(armor, path);
			} else {
				Save(armor, path);
			}
		}

		private static void Save(Armor armor, string path){
			string jsonString = JsonUtility.ToJson(armor);

			System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
			writer.WriteLine(jsonString);
			writer.Close();
		}
	}
}
