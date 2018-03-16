using UnityEngine;
using System.Collections;

public class SeverePsytoxin : CharacterTag {
	public SeverePsytoxin(ECS.Character character): base(character, CHARACTER_TAG.SEVERE_PSYTOXIN){
	}

	#region Overrides
	public override void Initialize (){
		base.Initialize ();
		if(Messenger.eventTable.ContainsKey("Psytoxinated")){
			Messenger.Broadcast ("Psytoxinated");
		}
		_character.AssignRole (CHARACTER_ROLE.SLYX);
	}
	public override void OnRemoveTag (){
		base.OnRemoveTag ();
		if (Messenger.eventTable.ContainsKey ("Unpsytoxinated")) {
			Messenger.Broadcast ("Unpsytoxinated");
		}
	}
	#endregion
}
