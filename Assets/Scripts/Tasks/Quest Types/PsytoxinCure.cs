using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class PsytoxinCure : Quest {
	//TODO: Go back to phase when an item has been stolen or dropped
	private BaseLandmark _craterLandmark;

	public PsytoxinCure(TaskCreator createdBy, BaseLandmark crater) : base(createdBy, QUEST_TYPE.THE_DARK_RITUAL) {
		_alignment = new List<ACTION_ALIGNMENT>() {
			ACTION_ALIGNMENT.PEACEFUL,
		};
		_craterLandmark = crater;

		QuestPhase phase1 = new QuestPhase(this, "Collect Meteorite");
		phase1.AddTask(new Collect(createdBy, "Meteorite", 1, _craterLandmark, -1, this));

		QuestPhase phase2 = new QuestPhase(this, "Search for a Neuroctus");
		phase2.AddTask(new Search(createdBy, 5, "Neuroctus", null, this));

		QuestPhase phase3 = new QuestPhase(this, "Search for an Herbalist");
		phase3.AddTask(new Search(createdBy, 5, "Psytoxin Herbalist", null, this));

		_phases.Add(phase1);
		_phases.Add(phase2);
		_phases.Add(phase3);
	}

	#region Overrides

	#endregion
}
