using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Expand : Quest {

	private HexTile _targetUnoccupiedTile;

	public Expand(QuestCreator createdBy, int daysBeforeDeadline, int maxPartyMembers, HexTile targetUnoccupiedTile) 
		: base(createdBy, daysBeforeDeadline, maxPartyMembers, QUEST_TYPE.EXPAND) {
		_questFilters = new List<QuestFilter>() {
			new MustBeFaction(new List<Faction>(){((ECS.Character)createdBy).faction})
		};
		_targetUnoccupiedTile = targetUnoccupiedTile;
	}
}
