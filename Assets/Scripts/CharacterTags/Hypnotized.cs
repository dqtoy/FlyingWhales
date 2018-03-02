using UnityEngine;
using System.Collections;

public class Hypnotized : CharacterTag {
	public Hypnotized(ECS.Character character): base(character, CHARACTER_TAG.HYPNOTIZED){
	}
}
