using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace ECS{
	public class UpdateWeaponData : EditorWindow{
		[MenuItem ("Window/Update Weapon Data")]
		static void Init()
		{
			UpdateData ();
		}

		private static void UpdateData(){
			string materialPath = "Assets/CombatPrototype/Data/WeaponMaterials/";
			foreach (string materialFile in System.IO.Directory.GetFiles(materialPath, "*.json")) {
				WeaponMaterial weaponMaterial = JsonUtility.FromJson<WeaponMaterial> (System.IO.File.ReadAllText (materialFile));
				string weaponTypePath = "Assets/CombatPrototype/Data/WeaponTypeSkills/";
				foreach (string weaponTypeFile in System.IO.Directory.GetFiles(weaponTypePath, "*.json")) {
					WeaponSkill weaponTypeSkill = JsonUtility.FromJson<WeaponSkill> (System.IO.File.ReadAllText (weaponTypeFile));
					Weapon weapon = CreateWeapon (weaponMaterial, weaponTypeSkill);
					SaveWeapon (weapon);
				}
			}
		}

		private static Weapon CreateWeapon(WeaponMaterial weaponMaterial, WeaponSkill weaponTypeSkill){
			Weapon weapon = new Weapon ();
			weapon.weaponType = weaponTypeSkill.weaponType;
			weapon.material = weaponMaterial.material;
			weapon.quality = QUALITY.NORMAL;
			weapon.weaponPower = weaponMaterial.power;
			weapon.durability = weaponMaterial.durability;
			weapon.equipRequirements = new List<IBodyPart.ATTRIBUTE> (weaponTypeSkill.equipRequirements);
			weapon.itemType = ITEM_TYPE.WEAPON;
			weapon.itemName = Utilities.NormalizeString (weapon.material.ToString ()) + " " + Utilities.NormalizeString (weapon.weaponType.ToString ());
			weapon.description = weapon.itemName;
			return weapon;
		}

		private static void SaveWeapon(Weapon weapon){
			string path = "Assets/CombatPrototype/Data/Items/WEAPON/" + weapon.itemName + ".json";
			if (Utilities.DoesFileExist(path)) {
				File.Delete(path);
				Save(weapon, path);
			} else {
				Save(weapon, path);
			}
		}

		private static void Save(Weapon weapon, string path){
			string jsonString = JsonUtility.ToJson(weapon);

			System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
			writer.WriteLine(jsonString);
			writer.Close();
		}
	}
}
