using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonParty : Party {

	Party encounteredByParty;

	public DungeonParty(ECS.Character partyLeader, bool mustBeAddedToPartyList = true): base (partyLeader, mustBeAddedToPartyList) {
		
	}

	public override void AddPartyMember(ECS.Character member) {
		if (!_partyMembers.Contains(member)) {
			_partyMembers.Add(member);
			member.SetParty(this);
			this._currLocation.RemoveCharacterOnTile (member);
		}
	}
	/*
     Remove a character from this party.
         */
	public override void RemovePartyMember(ECS.Character member, bool forDeath = false) {
		_partyMembers.Remove(member);
		if(_avatar != null) {
			_avatar.RemoveCharacter(member);
		}
		if(!forDeath){
			member.currLocation.AddCharacterOnTile(member, false);
		}
		member.SetParty(null);
	}

	public override void StartEncounter(Party encounteredBy){
		this.encounteredByParty = encounteredBy;
		ECS.CombatPrototype combat = new ECS.CombatPrototype (this, this.currLocation);
		combat.AddCharacters (ECS.SIDES.A, encounteredBy.partyMembers);
		combat.AddCharacters (ECS.SIDES.B, this._partyMembers);
		CombatThreadPool.Instance.AddToThreadPool (combat);
	}

	public override void ReturnResults (object result){
		if(result is ECS.CombatPrototype){
			ECS.CombatPrototype combat = (ECS.CombatPrototype)result;
			encounteredByParty.currentTask.AddNewLogs(combat.resultsLog);
			for (int i = 0; i < encounteredByParty.partyMembers.Count; i++) {
				encounteredByParty.partyMembers [i].AddHistory ("Encountered " + this._name + ".", combat);
			}
			if(combat.charactersSideA.Count > 0) {
				((Quest)encounteredByParty.currentTask).Result (true);
			} else {
				((Quest)encounteredByParty.currentTask).Result (false);
			}
		}
	}
}
