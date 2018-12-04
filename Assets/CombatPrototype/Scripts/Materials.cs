using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[System.Serializable]
public class Materials {
	public MATERIAL material;
	public MATERIAL_CATEGORY category;
//	public TECHNOLOGY technology;
	public int weight;
	public bool isEdible;
	public Structure structure;

	//Weapon Data
	public WeaponMaterial weaponData;

	//Armor Data
	public ArmorMaterial armorData;

	//Construction Data
	public int sturdiness;

	//Training Data
	public int trainingStatBonus;
}
