using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MaterialComponent: MonoBehaviour {
	public MATERIAL material;
	public MATERIAL_CATEGORY category;
	public TECHNOLOGY technology;
	public int weight;
	public bool isEdible;
	public Structure structure;

	//Weapon Data
	public ECS.WeaponMaterial weaponData;

	//Armor Data
	public ECS.ArmorMaterial armorData;

	//Construction Data
	public int sturdiness;

	//Training Data
	public int trainingStatBonus;
}

