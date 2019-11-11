using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Traits;

public class DouseFireState : CharacterState {

    private IPointOfInterest currentTarget;
    private BurningSource currentTargetSource;
    public Dictionary<BurningSource, List<IPointOfInterest>> fires;

    private bool isFetchingWater;
    private bool isDousingFire;

    public DouseFireState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Douse Fire State";
        characterState = CHARACTER_STATE.DOUSE_FIRE;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 0;
        actionIconString = GoapActionStateDB.Drink_Icon;
        fires = new Dictionary<BurningSource, List<IPointOfInterest>>();
    }

    #region Overrides
    public override void Load(SaveDataCharacterState saveData) {
        base.Load(saveData);
        DouseFireStateSaveDataCharacterState dfSaveData = saveData as DouseFireStateSaveDataCharacterState;
        for (int i = 0; i < dfSaveData.fires.Length; i++) {
            POIData poiData = dfSaveData.fires[i];
            IPointOfInterest poi = SaveUtilities.GetPOIFromData(poiData);
            if (poi == null) {
                throw new System.Exception("No POI found: " + poiData.ToString());
            }
            AddFire(poi);
        }
    }
    protected override void StartState() {
        //add initial objects on fire
        for (int i = 0; i < stateComponent.character.marker.inVisionPOIs.Count; i++) {
            IPointOfInterest poi = stateComponent.character.marker.inVisionPOIs[i];
            AddFire(poi);
        }
        AddFire(stateComponent.character);
        base.StartState();
        Messenger.AddListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
    }
    protected override void EndState() {
        base.EndState();
        Messenger.RemoveListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
    }
    public void DetermineAction() {
        if (StillHasFire()) {
            if (HasWater()) {
                //douse nearest fire
                DouseNearestFire();
            } else {
                //get water from pond
                GetWater();
            }
        } else {
            if (stateComponent.character.currentAction == null && stateComponent.currentState == this) {
                //no more fire, exit state
                OnExitThisState();
            }
        }
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        if (AddFire(targetPOI)) {
            //DetermineAction();
            //return true;
        }
        return base.OnEnterVisionWith(targetPOI);
    }
    //protected override void DoMovementBehavior() {
    //    base.DoMovementBehavior();
    //    DetermineAction();
    //}
    public override void PauseState() {
        base.PauseState();
        if (isFetchingWater) {
            isFetchingWater = false;
        }
        if (isDousingFire) {
            isDousingFire = false;
        }
        //currentTarget = null;
    }
    protected override void PerTickInState() {
        DetermineAction();
        //if (!StillHasFire() && stateComponent.character.currentAction == null && stateComponent.currentState == this) {
        //    //if there is no longer any fire, and the character is still trying to douse fire, exit this state
        //    OnExitThisState();
        //}else if (StillHasFire()) {
        //    DetermineAction();
        //}
    }
    #endregion

    #region Utilities
    public void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
        if (trait is Burning) {
            Burning burning = trait as Burning;
            if (fires.ContainsKey(burning.sourceOfBurning) && !fires[burning.sourceOfBurning].Contains(burning.owner)) {
                fires[burning.sourceOfBurning].Add(burning.owner);
                //if (currentTarget != null) {
                //    //check the distance of the new fire with the current target, if the current target is still nearer, continue dousing that, if not redetermine action
                //    float originalDistance = Vector2.Distance(stateComponent.character.gridTileLocation.localLocation, currentTarget.gridTileLocation.localLocation);
                //    float newDistance = Vector2.Distance(stateComponent.character.gridTileLocation.localLocation, burning.owner.gridTileLocation.localLocation);
                //    if (newDistance < originalDistance) {
                //        DetermineAction();
                //    }
                //} else {
                //    DetermineAction();
                //}
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
                }
                //if (currentTarget == burning.owner) {
                //    currentTarget = null;
                //    DetermineAction();
                //} else if (currentTargetSource == burning.sourceOfBurning) {
                //    currentTargetSource = null;
                //    if (fires.Count == 0) {//no more fires left
                //        DetermineAction();
                //    }
                //}
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
            Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DID_ACTION, OnAfterActionDone);
            //stateComponent.character.marker.GoTo(goapAction.targetTile, () => OnArriveAtWaterLocation(goapAction));
            PauseState();
            isFetchingWater = true;
            goapAction.DoAction();
        } else {
            throw new System.Exception(stateComponent.character.name + " cannot find any sources of water!");
        }
    }
    private void OnAfterActionDone(Character characterThatDidAction, GoapAction actionDone) {
        if(characterThatDidAction == stateComponent.character) {
            if(actionDone.goapType == INTERACTION_TYPE.GET_WATER) {
                Messenger.RemoveListener<Character, GoapAction>(Signals.CHARACTER_DID_ACTION, OnAfterActionDone);
                OnGetWaterFromPond(actionDone.result, actionDone);
            }else if (actionDone.goapType == INTERACTION_TYPE.DOUSE_FIRE) {
                Messenger.RemoveListener<Character, GoapAction>(Signals.CHARACTER_DID_ACTION, OnAfterActionDone);
                OnDouseFire(actionDone.result, actionDone);
            }
        }
    }
    private void OnGetWaterFromPond(string result, GoapAction action) {
        SetCurrentlyDoingAction(null);
        if (stateComponent.currentState != this) {
            return;
        }
        stateComponent.character.SetCurrentAction(null);
        isFetchingWater = false;
        ResumeState();
    }
    private void OnArriveAtWaterLocation(GoapAction goapAction) {
        stateComponent.character.SetCurrentAction(goapAction);
        stateComponent.character.currentAction.SetEndAction(OnGetWaterFromPond);
        stateComponent.character.currentAction.PerformActualAction();
    }
    private void DouseNearestFire() {
        if (isDousingFire) {
            return;
        }
        IPointOfInterest nearestFire = currentTarget;
        float nearest = 99999f;
        if (nearestFire != null) {
            nearest = Vector2.Distance(stateComponent.character.gridTileLocation.localLocation, currentTarget.gridTileLocation.localLocation);
        }
        
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
            Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DID_ACTION, OnAfterActionDone);
            PauseState();
            isDousingFire = true;
            goapAction.DoAction();
            //currentTarget = nearestFire;
            //stateComponent.character.marker.GoTo(nearestFire, () => OnArriveAtDouseFireLocaton(goapAction));
        } 
        //else if (nearestFire == null) {
        //    DetermineAction();
        //}
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
        //THIS IS QUICK FIX ONLY!
        if(result == InteractionManager.Goap_State_Fail) {
            //When douse fire fails, it means that the target is no longer burning or doesn't have grid tile location or actor no longer has water
            //When this happens, be sure to remove it from the list because it causes an UNENDING LOOP!
            foreach (List<IPointOfInterest> listOfBurning in fires.Values) {
                listOfBurning.Remove(action.poiTarget);
            }
        }
        isDousingFire = false;
        //objsOnFire.Remove(action.poiTarget);
        ResumeState();
    }
    private bool AddFire(IPointOfInterest poi) {
        Burning burning = poi.traitContainer.GetNormalTrait("Burning") as Burning;
        if (burning != null) {
            if (!fires.ContainsKey(burning.sourceOfBurning)) {
                fires.Add(burning.sourceOfBurning, new List<IPointOfInterest>());
            }
            if (!fires[burning.sourceOfBurning].Contains(poi)) {
                fires[burning.sourceOfBurning].Add(poi);
                return true;
            }
        }
        return false;
    }
    #endregion
}

#region Save Data
[System.Serializable]
public class DouseFireStateSaveDataCharacterState : SaveDataCharacterState {

    public POIData[] fires;

    public override void Save(CharacterState state) {
        base.Save(state);
        DouseFireState dfState = state as DouseFireState;
        fires = new POIData[dfState.fires.Sum(x => x.Value.Count)];
        int count = 0;
        foreach (KeyValuePair<BurningSource, List<IPointOfInterest>> kvp in dfState.fires) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                IPointOfInterest poi = kvp.Value[i];
                fires[count] = new POIData(poi);
                count++;
            }
        }
    }
}
#endregion