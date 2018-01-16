using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class JoinParty : Quest {

    private Party _partyToJoin;

    #region getters/setters
    public Party partyToJoin {
        get { return _partyToJoin; }
    }
    #endregion

    public JoinParty(QuestCreator createdBy, int daysBeforeDeadline, Party partyToJoin) 
        : base(createdBy, daysBeforeDeadline, QUEST_TYPE.JOIN_PARTY) {
        _questFilters = new List<QuestFilter>() {
            new MustBeRole(CHARACTER_ROLE.ADVENTURER)
        };
        onQuestAccepted += StartQuestLine;
        _partyToJoin = partyToJoin;
    }

    #region overrides
    public override void AcceptQuest(ECS.Character partyLeader) {
        _isAccepted = true;
        partyLeader.SetCurrentQuest(this);
        _partyToJoin.currentQuest.SetWaitingStatus(true); //wait for the character that will join the party
        if (onQuestAccepted != null) {
            onQuestAccepted();
        }
    }
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();

        _partyToJoin.AddPartyMember((ECS.Character)_createdBy); //Add member to party immediately

        GoToLocation goToLocation = new GoToLocation(this);
        goToLocation.InititalizeAction(partyToJoin.partyLeader.currLocation);
        goToLocation.onQuestActionDone += QuestSuccess;
        goToLocation.onQuestDoAction += goToLocation.Generic;

        _questLine.Enqueue(goToLocation);
    }
    internal override void QuestSuccess() {
        _isDone = true;
        _questResult = QUEST_RESULT.SUCCESS;
        _createdBy.RemoveQuest(this);
        _partyToJoin.currentQuest.CheckPartyMembers(); //When the character successfully arrives at the party leaders location, check if all the party members are present
    }
    #endregion
}
