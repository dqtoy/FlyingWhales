using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterComponent : MonoBehaviour {
	public string fileName;

	public string characterClassName;
	//public string raceSettingName;
	public CHARACTER_ROLE optionalRole;
    public List<ATTRIBUTE> tags;

	[SerializeField] internal List<ItemAndType> preEquippedItems;

	public int currCharacterSelectedIndex;
	//public int currRaceSelectedIndex;
	public int currItemSelectedIndex;

	//public List<string> raceChoices;
	public List<string> characterClassChoices;

	public bool itemFoldout;
	public ITEM_TYPE itemTypeToAdd;

	public void AddItem(string itemName){
		if (preEquippedItems == null) {
			preEquippedItems = new List<ItemAndType> ();
		}
		preEquippedItems.Add (new ItemAndType(itemTypeToAdd, itemName));
	}
//		public void RemoveItem(string itemName){
//			preEquippedItems.Remove (itemName);
//		}
//		public void RemoveItem(int index){
//			preEquippedItems.RemoveAt (index);
//		}
}