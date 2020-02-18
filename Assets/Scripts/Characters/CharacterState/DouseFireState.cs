using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;
using UnityEngine.Assertions;
using UtilityScripts;

public class DouseFireState : CharacterState {

    private ITraitable currentTarget;
    private BurningSource currentTargetSource;
    public Dictionary<BurningSource, List<ITraitable>> fires;

    private bool isFetchingWater;
    private bool isDousingFire;

    public DouseFireState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Douse Fire State";
        characterState = CHARACTER_STATE.DOUSE_FIRE;
        //stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 0;
        actionIconString = GoapActionStateDB.Drink_Icon;
        fires = new Dictionary<BurningSource, List<ITraitable>>();
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
        for (int i = 0; i < stateComponent.character.currentRegion.innerMap.activeBurningSources.Count; i++) {
            BurningSource burningSource = stateComponent.character.currentRegion.innerMap.activeBurningSources[i];
            for (int j = 0; j < burningSource.objectsOnFire.Count; j++) {
                ITraitable traitable = burningSource.objectsOnFire[j];
                if (traitable is IPointOfInterest) {
                    AddFire(traitable as IPointOfInterest);
                }
            }
        }
        // for (int i = 0; i < stateComponent.character.marker.inVisionPOIs.Count; i++) {
        //     IPointOfInterest poi = stateComponent.character.marker.inVisionPOIs[i];
        //     AddFire(poi);
        // }
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
            if (HasWater() || NeedsWater() == false) {
                //douse nearest fire
                DouseNearestFire();
            } else {
                //get water from pond
                if (GetWater() == false) {
                    stateComponent.ExitCurrentState();
                }
            }
        } else {
            if (stateComponent.character.currentActionNode == null && stateComponent.currentState == this) {
                //no more fire, exit state
                stateComponent.ExitCurrentState();
            }
        }
    }
    // public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
    //     if (AddFire(targetPOI)) {
    //         //DetermineAction();
    //         //return true;
    //     }
    //     return base.OnEnterVisionWith(targetPOI);
    // }
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
    public override void PerTickInState() {
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
            if (fires.ContainsKey(burning.sourceOfBurning) == false) {
                fires.Add(burning.sourceOfBurning, new List<ITraitable>());
            }
            if (!fires[burning.sourceOfBurning].Contains(burning.owner)) {
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
                if (currentTarget == burning.owner && removedBy != stateComponent.character) { 
                    //only redetermine action if burning was removed by something or someone else
                    // currentTarget = null;
                    DetermineAction();
                } else if (currentTargetSource == burning.sourceOfBurning) {
                    // currentTargetSource = null;
                    DetermineAction();
                }
            }
        }
    }
    private bool HasWater() {
        return stateComponent.character.HasItem(TILE_OBJECT_TYPE.WATER_BUCKET);
    }
    private bool NeedsWater() {
        return !stateComponent.character.traitContainer.HasTrait("Elemental Master");
    }
    private bool StillHasFire() {
        return fires.Count > 0;
    }
    private bool GetWater() {
        if (isFetchingWater) {
            return true;
        }
        List<TileObject> targets = stateComponent.character.currentRegion.GetTileObjectsOfType(TILE_OBJECT_TYPE.WATER_WELL);
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
            stateComponent.character.marker.GoTo(nearestWater.gridTileLocation, ObtainWater);
            isFetchingWater = true;
            return true;
        }
        Debug.LogWarning(stateComponent.character.name + " cannot find any sources of water!");
        return false;
    }
    private void ObtainWater() {
        stateComponent.character.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_BUCKET));
        isFetchingWater = false; 
    }
    private void DouseNearestFire() {
        if (isDousingFire) {
            return;
        }
        ITraitable nearestFire = currentTarget;
        float nearest = 99999f;
        if (nearestFire != null) {
            nearest = Vector2.Distance(stateComponent.character.worldObject.transform.position, currentTarget.worldObject.transform.position);
        }
        
        if (currentTargetSource == null || fires.ContainsKey(currentTargetSource) == false) {
            currentTargetSource = fires.Keys.First();
        }
        
        Assert.IsNotNull(currentTargetSource, $"{stateComponent.character.name} is trying to douse the nearest " +
                                              $"fire, but could not get a target source. Fire dictionary is \n{CollectionUtilities.GetDictionaryLog(fires)}");
        
        for (int i = 0; i < fires[currentTargetSource].Count; i++) {
            ITraitable currFire = fires[currentTargetSource][i];
            float dist = Vector2.Distance(stateComponent.character.worldObject.transform.position, currFire.worldObject.transform.position);
            if (dist < nearest) {
                nearestFire = currFire;
                nearest = dist;
            }
        }
        if (nearestFire != null) {//&& nearestFire != currentTarget
            isDousingFire = true;
            currentTarget = nearestFire;
            stateComponent.character.marker.GoTo(nearestFire, DouseFire);
        } 
        //else if (nearestFire == null) {
        //    DetermineAction();
        //}
    }
    private void DouseFire() {
        currentTarget.traitContainer.RemoveTrait(currentTarget, "Burning", removedBy: this.stateComponent.character);
        if (NeedsWater()) {
            TileObject water = this.stateComponent.character.GetItem(TILE_OBJECT_TYPE.WATER_BUCKET);
            if (water != null) {
                //Reduce water count by 1.
                this.stateComponent.character.UnobtainItem(water);
            }
            currentTarget.traitContainer.AddTrait(currentTarget, "Wet", this.stateComponent.character);    
        }
        isDousingFire = false;
        currentTarget = null;
    }
    private bool AddFire(IPointOfInterest poi) {
        Burning burning = poi.traitContainer.GetNormalTrait<Burning>("Burning");
        if (burning != null) {
            if (!fires.ContainsKey(burning.sourceOfBurning)) {
                fires.Add(burning.sourceOfBurning, new List<ITraitable>());
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
        foreach (KeyValuePair<BurningSource, List<ITraitable>> kvp in dfState.fires) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                ITraitable poi = kvp.Value[i];
                //TODO: fires[count] = new POIData(poi);
                count++;
            }
        }
    }
}
#endregion