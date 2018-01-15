using UnityEngine;
using System.Collections;

public class DoNothing : Quest {
    public DoNothing(QuestCreator createdBy, int daysBeforeDeadline, int maxPartyMembers) 
        : base(createdBy, daysBeforeDeadline, maxPartyMembers, QUEST_TYPE.DO_NOTHING) {
        onQuestAccepted += EndQuestAfterDays;
    }

    private void EndQuestAfterDays() {
        ScheduleQuestEnd(10, QUEST_RESULT.SUCCESS);
    }
}
