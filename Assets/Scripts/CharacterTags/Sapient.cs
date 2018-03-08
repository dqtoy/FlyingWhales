using UnityEngine;
using System.Collections;

public class Sapient : CharacterTag {
	public Sapient(ECS.Character character): base(character, CHARACTER_TAG.SAPIENT){
	}
}
