using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Materials {
	public MATERIAL material;
	public MATERIAL_CATEGORY category;
	public int weight;
	public bool isEdible;
	public Structure structure;

	//Weapon Data
	public ECS.WeaponMaterial weaponData;

	//Armor Data
	public ECS.ArmorMaterial armorData;

	//Construction Data
	public int sturdiness;
}
