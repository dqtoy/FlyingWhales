using UnityEngine;
using System.Collections;

public class Vampire : CharacterTag {

	public Vampire(ECS.Character character): base(character, CHARACTER_TAG.VAMPIRE){
		_statsModifierPercentage.intPercentage = 0.25f;
		_statsModifierPercentage.strPercentage = 0.25f;
		_statsModifierPercentage.agiPercentage = 0.25f;
	}
}
