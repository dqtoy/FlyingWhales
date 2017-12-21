using UnityEngine;
using System.Collections;

namespace ECS{
	[System.Serializable]
	public struct ItemAndType {
		[ReadOnly] public string itemName;
		public ITEM_TYPE itemType;

		public ItemAndType(ITEM_TYPE itemType, string itemName){
			this.itemName = itemName;
			this.itemType = itemType;
		}
	}

}
