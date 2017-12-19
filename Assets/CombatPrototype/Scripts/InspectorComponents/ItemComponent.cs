using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class ItemComponent : MonoBehaviour {
		public ITEM_TYPE itemType;
		public string itemName;
		public string description;
		public int bonusActRate;
		public int bonusStrength;
		public int bonusIntelligence;
		public int bonusAgility;
		public int bonusMaxHP;
		public int bonusDodgeRate;
		public int bonusParryRate;
		public int bonusBlockRate;
		public int durability;
		public List<StatusEffectRate> statusEffectResistances = new List<StatusEffectRate>();

		//Weapon Fields
		public WEAPON_TYPE weaponType;
		public float weaponPower;
		public int durabilityDamage;
		public List<IBodyPart.ATTRIBUTE> equipRequirements = new List<IBodyPart.ATTRIBUTE>();
		public List<IBodyPart.ATTRIBUTE> weaponAttributes = new List<IBodyPart.ATTRIBUTE>();


		//Armor Fields
		public ARMOR_TYPE armorType;
		public BODY_PART armorBodyType;
		public int hitPoints;
		public List<IBodyPart.ATTRIBUTE> armorAttributes = new List<IBodyPart.ATTRIBUTE>();
	}

}
