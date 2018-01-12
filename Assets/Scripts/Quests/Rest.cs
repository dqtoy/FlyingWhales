using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Rest : Quest {
    public Rest(QuestCreator createdBy, int daysBeforeDeadline, int maxPartyMembers) 
        : base(createdBy, daysBeforeDeadline, maxPartyMembers, QUEST_TYPE.REST) {
        onQuestAccepted += StartQuestLine;
    }

    private Settlement GetTargetSettlement() {
        Character character = (Character)_createdBy;
        List<Settlement> factionSettlements = character._faction.settlements.OrderBy(x => Vector2.Distance(character.currLocation.transform.position, x.location.transform.position)).ToList();
        for (int i = 0; i < factionSettlements.Count; i++) {
            Settlement currSettlement = factionSettlements[i];
            if(PathGenerator.Instance.GetPath(character.currLocation, currSettlement.location, PATHFINDING_MODE.USE_ROADS) != null) {
                return currSettlement;
            }
        }
        return null;
    }

    #region overrides
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();
        Settlement targetSettlement = GetTargetSettlement();

        GoToLocation goToLocation = new GoToLocation(); //Make character go to chosen settlement
        goToLocation.InititalizeAction(targetSettlement.location);
        goToLocation.onQuestActionDone += this.PerformNextQuestAction;

        RestForDays restForDays = new RestForDays();
        restForDays.InititalizeAction(30); //set character to rest for x days
        restForDays.onQuestActionDone += QuestSuccess;

    }
    #endregion
}
