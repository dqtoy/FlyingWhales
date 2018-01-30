using UnityEngine;
using System.Collections;

public class MoveTo : Quest {
    private HexTile _targetTile;
    private PATHFINDING_MODE _pathfindingMode;

    #region getters/setters
    public HexTile targetTile {
        get { return _targetTile; }
    }
    #endregion

    public MoveTo(QuestCreator createdBy, int daysBeforeDeadline, HexTile targetTile, PATHFINDING_MODE pathFindingMode) 
        : base(createdBy, daysBeforeDeadline, QUEST_TYPE.MOVE_TO) {
        _targetTile = targetTile;
        _pathfindingMode = pathFindingMode;
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
        goToLocation.InititalizeAction(_targetTile);
        goToLocation.SetPathfindingMode(_pathfindingMode);
        goToLocation.onQuestActionDone += SuccessQuest;
        goToLocation.onQuestDoAction += goToLocation.Generic;

        _questLine.Enqueue(goToLocation);
    }
    internal override void QuestSuccess() {
        if (_assignedParty == null) {
            //When the character has gone home, determine the next action
            ECS.Character character = ((ECS.Character)_createdBy);
            character.DestroyAvatar();
            character.DetermineAction();
        } else {
            RetaskParty(_assignedParty.OnReachNonHostileSettlementAfterQuest);
        }
    }
    #endregion

    private void SuccessQuest() {
        EndQuest(QUEST_RESULT.SUCCESS);
    }
}
