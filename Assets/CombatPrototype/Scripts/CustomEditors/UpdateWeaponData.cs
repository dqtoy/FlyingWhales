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
			string weaponTypePath = "Assets/CombatPrototype/Data/WeaponTypes/";
			foreach (string weaponTypeFile in System.IO.Directory.GetFiles(weaponTypePath, "*.json")) {
				WeaponType weaponType = JsonUtility.FromJson<WeaponType> (System.IO.File.ReadAllText (weaponTypeFile));
				if(weaponType.weaponMaterials != null){
					for (int i = 0; i < weaponType.weaponMaterials.Count; i++) {
						Weapon weapon = CreateWeapon (weaponType.weaponMaterials[i], weaponType);
						SaveWeapon (weapon);
					}
				}
			}
		}

		private static Weapon CreateWeapon(WeaponMaterial weaponMaterial, WeaponType weaponType){
			Weapon weapon = new Weapon ();
			weapon.weaponType = weaponType.weaponType;
			weapon.material = weaponMaterial.material;
			weapon.quality = QUALITY.NORMAL;
			weapon.weaponPower = weaponMaterial.power;
			weapon.durability = weaponMaterial.durability;
			weapon.cost = weaponMaterial.cost;
			weapon.equipRequirements = new List<IBodyPart.ATTRIBUTE> (weaponType.equipRequirements);
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
