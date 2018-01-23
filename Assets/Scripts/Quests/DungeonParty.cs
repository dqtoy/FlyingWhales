using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonParty : Party {

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
	public override void RemovePartyMember(ECS.Character member) {
		_partyMembers.Remove(member);
		if(_avatar != null) {
			_avatar.RemoveCharacter(member);
		}
		member.SetParty(null);
	}
}
