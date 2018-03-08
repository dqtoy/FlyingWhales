using UnityEngine;
using System.Collections;

public class QuestData {

    public Quest activeQuest;
    public int currentPhase;

    public void SetActiveQuest(Quest quest) {
        activeQuest = quest;
    }

    public void SetQuestPhase(int phase) {
        currentPhase = phase;
    }
}
