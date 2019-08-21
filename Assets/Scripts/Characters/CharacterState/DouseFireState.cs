using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DouseFireState : CharacterState {

    //private List<IPointOfInterest> objsOnFire;
    //public object sourceOfBurning { get; private set; } //only put out fires from this source.
    private IPointOfInterest currentTarget;
    private object currentTargetSource;
    private Dictionary<object, List<IPointOfInterest>> fires;

    private bool isFetchingWater;

    public DouseFireState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Douse Fire State";
        characterState = CHARACTER_STATE.DOUSE_FIRE;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 0;
        actionIconString = GoapActionStateDB.Drink_Icon;
        fires = new Dictionary<object, List<IPointOfInterest>>();
    }

    #region Overrides
    protected override void StartState() {
        //add initial objects on fire
        for (int i = 0; i < stateComponent.character.marker.inVisionPOIs.Count; i++) {
            IPointOfInterest poi = stateComponent.character.marker.inVisionPOIs[i];
            Burning burningTrait = poi.GetNormalTrait("Burning") as Burning;
            if (burningTrait != null) {
                if (!fires.ContainsKey(burningTrait.sourceOfBurning)) {
                    fires.Add(burningTrait.sourceOfBurning, new List<IPointOfInterest>());
                }
                fires[burningTrait.sourceOfBurning].Add(burningTrait.owner);
            }
        }
        Burning characterBurning = stateComponent.character.GetNormalTrait("Burning") as Burning;
        if (characterBurning != null) {
            fires[characterBurning.sourceOfBurning].Add(characterBurning.owner);
        }
        base.StartState();
        Messenger.AddListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
    }
    protected override void EndState() {
        base.EndState();
        Messenger.RemoveListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
    }
    private void DetermineAction() {
        if (StillHasFire()) {
            if (HasWater()) {
                //douse nearest fire
                DouseNearestFire();
            } else {
                //get water from pond
                GetWater();
            }
        } else {
            //no more fire, exit state
            OnExitThisState();
        }
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        Burning burning = targetPOI.GetNormalTrait("Burning") as Burning;
        if (burning != null) {
            if (!fires.ContainsKey(burning.sourceOfBurning)) {
                fires.Add(burning.sourceOfBurning, new List<IPointOfInterest>());
            }
            if (!fires[burning.sourceOfBurning].Contains(targetPOI)) {
                fires[burning.sourceOfBurning].Add(targetPOI);
            }
            DetermineAction();
            return true;
        }
        return base.OnEnterVisionWith(targetPOI);
    }
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        DetermineAction();
    }
    public override void PauseState() {
        base.PauseState();
        if (isFetchingWater) {
            isFetchingWater = false;
        }
    }
    protected override void PerTickInState() {
        if (!StillHasFire() && stateComponent.character.currentAction == null && stateComponent.currentState == this) {
            //if there is no longer any fire, and the character is still trying to douse fire, exit this state
            OnExitThisState();
        }
    }
    #endregion

    #region Utilities
    private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
        if (trait is Burning) {
            Burning burning = trait as Burning;
            if (fires.ContainsKey(burning.sourceOfBurning) && !fires[burning.sourceOfBurning].Contains(burning.owner)) {
                fires[burning.sourceOfBurning].Add(burning.owner);
            }
        }
    }
    private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
        if (trait is Burning) {
            Burning burning = trait as Burning;
            if (fires.ContainsKey(burning.sourceOfBurning) && fires[burning.sourceOfBurning].Contains(burning.owner)) {
                fires[burning.sourceOfBurning].Remove(burning.owner);
                if (fires[burning.sourceOfBurning].Count == 0) {
                    fires.Remove(burning.sourceOfBurning);
                    if (currentTargetSource == burning.sourceOfBurning) {
                        currentTargetSource = null;
                    }
                }

            }
        }
    }
    private bool HasWater() {
        return stateComponent.character.GetToken(SPECIAL_TOKEN.WATER_BUCKET) != null;
    }
    private bool StillHasFire() {
        return fires.Count > 0;
    }
    private void GetWater() {
        if (isFetchingWater) {
            return;
        }
        List<TileObject> targets = stateComponent.character.specificLocation.GetTileObjectsThatAdvertise(INTERACTION_TYPE.GET_WATER);
        TileObject nearestWater = null;
        float nearestDist = 9999f;
        for (int i = 0; i < targets.Count; i++) {
            TileObject currObj = targets[i];
            float dist = Vector2.Distance(stateComponent.character.gridTileLocation.localLocation, currObj.gridTileLocation.localLocation);
            if (dist < nearestDist) {
                nearestWater = currObj;
                nearestDist = dist;
            }
        }

        if (nearestWater != null) {
            GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.GET_WATER, stateComponent.character, nearestWater);
            GoapNode node = new GoapNode(null, goapAction.cost, goapAction);
            GoapPlan plan = new GoapPlan(node, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.REMOVE_TRAIT }, GOAP_CATEGORY.REACTION);
            plan.ConstructAllNodes();
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_FIRE, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Burning", targetPOI = nearestWater });
            job.SetAssignedPlan(plan);
            SetCurrentlyDoingAction(goapAction);
            goapAction.CreateStates();
            stateComponent.character.marker.GoTo(goapAction.targetTile, () => OnArriveAtWaterLocation(goapAction));
            PauseState();
            isFetchingWater = true;
        } else {
            throw new System.Exception(stateComponent.character.name + " cannot find any sources of water!");
        }
    }
    private void OnGetWaterFromPond(string result, GoapAction action) {
        SetCurrentlyDoingAction(null);
        if (stateComponent.currentState != this) {
            return;
        }
        stateComponent.character.SetCurrentAction(null);
        ResumeState();
        isFetchingWater = false;
    }
    private void OnArriveAtWaterLocation(GoapAction goapAction) {
        stateComponent.character.SetCurrentAction(goapAction);
        stateComponent.character.currentAction.SetEndAction(OnGetWaterFromPond);
        stateComponent.character.currentAction.PerformActualAction();
    }
    private void DouseNearestFire() {
        IPointOfInterest nearestFire = null;
        float nearest = 99999f;

        if (currentTargetSource == null) {
            currentTargetSource = fires.Keys.First();
        }

        for (int i = 0; i < fires[currentTargetSource].Count; i++) {
            IPointOfInterest currFire = fires[currentTargetSource][i];
            float dist = Vector2.Distance(stateComponent.character.gridTileLocation.localLocation, currFire.gridTileLocation.localLocation);
            if (dist < nearest) {
                nearestFire = currFire;
                nearest = dist;
            }
        }
        if (nearestFire != null && nearestFire != currentTarget) {
            GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.DOUSE_FIRE, stateComponent.character, nearestFire);
            GoapNode node = new GoapNode(null, goapAction.cost, goapAction);
            GoapPlan plan = new GoapPlan(node, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.REMOVE_TRAIT }, GOAP_CATEGORY.REACTION);
            plan.ConstructAllNodes();
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_FIRE, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Burning", targetPOI = nearestFire });
            job.SetAssignedPlan(plan);
            SetCurrentlyDoingAction(goapAction);
            goapAction.CreateStates();
            PauseState();
            currentTarget = nearestFire;
            stateComponent.character.marker.GoTo(nearestFire, () => OnArriveAtDouseFireLocaton(goapAction));
        } else if (nearestFire == null) {
            DetermineAction();
        }
    }
    private void OnArriveAtDouseFireLocaton(GoapAction goapAction) {
        //if (stateComponent.character.currentAction == null) {
        //    OnDouseFire(InteractionManager.Goap_State_Fail, null); //ususally happens when character collides with ghost collider of fire and the douse fire action is automatically ended.
        //    return;
        //}
        stateComponent.character.SetCurrentAction(goapAction);
        stateComponent.character.currentAction.SetEndAction(OnDouseFire);
        stateComponent.character.currentAction.PerformActualAction();
    }
    private void OnDouseFire(string result, GoapAction action) {
        SetCurrentlyDoingAction(null);
        if (stateComponent.currentState != this) {
            return;
        }
        stateComponent.character.SetCurrentAction(null);
        //objsOnFire.Remove(action.poiTarget);
        ResumeState();
    }
    #endregion
}
