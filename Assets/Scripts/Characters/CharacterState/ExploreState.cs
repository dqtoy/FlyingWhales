using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploreState : CharacterState {

    public ExploreState(CharacterStateComponent characterComp) : base (characterComp) {
        stateName = "Explore State";
        characterState = CHARACTER_STATE.EXPLORE;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 288;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartExploreMovement();
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        if(targetPOI is SpecialToken) {
            GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PICK_ITEM, stateComponent.character, targetPOI);
            if(goapAction.targetTile != null) {
                goapAction.CreateStates();
                stateComponent.character.SetCurrentAction(goapAction);
                stateComponent.character.marker.GoToTile(goapAction.targetTile, targetPOI, () => OnArriveAtPickUpLocation());
            } else {
                Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't pick up item " + targetPOI.name + " because there is no tile to go to!");
            }
            return true;
        }else if (targetPOI is Character) {
            stateComponent.character.marker.AddHostileInRange(targetPOI as Character);
            return true;
        }
        return base.OnEnterVisionWith(targetPOI);
    }
    public override void OnExitThisState() {
        base.OnExitThisState();
        if (!stateComponent.character.isDead) {
            //Force deposit items in character home location warehouse
            //Stroll goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.DROP_ITEM, this, this) as Stroll;
            //goapAction.SetTargetStructure(targetStructure);
            //GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
            //GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
            //allGoapPlans.Add(goapPlan);
        }
    }
    #endregion

    private void OnArriveAtPickUpLocation() {
        stateComponent.character.currentAction.SetEndAction(ExploreAgain);
        stateComponent.character.currentAction.PerformActualAction();
    }
    private void ExploreAgain(string result, GoapAction goapAction) {
        StartExploreMovement();
    }
    private void StartExploreMovement() {
        stateComponent.character.marker.GoToTile(PickRandomTileToGoTo(), stateComponent.character, () => StartExploreMovement());
    }
    private LocationGridTile PickRandomTileToGoTo() {
        LocationStructure chosenStructure = stateComponent.character.specificLocation.GetRandomStructure();
        LocationGridTile chosenTile = chosenStructure.GetRandomUnoccupiedTile();
        if(chosenTile != null) {
            return chosenTile;
        } else {
            throw new System.Exception("No unoccupied tile in " + chosenStructure.name + " for " + stateComponent.character.name + " to go to in " + stateName);
        }
    }
}
