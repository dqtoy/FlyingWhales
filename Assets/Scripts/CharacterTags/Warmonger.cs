using UnityEngine;
using System.Collections;

public class Warmonger : CharacterTag {
	public Warmonger(ECS.Character character): base(character, CHARACTER_TAG.WARMONGER){
	}
}
