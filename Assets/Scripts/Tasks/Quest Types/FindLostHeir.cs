using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FindLostHeir : Quest {
    public FindLostHeir(TaskCreator createdBy) : base(createdBy, QUEST_TYPE.FIND_LOST_HEIR) {
        _alignment = new List<QUEST_ALIGNMENT>() {
            QUEST_ALIGNMENT.LAWFUL,
            QUEST_ALIGNMENT.HEROIC
        };
        QuestPhase phase1 = new QuestPhase(this, "Search for Heirloom Necklace");
        phase1.AddTask(new Search(createdBy, 5, "Heirloom Necklace", null));
        _phases.Add(phase1);
    }
}
