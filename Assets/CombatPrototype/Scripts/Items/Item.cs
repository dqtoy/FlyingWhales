using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Item : EntityComponent {
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
		public int durability;
        public int currDurability;
		public List<StatusEffectResistance> statusEffectResistances = new List<StatusEffectResistance>();

        public Item() {
            currDurability = durability;
        }

        public void AdjustDurability(int adjustment) {
            currDurability += adjustment;
            currDurability = Mathf.Clamp(currDurability, 0, durability);
            if (currDurability == 0) {
                //Item Destroyed!
            }
        }
    }
}

