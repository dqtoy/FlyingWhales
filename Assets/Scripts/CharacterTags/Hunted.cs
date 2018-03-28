using UnityEngine;
using System.Collections;

public class Hunted : CharacterTag {
	public Hunted(ECS.Character character): base(character, CHARACTER_TAG.HUNTED){
	}
}
