using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Item : EntityComponent {
		public string itemName;
		public int bonusActRate;
		public int bonusStrength;
		public int bonusIntelligence;
		public int bonusAgility;
		public int bonusMaxHP;
		public int bonusDodgeRate;
		public int bonusParryRate;
		public int bonusBlockRate;
		public int durability;
		public List<StatusEffectResistance> statusEffectResistances = new List<StatusEffectResistance>();
	}
}

