using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Item : EntityComponent, IPlayerPicker {
    public ITEM_TYPE itemType;
	public string itemName;
	public string description;
    public string interactString;
    public string iconName;
    public int goldCost;
    public bool isStackable;
    public List<string> attributeNames;

    protected Character _owner; //Not included in CreateNewCopy
    protected bool _isEquipped;
    protected int _id;

    //protected TaskCreator _possessor; //Not included in CreateNewCopy

    #region getters/setters
    public string thisName {
        get { return itemName; }
    }
    public bool isEquipped{
		get { return _isEquipped; }
	}
    public int id {
        get { return _id; }
    }
    public Character owner{
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
    public void SetOwner(Character owner) {
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

    public void OnItemPlacedOnLandmark(BaseLandmark landmark) {
        Messenger.Broadcast(Signals.ITEM_PLACED_AT_LANDMARK, this, landmark);
    }
    public void OnItemPutInInventory(Character character) {
        Messenger.Broadcast(Signals.ITEM_PLACED_INVENTORY, this, character);
    }

    #region virtuals
    public virtual Item CreateNewCopy() {
        if (isStackable) {
            return ItemManager.Instance.allItems[itemName];
        }
        Item copy = new Item();
        SetCommonData(copy);
        return copy;
    }
    #endregion

    protected void SetCommonData(Item item) {
        item.itemType = itemType;
        item.itemName = itemName;
        item.description = description;
        item.interactString = interactString;
        item.goldCost = goldCost;
        item.iconName = iconName;
        item.isStackable = isStackable;
        item.attributeNames = new List<string>(attributeNames);
    }
}