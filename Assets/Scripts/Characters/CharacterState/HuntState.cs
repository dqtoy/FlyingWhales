using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntState : CharacterState {

    public HuntState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Hunt State";
        characterState = CHARACTER_STATE.HUNT;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 48;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartHuntMovement();
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character targetCharacter = targetPOI as Character;
            if (targetCharacter.isDead) {
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.EAT_CORPSE, stateComponent.character, targetPOI);
                if (goapAction.targetTile != null) {
                    SetCurrentlyDoingAction(goapAction);
                    goapAction.CreateStates();
                    stateComponent.character.SetCurrentAction(goapAction);
                    stateComponent.character.marker.GoTo(goapAction.targetTile, OnArriveAtCorpseLocation);
                } else {
                    Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't eat corpse " + targetPOI.name + " because there is no tile to go to!");
                }
            } else {
                stateComponent.character.marker.AddHostileInRange(targetCharacter);
            }
           
            return true;
        }
        //else if (targetPOI is Corpse) {
        //    GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.EAT_CORPSE, stateComponent.character, targetPOI);
        //    if (goapAction.targetTile != null) {
        //        goapAction.CreateStates();
        //        stateComponent.character.SetCurrentAction(goapAction);
        //        stateComponent.character.marker.GoTo(goapAction.targetTile, targetPOI, () => OnArriveAtCorpseLocation());
        //    } else {
        //        Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't eat corpse " + targetPOI.name + " because there is no tile to go to!");
        //    }
        //    return true;
        //}
        return base.OnEnterVisionWith(targetPOI);
    }
    #endregion

    private void OnArriveAtCorpseLocation() {
        stateComponent.character.currentAction.SetEndAction(HuntAgain);
        stateComponent.character.currentAction.PerformActualAction();
    }
    private void HuntAgain(string result, GoapAction goapAction) {
        SetCurrentlyDoingAction(null);
        if (stateComponent.currentState != this) {
            return;
        }
        stateComponent.character.SetCurrentAction(null);
        StartHuntMovement();
    }
    private void StartHuntMovement() {
        stateComponent.character.marker.GoTo(PickRandomTileToGoTo(), StartHuntMovement);
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
