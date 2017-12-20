using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Item : EntityComponent {
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
        public int currDurability;
		public List<StatusEffectRate> statusEffectResistances = new List<StatusEffectRate>();

        protected Character _owner;

        public void AdjustDurability(int adjustment) {
            currDurability += adjustment;
            currDurability = Mathf.Clamp(currDurability, 0, durability);
            if (currDurability == 0) {
                //Item Destroyed! Unequip item if armor or weapon
                if(this.itemType == ITEM_TYPE.WEAPON) {
                    Weapon currWeapon = (Weapon)this;
                    _owner.UnequipWeapon(currWeapon);
                } else if(this.itemType == ITEM_TYPE.ARMOR) {
                    Armor currArmor = (Armor)this;
                    _owner.UnequipArmor(currArmor);
                }
            }
        }

        public void ResetDurability() {
            currDurability = durability;
        }

        public void SetOwner(Character owner) {
            _owner = owner;
        }
    }
}

