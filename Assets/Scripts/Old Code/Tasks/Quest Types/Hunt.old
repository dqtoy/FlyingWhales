using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Hunt : Quest {
	private Character _targetCharacter;

	public Hunt(TaskCreator createdBy, Character target) : base(createdBy, QUEST_TYPE.HUNT) {
		_targetCharacter = target;
		_questName = "Hunt " + _targetCharacter.name;
		_questURLName = "Hunt " + _targetCharacter.urlName;

		_alignment = new List<ACTION_ALIGNMENT>() {
			ACTION_ALIGNMENT.LAWFUL,
			ACTION_ALIGNMENT.HEROIC
		};

		QuestPhase phase1 = new QuestPhase(this, "Attack Target!");
		phase1.AddTask(new Attack(createdBy, _targetCharacter, 5, this));

		MoveTowardsCharacter moveTowardsCharacter = new MoveTowardsCharacter (createdBy, null, -1, this);
		moveTowardsCharacter.SetSpecificTarget (_targetCharacter);
		phase1.AddTask(moveTowardsCharacter);

		_phases.Add(phase1);

		_targetCharacter.AddActionOnDeath (ForceCancelQuest);
		_targetCharacter.AddActionOnImprison (ForceCancelQuest);
	}

	#region Overrides
	public override bool CanAcceptQuest (Character character){
		if (base.CanAcceptQuest (character)) {
			if(character.currentRegion.id == _targetCharacter.currentRegion.id || character.currentRegion.adjacentRegionsViaRoad.Contains(_targetCharacter.currentRegion)){
				return true;
			}
		}
		return false;
	}
	#endregion
}
