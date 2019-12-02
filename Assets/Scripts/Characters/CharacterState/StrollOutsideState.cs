using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrollOutsideState : CharacterState {

    private STRUCTURE_TYPE[] _notAllowedStructures;
    //private int _planDuration;

    public StrollOutsideState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Stroll Outside State";
        characterState = CHARACTER_STATE.STROLL_OUTSIDE;
        //stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = GameManager.ticksPerHour;
        //_planDuration = 0;
        _notAllowedStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.INN, STRUCTURE_TYPE.DWELLING, STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.PRISON };
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartStrollMovement();
    }
    //protected override void PerTickInState() {
    //    base.PerTickInState();
        //if (!isDone && !isPaused) {
        //    if(_planDuration >= 4) {
        //        _planDuration = 0;
        //        if (!stateComponent.character.PlanFullnessRecoveryActions(true)) {
        //            if (!stateComponent.character.PlanTirednessRecoveryActions(true)) {
        //                stateComponent.character.PlanHappinessRecoveryActions(true);
        //            }
        //        }
        //    } else {
        //        _planDuration++;
        //    }
        //}
    //}
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        if (stateComponent.character.role.roleType != CHARACTER_ROLE.BEAST && stateComponent.character.race != RACE.SKELETON && targetPOI is SpecialToken) {
            SpecialToken token = targetPOI as SpecialToken;
            if (token.characterOwner == null && token.advertisedActions.Contains(INTERACTION_TYPE.PICK_UP)) {
                stateComponent.character.marker.GoTo(token, () => OnArriveAtPickUpLocation(token));
                //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PICK_UP, stateComponent.character, targetPOI);
                //if (goapAction.targetTile != null) {
                //    SetCurrentlyDoingAction(goapAction);
                //    goapAction.CreateStates();
                //    stateComponent.character.SetCurrentActionNode(goapAction);
                //    stateComponent.character.marker.GoTo(goapAction.targetTile, OnArriveAtPickUpLocation);
                //    PauseState();
                //} else {
                //    Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't pick up item " + targetPOI.name + " because there is no tile to go to!");
                //}
                return true;
            }
        }
        return base.OnEnterVisionWith(targetPOI);
    }
    #endregion

    private void OnArriveAtPickUpLocation(SpecialToken token) {
        if (token.gridTileLocation != null) {
            stateComponent.character.PickUpToken(token);
            StrollAgain();
        }
    }
    private void StrollAgain() {
        ResumeState();
    }

    private void StartStrollMovement() {
        LocationGridTile target = PickRandomTileToGoTo();
        stateComponent.character.marker.GoTo(target, StartStrollMovement, _notAllowedStructures);
        //Debug.Log(stateComponent.character.name + " will stroll to " + target.ToString());
    }
    private LocationGridTile PickRandomTileToGoTo() {
        LocationStructure structure = stateComponent.character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
        LocationGridTile tile = structure.GetRandomTile();
        if(tile != null) {
            return tile;
        } else {
            throw new System.Exception("No unoccupied tile in 3-tile radius for " + stateComponent.character.name + " to go to in " + stateName);
        }
        //List<LocationGridTile> tiles = stateComponent.character.gridTileLocation.parentAreaMap.GetUnoccupiedTilesInRadius(stateComponent.character.gridTileLocation, 4, 3, false, true);
        //if (tiles.Count > 0) {
        //    return tiles[UnityEngine.Random.Range(0, tiles.Count)];
        //} else {
        //    throw new System.Exception("No unoccupied tile in 3-tile radius for " + stateComponent.character.name + " to go to in " + stateName);
        //}
    }
}
