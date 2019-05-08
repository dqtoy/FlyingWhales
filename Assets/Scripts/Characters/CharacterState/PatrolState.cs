using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : CharacterState {

    public PatrolState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Patrol State";
        characterState = CHARACTER_STATE.PATROL;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 24;
        actionIconString = GoapActionStateDB.Patrol_Icon;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartPatrolMovement();
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        if(targetPOI is Character) {
            return stateComponent.character.marker.AddHostileInRange(targetPOI as Character);
        }else if (stateComponent.character.role.roleType != CHARACTER_ROLE.BEAST && targetPOI is SpecialToken) {
            SpecialToken token = targetPOI as SpecialToken;
            if(token.characterOwner == null) {
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PICK_ITEM, stateComponent.character, targetPOI);
                if (goapAction.targetTile != null) {
                    goapAction.CreateStates();
                    stateComponent.character.SetCurrentAction(goapAction);
                    stateComponent.character.marker.GoTo(goapAction.targetTile, targetPOI, () => OnArriveAtPickUpLocation());
                    PauseState();
                } else {
                    Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't pick up item " + targetPOI.name + " because there is no tile to go to!");
                }
                return true;
            }
        }
        return base.OnEnterVisionWith(targetPOI);
    }
    protected override void PerTickInState() {
        base.PerTickInState();
        if (!isDone) {
            if(stateComponent.character.GetTrait("Injured") != null) {
                StopStatePerTick();
                OnExitThisState();
            }
        }
    }
    #endregion

    private void OnArriveAtPickUpLocation() {
        stateComponent.character.currentAction.SetEndAction(PatrolAgain);
        stateComponent.character.currentAction.PerformActualAction();
    }
    private void PatrolAgain(string result, GoapAction goapAction) {
        stateComponent.character.SetCurrentAction(null);
        ResumeState();
    }

    private void StartPatrolMovement() {
        stateComponent.character.marker.GoTo(PickRandomTileToGoTo(), stateComponent.character, () => StartPatrolMovement());
    }
    private LocationGridTile PickRandomTileToGoTo() {
        LocationStructure chosenStructure = stateComponent.character.specificLocation.GetRandomStructure();
        LocationGridTile chosenTile = chosenStructure.GetRandomUnoccupiedTile();
        if (chosenTile != null) {
            return chosenTile;
        } else {
            throw new System.Exception("No unoccupied tile in " + chosenStructure.name + " for " + stateComponent.character.name + " to go to in " + stateName);
        }
    }
}
