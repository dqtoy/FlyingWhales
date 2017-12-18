using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class ItemComponent : MonoBehaviour {
		public ITEM_TYPE itemType;
		public string itemName;
		public int bonusActRate;
		public int bonusStrength;
		public int bonusIntelligence;
		public int bonusAgility;
		public int bonusMaxHP;
		public int bonusDodgeRate;
		public int bonusParryRate;
		public int bonusBlockRate;
		public List<StatusEffectResistance> statusEffectResistances = new List<StatusEffectResistance>();

		//Weapon Fields
		public WEAPON_TYPE weaponType;
		public float skillPowerModifier;
		public List<IBodyPart.ATTRIBUTE> weaponAttributes = new List<IBodyPart.ATTRIBUTE>();

		//Armor Fields
		public ARMOR_TYPE armorType;
		public float damageMitigation;
		public List<IBodyPart.ATTRIBUTE> armorAttributes = new List<IBodyPart.ATTRIBUTE>();
	}

}
