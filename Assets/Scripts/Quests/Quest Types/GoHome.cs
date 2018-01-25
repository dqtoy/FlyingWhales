using UnityEngine;
using System.Collections;

public class GoHome : Quest {
    public GoHome(QuestCreator createdBy, int daysBeforeDeadline) 
        : base(createdBy, daysBeforeDeadline, QUEST_TYPE.GO_HOME) {
        onQuestAccepted += StartQuestLine;
    }

    #region overrides
	public override void AcceptQuest(ECS.Character partyLeader) {
		_isAccepted = true;
		partyLeader.SetCurrentQuest(this);
		if (partyLeader.party != null) {
			partyLeader.party.SetCurrentQuest(this);
		}
		this.SetWaitingStatus(false);
		if (onQuestAccepted != null) {
			onQuestAccepted();
		}
	}
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();

        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
        goToLocation.InititalizeAction(((ECS.Character)_createdBy).home.location);
		goToLocation.onQuestActionDone += SuccessQuest;
        goToLocation.onQuestDoAction += goToLocation.Generic;

        _questLine.Enqueue(goToLocation);
    }
    internal override void QuestSuccess() {
        if(_assignedParty == null) {
            //When the character has gone home, determine the next action
            ECS.Character character = ((ECS.Character)_createdBy);
            character.DestroyAvatar();
            character.DetermineAction();
        } else {
            RetaskParty(_assignedParty.OnReachNonHostileSettlementAfterQuest);
        }
    }
    #endregion

	private void SuccessQuest(){
		EndQuest (QUEST_RESULT.SUCCESS);
	}
}
