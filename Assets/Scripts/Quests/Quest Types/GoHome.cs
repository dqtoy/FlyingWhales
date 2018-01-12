using UnityEngine;
using System.Collections;

public class GoHome : Quest {
    public GoHome(QuestCreator createdBy, int daysBeforeDeadline, int maxPartyMembers) 
        : base(createdBy, daysBeforeDeadline, maxPartyMembers, QUEST_TYPE.GO_HOME) {
        onQuestAccepted += StartQuestLine;
    }

    #region overrides
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();

        GoToLocation goToLocation = new GoToLocation(); //Make character go to chosen settlement
        goToLocation.InititalizeAction(((Character)_createdBy).home);
        goToLocation.onQuestActionDone += QuestSuccess;

        _questLine.Enqueue(goToLocation);
    }
    #endregion
}
