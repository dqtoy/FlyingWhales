using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Rest : Quest {
    public Rest(QuestCreator createdBy, int daysBeforeDeadline) 
        : base(createdBy, daysBeforeDeadline, QUEST_TYPE.REST) {
        onQuestAccepted += StartQuestLine;
    }

    private Settlement GetTargetSettlement() {
        ECS.Character character = (ECS.Character)_createdBy;
        List<Settlement> factionSettlements = character.faction.settlements.OrderBy(x => Vector2.Distance(character.currLocation.transform.position, x.location.transform.position)).ToList();
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

        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
        goToLocation.InititalizeAction(targetSettlement.location);
        goToLocation.onQuestActionDone += this.PerformNextQuestAction;
        goToLocation.onQuestDoAction += goToLocation.Generic;

        RestAction restAction = new RestAction(this);
        restAction.onQuestActionDone += QuestSuccess;
        restAction.onQuestDoAction += restAction.StartDailyRegeneration;

        _questLine.Enqueue(goToLocation);
        _questLine.Enqueue(restAction);

    }
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
    internal override void QuestSuccess() {
        Debug.Log(((ECS.Character)_createdBy).name + " and party has finished resting on " + Utilities.GetDateString(GameManager.Instance.Today()));
        _isDone = true;
        _createdBy.RemoveQuest(this);
        ((ECS.Character)_createdBy).DetermineAction();
    }
    internal override void QuestCancel() {
        _questResult = QUEST_RESULT.CANCEL;
        if (_currentAction != null) {
            _currentAction.ActionDone(QUEST_ACTION_RESULT.CANCEL);
        }
        //TODO: What to do when rest is cancelled?
        //RetaskParty(_assignedParty.partyLeader.OnReachNonHostileSettlementAfterQuest);
        //ResetQuestValues();
    }
    #endregion
}
