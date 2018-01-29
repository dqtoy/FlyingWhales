using UnityEngine;
using System.Collections;
using ECS;

public class DoNothing : Quest {
    public DoNothing(QuestCreator createdBy, int daysBeforeDeadline) 
        : base(createdBy, daysBeforeDeadline, QUEST_TYPE.DO_NOTHING) {
        onQuestAccepted += EndQuestAfterDays;
    }

    private void EndQuestAfterDays() {
        ScheduleQuestEnd(Random.Range(4, 9), QUEST_RESULT.SUCCESS); //Do Nothing should only last for a random number of days between 4 days to 8 days
    }

    #region overrides
    public override void AcceptQuest(ECS.Character partyLeader) {
        _isAccepted = true;
        partyLeader.SetCurrentQuest(this);
        if(partyLeader.party != null) {
            partyLeader.party.SetCurrentQuest(this);
        }
        this.SetWaitingStatus(false);
        if (onQuestAccepted != null) {
            onQuestAccepted();
        }
    }
    internal override void EndQuest(QUEST_RESULT result) {
        if (!_isDone) {
            _questResult = result;
            _isDone = true;
            _createdBy.RemoveQuest(this);
            ((ECS.Character)_createdBy).DetermineAction();
        }
    }
    internal override void QuestCancel() {
        _isDone = true;
        _createdBy.RemoveQuest(this);
        _questResult = QUEST_RESULT.SUCCESS;
    }
    #endregion
}
