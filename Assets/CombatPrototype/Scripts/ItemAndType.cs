using UnityEngine;
using System.Collections;

[System.Serializable]
public struct ItemAndType {
	[ReadOnly] public string itemName;
	[ReadOnly] public ITEM_TYPE itemType;

	public ItemAndType(ITEM_TYPE itemType, string itemName){
		this.itemName = itemName;
		this.itemType = itemType;
	}
}