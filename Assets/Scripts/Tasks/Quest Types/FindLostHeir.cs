using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class FindLostHeir : Quest {

    private Character _chieftain, _falseHeir, _lostHeir;

	public FindLostHeir(TaskCreator createdBy, Character chieftain, Character falseHeir, Character lostHeir, Item heirloomNecklace) : base(createdBy, QUEST_TYPE.FIND_LOST_HEIR) {
        _alignment = new List<ACTION_ALIGNMENT>() {
            ACTION_ALIGNMENT.LAWFUL,
            ACTION_ALIGNMENT.HEROIC
        };
        _chieftain = chieftain;
        _falseHeir = falseHeir;
        _lostHeir = lostHeir;
        _filters = new TaskFilter[] {
            new MustBeFaction((createdBy as Character).faction),
            new MustNotBeCharacter(falseHeir)
        };
			
        QuestPhase phase1 = new QuestPhase(this, "Search for Heirloom Necklace");
        phase1.AddTask(new Search(createdBy, 5, "Heirloom Necklace", null, this));
		phase1.AddTask(new MoveTowardsCharacter (createdBy, heirloomNecklace, -1, this));
        phase1.AddPhaseRequirement(new MustFindItem("Heirloom Necklace"));

        QuestPhase phase2 = new QuestPhase(this, "Report back to chieftain");
        phase2.AddTask(new Report(createdBy, createdBy as Character, this));
        phase2.AddPhaseRequirement(new MustFinishAllPhaseTasks());

        _phases.Add(phase1);
        _phases.Add(phase2);
    }

    public void OnLostHeirFound() {
        //transfer successor tag from false heir to lost heir
        _falseHeir.RemoveCharacterTag(CHARACTER_TAG.SUCCESSOR);
        Successor successorTag = _lostHeir.AssignTag(CHARACTER_TAG.SUCCESSOR) as Successor;
        successorTag.SetCharacterToSucceed(_chieftain);
        if (_lostHeir.isFactionless) {
            //if the lost heir is factionless set his/her faction to the faction of the cheiftain
            _lostHeir.SetFaction(_chieftain.faction);
        }
    }
}
