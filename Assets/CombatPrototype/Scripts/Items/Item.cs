using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Item : EntityComponent {
        public ITEM_TYPE itemType;
		public string itemName;
		public string description;
        public string interactString;
        public bool isUnlimited;
        public bool _isObtainable;
        public int goldCost;

        protected ECS.Character _owner; //Not included in CreateNewCopy
        protected bool _isEquipped;
        protected int _id;

        //protected TaskCreator _possessor; //Not included in CreateNewCopy

        #region getters/setters
        public bool isEquipped{
			get { return _isEquipped; }
		}
        public bool isObtainable {
            get { return (itemType != ITEM_TYPE.ITEM ? true : _isObtainable); }
        }
        public int id {
            get { return _id; }
        }
        public ECS.Character owner{
			get { return _owner; }
		}
        #endregion

        public Item() {
            _id = Utilities.SetID(this);
        }

    //    public void AdjustDurability(int adjustment) {
    //        currDurability += adjustment;
    //        currDurability = Mathf.Clamp(currDurability, 0, durability);
    //        if (currDurability == 0) {
    //            //Item Destroyed! Unequip item if armor or weapon
				//_owner.UnequipItem(this);
    //        }
    //    }
    //    public void ResetDurability() {
    //        currDurability = durability;
    //    }
        public void SetOwner(ECS.Character owner) {
            _owner = owner;
        }
		//public void SetPossessor(TaskCreator possessor) {
		//	_possessor = possessor;
		//}
		public void SetEquipped(bool state){
			this._isEquipped = state;
		}

        //public void SetExploreWeight(int weight) {
        //    exploreWeight = weight;
        //}
        //public void SetcollectChance(int weight) {
        //    collectChance = weight;
        //}
        public void SetIsUnlimited(bool state) {
            isUnlimited = state;
        }

        public void OnItemPlacedOnLandmark(BaseLandmark landmark) {
            Messenger.Broadcast(Signals.ITEM_PLACED_LANDMARK, this, landmark);
        }
        public void OnItemPutInInventory(ECS.Character character) {
            Messenger.Broadcast(Signals.ITEM_PLACED_INVENTORY, this, character);
        }

        #region virtuals
        public virtual Item CreateNewCopy() {
            Item copy = new Item();
            SetCommonData(copy);
            return copy;
        }
        #endregion

        protected void SetCommonData(Item item) {
            item.itemType = itemType;
            item.itemName = itemName;
            item.description = description;
            //         item.bonusActRate = bonusActRate;
            //         item.bonusStrength = bonusStrength;
            //         item.bonusIntelligence = bonusIntelligence;
            //         item.bonusAgility = bonusAgility;
            //         item.bonusVitality = bonusVitality;
            //         item.bonusMaxHP = bonusMaxHP;
            //         item.bonusDodgeRate = bonusDodgeRate;
            //         item.bonusParryRate = bonusParryRate;
            //         item.bonusBlockRate = bonusBlockRate;
            //         item.durability = durability;
            //         item.currDurability = currDurability;
            //item.cost = cost;
            //item.exploreWeight = exploreWeight;
            //item.collectChance = collectChance;
            item.isUnlimited = isUnlimited;
            item._isObtainable = isObtainable;
            item.interactString = interactString;
            item.goldCost = goldCost;
            //item.statusEffectResistances = new List<StatusEffectRate>(statusEffectResistances);
        }
    }
}

