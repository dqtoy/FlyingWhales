using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class StrollState : CharacterState {
    private int _planDuration;

    public StrollState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Stroll State";
        characterState = CHARACTER_STATE.STROLL;
        //stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = GameManager.ticksPerHour;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartStrollMovement();
    }
    public override void PerTickInState() {
        base.PerTickInState();
        if (_planDuration >= 4) {
            _planDuration = 0;
            if (!stateComponent.character.PlanFullnessRecoveryActions()) {
                if (!stateComponent.character.PlanTirednessRecoveryActions()) {
                    stateComponent.character.PlanHappinessRecoveryActions();
                }
            }
        } else {
            _planDuration++;
        }
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        if (stateComponent.character.faction.isMajorFriendlyNeutral && stateComponent.character.role.roleType != CHARACTER_ROLE.BEAST && targetPOI is SpecialToken) {
            SpecialToken token = targetPOI as SpecialToken;
            if (token.characterOwner == null) {

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
        stateComponent.character.marker.GoTo(target, StartStrollMovement);
        //Debug.Log(stateComponent.character.name + " will stroll to " + target.ToString());
    }
    private LocationGridTile PickRandomTileToGoTo() {
        List<LocationGridTile> tiles = stateComponent.character.gridTileLocation.parentMap.GetUnoccupiedTilesInRadius(stateComponent.character.gridTileLocation, 4, 3, false, true);
        if (tiles.Count > 0) {
            return tiles[UnityEngine.Random.Range(0, tiles.Count)];
        } else {
            throw new System.Exception("No unoccupied tile in 3-tile radius for " + stateComponent.character.name + " to go to in " + stateName);
        }
        //int multiplier = 1;//UnityEngine.Random.Range(5, 8);
        //Vector3 forwardPos = stateComponent.character.marker.visualsParent.up * multiplier;
        //Vector2Int forwardPosInt = new Vector2Int((int) forwardPos.x, (int) forwardPos.y);
        //LocationGridTile chosenTile = stateComponent.character.GetLocationGridTileByXY(forwardPosInt.x, forwardPosInt.y, false);
        //if (chosenTile != null && chosenTile.structure != null) {
        //    return chosenTile;
        //} else {
        //    List<LocationGridTile> tiles = stateComponent.character.gridTileLocation.parentAreaMap.GetUnoccupiedTilesInRadius(stateComponent.character.gridTileLocation, 3, false, true);
        //    if (tiles.Count > 0) {
        //        return tiles[UnityEngine.Random.Range(0, tiles.Count)];
        //    } else {
        //        throw new System.Exception("No unoccupied tile in 3-tile radius for " + stateComponent.character.name + " to go to in " + stateName);
        //    }
        //}
    }
}
