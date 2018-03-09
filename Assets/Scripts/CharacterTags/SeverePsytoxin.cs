using UnityEngine;
using System.Collections;

public class SeverePsytoxin : CharacterTag {
	public SeverePsytoxin(ECS.Character character): base(character, CHARACTER_TAG.SEVERE_PSYTOXIN){
	}

	#region Overrides
	public override void Initialize (){
		base.Initialize ();
		_character.AssignRole (CHARACTER_ROLE.SLYX);
	}
	#endregion
}
