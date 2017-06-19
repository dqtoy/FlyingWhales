using UnityEngine;
using System.Collections;

[System.Serializable]
public struct CharacterValue {
	public CHARACTER_VALUE character;
	public int value;

	public CharacterValue(CHARACTER_VALUE character, int value){
		this.character = character;
		this.value = value;
	}
}
