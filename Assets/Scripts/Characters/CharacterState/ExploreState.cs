using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploreState : CharacterState {

    public List<SpecialToken> itemsCollected { get; private set; } //the list of items collected while in this state

    public bool hasStateStarted { get; private set; }

    public ExploreState(CharacterStateComponent characterComp) : base (characterComp) {
        stateName = "Explore State";
        characterState = CHARACTER_STATE.EXPLORE;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 36;
        hasStateStarted = false;
        itemsCollected = new List<SpecialToken>();
        actionIconString = GoapActionStateDB.Explore_Icon;
    }

    #region Overrides
    protected override void StartState() {
        base.StartState();
        hasStateStarted = true;
        stateComponent.character.SetLastExploreState(null);
    }

    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartExploreMovement();
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            return stateComponent.character.marker.AddHostileInRange(targetPOI as Character);
        } else if (stateComponent.character.role.roleType != CHARACTER_ROLE.BEAST && targetPOI is SpecialToken) {
            SpecialToken token = targetPOI as SpecialToken;
            if (token.characterOwner == null) {
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
        }else if (targetPOI is Character) {
            stateComponent.character.marker.AddHostileInRange(targetPOI as Character);
            return true;
        }
        return base.OnEnterVisionWith(targetPOI);
    }
    public override void OnExitThisState() {
        base.OnExitThisState();
        stateComponent.character.SetLastExploreState(this);
        if (!stateComponent.character.isDead) {
            //Force deposit items in character home location warehouse
            //Stroll goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.DROP_ITEM, this, this) as Stroll;
            //goapAction.SetTargetStructure(targetStructure);
            //GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
            //GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
            //allGoapPlans.Add(goapPlan);
        }
    }
    protected override void PerTickInState() {
        base.PerTickInState();
        if (!isDone) {
            if (stateComponent.character.GetTrait("Injured") != null) {
                StopStatePerTick();
                OnExitThisState();
            }
        }
    }
    #endregion

    private void OnArriveAtPickUpLocation() {
        if (stateComponent.character.currentAction == null) {
            Debug.LogWarning(GameManager.Instance.TodayLogString() + stateComponent.character.name + " arrived at pick up location of item during " + stateName + ", but current action is null");
            return;
        }
        stateComponent.character.currentAction.SetEndAction(ExploreAgain);
        stateComponent.character.currentAction.PerformActualAction();
    }
    private void ExploreAgain(string result, GoapAction goapAction) {
        SetCurrentlyDoingAction(null);
        if (result == InteractionManager.Goap_State_Success && goapAction.poiTarget is SpecialToken) {
            itemsCollected.Add(goapAction.poiTarget as SpecialToken);
        }
        if (stateComponent.currentState != this) {
            return;
        }
        stateComponent.character.SetCurrentAction(null);
        ResumeState();
    }
    private void StartExploreMovement() {
        stateComponent.character.marker.GoTo(PickRandomTileToGoTo(), StartExploreMovement);
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

    public void CreateDeliverTreasureJob() {
        GoapPlanJob job = new GoapPlanJob("Deliver Treasure", INTERACTION_TYPE.DROP_ITEM_WAREHOUSE);
        job.SetPlanConstructor(DeliverTreasureConstructor);
        stateComponent.character.jobQueue.AddJobInQueue(job);
    }
    private GoapPlan DeliverTreasureConstructor() {
        GoapNode previousNode = null;
        GoapNode startNode = null;
        for (int i = 0; i < itemsCollected.Count; i++) {
            SpecialToken item = itemsCollected[i];
            GoapAction currAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.DROP_ITEM_WAREHOUSE, stateComponent.character, stateComponent.character);
            currAction.InitializeOtherData(new object[] { item.specialTokenType });
            GoapNode currNode = new GoapNode(previousNode, currAction.cost, currAction);
            if (i + 1 == itemsCollected.Count) {
                startNode = currNode;
            }
            previousNode = currNode;
        }
        GoapPlan goapPlan = new GoapPlan(startNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.WORK);
        goapPlan.ConstructAllNodes();
        return goapPlan;
    }
}
