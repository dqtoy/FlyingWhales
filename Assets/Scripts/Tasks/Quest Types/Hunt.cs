using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Hunt : Quest {
	private Character _targetCharacter;

	public Hunt(TaskCreator createdBy, Character target) : base(createdBy, QUEST_TYPE.HUNT) {
		_questName = "Hunt " + target.name;
		_targetCharacter = target;

		_alignment = new List<ACTION_ALIGNMENT>() {
			ACTION_ALIGNMENT.LAWFUL,
			ACTION_ALIGNMENT.HEROIC
		};

		QuestPhase phase1 = new QuestPhase(this, "Attack Target!");
		phase1.AddTask(new Attack(createdBy, _targetCharacter, 5, this));

		MoveTo moveTo = new MoveTo (createdBy, -1, this);
		moveTo.SetForGameOnly (true);
		phase1.AddTask(moveTo);

		_phases.Add(phase1);

		_targetCharacter.AddActionOnDeath (ForceCancelQuest);
		_targetCharacter.AddActionOnImprison (ForceCancelQuest);
	}
}
