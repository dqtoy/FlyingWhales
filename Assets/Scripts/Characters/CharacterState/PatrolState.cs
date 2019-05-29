using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : CharacterState {
    private int _currentDuration;

    public PatrolState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Patrol State";
        characterState = CHARACTER_STATE.PATROL;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 24;
        actionIconString = GoapActionStateDB.Patrol_Icon;
        _currentDuration = 0;
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
                //Patrollers should not pick up items from their warehouse
                if (token.structureLocation != null && token.structureLocation.structureType == STRUCTURE_TYPE.WAREHOUSE 
                    && token.specificLocation == stateComponent.character.homeArea) {
                    return false;
                }
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PICK_ITEM, stateComponent.character, targetPOI);
                if (goapAction.targetTile != null) {
                    SetCurrentlyDoingAction(goapAction);
                    goapAction.CreateStates();
                    stateComponent.character.SetCurrentAction(goapAction);
                    stateComponent.character.marker.GoTo(goapAction.targetTile, OnArriveAtPickUpLocation);
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
                return;
            }
            if (_currentDuration >= 4) {
                _currentDuration = 0;
                if (!stateComponent.character.PlanFullnessRecoveryActions()) {
                    if (!stateComponent.character.PlanTirednessRecoveryActions()) {
                        stateComponent.character.PlanHappinessRecoveryActions();
                    }
                }
            } else {
                _currentDuration++;
            }
        }
    }
    #endregion

    private void OnArriveAtPickUpLocation() {
        if (stateComponent.character.currentAction == null) {
            Debug.LogWarning(GameManager.Instance.TodayLogString() + stateComponent.character.name + " arrived at pick up location of item during " + stateName + ", but current action is null");
            return;
        }
        stateComponent.character.currentAction.SetEndAction(PatrolAgain);
        stateComponent.character.currentAction.PerformActualAction();
    }
    private void PatrolAgain(string result, GoapAction goapAction) {
        SetCurrentlyDoingAction(null);
        if (stateComponent.currentState != this) {
            return;
        }
        stateComponent.character.SetCurrentAction(null);
        ResumeState();
    }

    private void StartPatrolMovement() {
        stateComponent.character.marker.GoTo(PickRandomTileToGoTo(), StartPatrolMovement);
    }
    private LocationGridTile PickRandomTileToGoTo() {
        LocationStructure chosenStructure = stateComponent.character.specificLocation.GetRandomStructure();
        LocationGridTile chosenTile = chosenStructure.GetRandomTile();
        if (chosenTile != null) {
            return chosenTile;
        } else {
            throw new System.Exception("No tile in " + chosenStructure.name + " for " + stateComponent.character.name + " to go to in " + stateName);
        }
    }
}
