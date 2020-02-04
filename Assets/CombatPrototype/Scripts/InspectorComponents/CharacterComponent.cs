using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterComponent : MonoBehaviour {
	public string fileName;

	public string characterClassName;
	//public string raceSettingName;
	public CHARACTER_ROLE optionalRole;
    public List<ATTRIBUTE> tags;

	public int currCharacterSelectedIndex;
	//public int currRaceSelectedIndex;
	public int currItemSelectedIndex;

	//public List<string> raceChoices;
	public List<string> characterClassChoices;

	public bool itemFoldout;
	public ITEM_TYPE itemTypeToAdd;

	public void AddItem(string itemName){
		
	}
//		public void RemoveItem(string itemName){
//			preEquippedItems.Remove (itemName);
//		}
//		public void RemoveItem(int index){
//			preEquippedItems.RemoveAt (index);
//		}
}