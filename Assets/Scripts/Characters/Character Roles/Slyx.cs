using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Slyx : CharacterRole {

	public Slyx(ECS.Character character): base (character) {
		_roleType = CHARACTER_ROLE.SLYX;
		_cancelsAllOtherTasks = true;
		_roleTasks.Clear ();

		if(Messenger.eventTable.ContainsKey("SlyxTransform")){
			Messenger.Broadcast ("SlyxTransform");
		}

		Messenger.AddListener<ILocation> ("CallSlyx", CallThisSlyx);
	}

	#region Overrides
	public override void DeathRole (){
		base.DeathRole ();
		if(Messenger.eventTable.ContainsKey("CallSlyx")){
			Messenger.RemoveListener<ILocation> ("CallSlyx", CallThisSlyx);
		}
		InfectPsytoxinToAllCharactersInLandmark ();
	}
	public override void ChangedRole (){
		base.ChangedRole ();
		if(Messenger.eventTable.ContainsKey("CallSlyx")){
			Messenger.RemoveListener<ILocation> ("CallSlyx", CallThisSlyx);
		}
	}
	#endregion

	private void CallThisSlyx(ILocation location){
		_character.GoToLocation (location, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP, () => _character.DestroyAvatar());
		Messenger.RemoveListener<ILocation> ("CallSlyx", CallThisSlyx);
	}

	private void InfectPsytoxinToAllCharactersInLandmark(){
		for (int i = 0; i < _character.specificLocation.charactersAtLocation.Count; i++){
			if(_character.specificLocation.charactersAtLocation[i] is Party){
				Party party = (Party)_character.specificLocation.charactersAtLocation [i];
				for (int j = 0; j < party.partyMembers.Count; j++) {
					ECS.Character character = party.partyMembers [j];
					if(!character.HasTag(CHARACTER_TAG.SEVERE_PSYTOXIN)){
						if(character.role != null && (character.role.roleType == CHARACTER_ROLE.SLYX || character.role.roleType == CHARACTER_ROLE.CRATER_BEAST)){
							continue;
						}
						InfectPsytoxin (character);
					}
				}
			}else if(_character.specificLocation.charactersAtLocation[i] is ECS.Character){
				ECS.Character character = (ECS.Character) _character.specificLocation.charactersAtLocation[i];
				if(!character.HasTag(CHARACTER_TAG.SEVERE_PSYTOXIN)){
					if(character.role != null && (character.role.roleType == CHARACTER_ROLE.SLYX || character.role.roleType == CHARACTER_ROLE.CRATER_BEAST)){
						continue;
					}
					InfectPsytoxin (character);
				}
			}
		}
	}
	private void InfectPsytoxin(ECS.Character character){
		ModeratePsytoxin modPsytoxin = (ModeratePsytoxin)character.GetTag (CHARACTER_TAG.MODERATE_PSYTOXIN);
		if(modPsytoxin != null){
			modPsytoxin.TriggerWorsenCase ();
		}else{
			MildPsytoxin mildPsytoxin = (MildPsytoxin)character.GetTag (CHARACTER_TAG.MILD_PSYTOXIN);
			if(mildPsytoxin != null){
				mildPsytoxin.TriggerWorsenCase ();
			}else{
				int chance = Utilities.rng.Next (0, 100);
				if(chance < 80){
					character.AssignTag (CHARACTER_TAG.MILD_PSYTOXIN);	
				}
			}
		}
	}
}
