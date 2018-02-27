using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hibernate : CharacterTask {

    private RestAction restAction;
    private List<ECS.Character> _charactersToRest;

	public Hibernate(TaskCreator createdBy, int defaultDaysLeft = -1) : base(createdBy, TASK_TYPE.HIBERNATE, defaultDaysLeft) {
    }

    #region overrides
    public override void OnChooseTask(ECS.Character character) {
        base.OnChooseTask(character);
        //Get the characters that will rest
        _charactersToRest = new List<ECS.Character>();
        if (character.party != null) {
            _charactersToRest.AddRange(character.party.partyMembers);
        } else {
            _charactersToRest.Add(character);
        }
        for (int i = 0; i < _charactersToRest.Count; i++) {
            _charactersToRest[i].AddHistory("Taking a rest.");
        }
    }
    public override void PerformTask() {
        base.PerformTask();
        PerformHibernate();
    }
    //public override void PerformDailyAction() {
    //    if (_canDoDailyAction) {
    //        base.PerformDailyAction();
    //        restAction.DoAction(_assignedCharacter);
    //    }
    //}
    //public override void EndTask(TASK_STATUS taskResult) {
    //    SetCanDoDailyAction(false);
    //    base.EndTask(taskResult);
    //}
    #endregion

    public void PerformHibernate() {
        for (int i = 0; i < _charactersToRest.Count; i++) {
            ECS.Character currCharacter = _charactersToRest[i];
            currCharacter.AdjustHP(currCharacter.raceSetting.restRegenAmount);
        }
    }

    //private void GoToTargetLocation() {
    //    // The monster will move towards its Lair and then rest there indefinitely
    //    GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
    //    goToLocation.InititalizeAction(_assignedCharacter.lair);
    //    goToLocation.SetPathfindingMode(PATHFINDING_MODE.NORMAL);
    //    goToLocation.onTaskActionDone += StartHibernation;
    //    goToLocation.onTaskDoAction += goToLocation.Generic;

    //    goToLocation.DoAction(_assignedCharacter);
    //}

    //private void StartHibernation() {
    //    //SetCanDoDailyAction(true);
    //    restAction = new RestAction(this);
    //    //restAction.onTaskActionDone += TaskSuccess; //rest indefinitely
    //    restAction.onTaskDoAction += restAction.RestIndefinitely;
    //    restAction.DoAction(_assignedCharacter);
    //}

}
