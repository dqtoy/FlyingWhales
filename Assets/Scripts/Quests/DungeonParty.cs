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
		member.SetParty(null);
	}

	public override bool StartEncounter(Party encounteredBy){
		this.encounteredByParty = encounteredBy;
		ECS.CombatPrototype combat = new ECS.CombatPrototype (this);
		combat.AddCharacters (ECS.SIDES.A, encounteredBy.partyMembers);
		combat.AddCharacters (ECS.SIDES.B, this._partyMembers);
		CombatThreadPool.Instance.AddToThreadPool (combat);
		return false;
	}

	public override void ReturnResults (object result){
		if(result is ECS.CombatPrototype){
			ECS.CombatPrototype combat = (ECS.CombatPrototype)result;
			encounteredByParty.currentQuest.AddNewLogs(combat.resultsLog);
			if(combat.charactersSideA.Count > 0) {
				encounteredByParty.currentQuest.Result (true);
			} else {
				encounteredByParty.currentQuest.Result (false);
			}
		}
	}
}
