using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillComponent : MonoBehaviour {
	public SKILL_TYPE skillType;
    public SKILL_CATEGORY skillCategory;
    public string skillName;
	public string description;
	//public ACTIVATION_WEIGHT_TYPE actWeightType;
	public int activationWeight;
	//public float accuracy;
	public int range;
    public TARGET_TYPE targetType;
    public int numOfRowsHit;

    //Attack Skill Fields
    public int power;
    public int spCost;
    public ATTACK_CATEGORY attackCategory;
    public ELEMENT element;


	//Heal Skill Fields
	public int healPower;

    //Body Part
    //public SkillRequirement[] skillRequirements;

    //Weapon
    public WEAPON_TYPE[] allowedWeaponTypes;
}