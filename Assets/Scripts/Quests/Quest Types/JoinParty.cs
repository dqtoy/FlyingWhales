using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JoinParty : Quest {

    private Party _partyToJoin;

    #region getters/setters
    public Party partyToJoin {
        get { return _partyToJoin; }
    }
    #endregion

    public JoinParty(QuestCreator createdBy, int daysBeforeDeadline, int maxPartyMembers, Party partyToJoin) 
        : base(createdBy, daysBeforeDeadline, maxPartyMembers, QUEST_TYPE.JOIN_PARTY) {
        _questFilters = new List<QuestFilter>() {
            new MustBeRole(CHARACTER_ROLE.ADVENTURER)
        };
        onQuestAccepted += StartQuestLine;
        _partyToJoin = partyToJoin;
    }

    #region overrides
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();

        GoToLocation goToLocation = new GoToLocation(this);
        goToLocation.InititalizeAction(partyToJoin.partyLeader.currLocation);
        goToLocation.onQuestActionDone += QuestSuccess;
        goToLocation.onQuestDoAction += goToLocation.Generic;

        _questLine.Enqueue(goToLocation);
    }
    #endregion
}
