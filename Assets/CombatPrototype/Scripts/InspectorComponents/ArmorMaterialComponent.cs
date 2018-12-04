using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArmorMaterialComponent: MonoBehaviour {
	//The stats here is for NORMAL quality
	public MATERIAL material;
	public float baseDamageMitigation;
	public float damageNullificationChance;
	public List<ATTACK_TYPE> ineffectiveAttackTypes;
	public List<ATTACK_TYPE> effectiveAttackTypes;
	public int durability;
}