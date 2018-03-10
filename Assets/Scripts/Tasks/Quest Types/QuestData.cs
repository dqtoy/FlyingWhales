using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestData {

    public Quest activeQuest;
    public int currentPhase;

    public void SetActiveQuest(Quest quest) {
        activeQuest = quest;
    }

    public void SetQuestPhase(int phase) {
        currentPhase = phase;
    }

    public QuestPhase GetQuestPhase() {
        return activeQuest.phases[currentPhase];
    }
}
