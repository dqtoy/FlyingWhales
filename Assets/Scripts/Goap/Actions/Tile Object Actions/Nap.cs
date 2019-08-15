using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nap : GoapAction {
    protected override string failActionState { get { return "Nap Fail"; } }

    private Resting _restingTrait;
    public Nap(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.NAP, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            if (CanSleepInBed(actor, poiTarget as TileObject)) {
                SetState("Nap Success");
            } else {
                SetState("Nap Fail");
            }
        } else {
            TileObject obj = poiTarget as TileObject;
            if (!obj.IsAvailable()) {
                SetState("Nap Fail");
            } else {
                SetState("Target Missing");
            }
        }
    }
    protected override int GetCost() {
        if(targetStructure.structureType == STRUCTURE_TYPE.DWELLING) {
            Dwelling dwelling = targetStructure as Dwelling;
            if (dwelling.IsResident(actor)) {
                return 8;
            } else {
                for (int i = 0; i < dwelling.residents.Count; i++) {
                    Character resident = dwelling.residents[i];
                    if (resident != actor) {
                        CharacterRelationshipData characterRelationshipData = actor.GetCharacterRelationshipData(resident);
                        if (characterRelationshipData != null) {
                            if (characterRelationshipData.HasRelationshipOfEffect(TRAIT_EFFECT.POSITIVE)) {
                                return 25;
                            }
                        }
                    }
                }
                return 45;
            }
        } else if(targetStructure.structureType == STRUCTURE_TYPE.INN) {
            return 45;
        }
        return 100;
    }
    public override void OnStopActionDuringCurrentState() {
        if(currentState.name == "Nap Success") {
            RemoveTraitFrom(actor, "Resting");
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        //if (targetStructure.structureType == STRUCTURE_TYPE.DWELLING && knownLoc != null) {
        //    TileObject obj = poiTarget as TileObject;
        //    return obj.IsAvailable();
        //}
        //return false;

        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
    #endregion

    #region State Effects
    private void PreNapSuccess() {
        //currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        _restingTrait = new Resting();
        actor.AddTrait(_restingTrait);
    }
    private void PerTickNapSuccess() {
        actor.AdjustTiredness(4);

        if(_restingTrait.lycanthropyTrait == null) {
            if (currentState.currentDuration == currentState.duration) {
                //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
                bool isTargettedByDrinkBlood = false;
                for (int i = 0; i < actor.targettedByAction.Count; i++) {
                    if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
                        isTargettedByDrinkBlood = true;
                        break;
                    }
                }
                if (isTargettedByDrinkBlood) {
                    currentState.OverrideDuration(currentState.duration + 1);
                }
            }
        } else {
            bool isTargettedByDrinkBlood = false;
            for (int i = 0; i < actor.targettedByAction.Count; i++) {
                if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
                    isTargettedByDrinkBlood = true;
                    break;
                }
            }
            if (currentState.currentDuration == currentState.duration) {
                //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
                if (isTargettedByDrinkBlood) {
                    currentState.OverrideDuration(currentState.duration + 1);
                } else {
                    if (!_restingTrait.hasTransformed) {
                        _restingTrait.CheckForLycanthropy(true);
                    }
                }
            } else {
                if (!isTargettedByDrinkBlood) {
                    _restingTrait.CheckForLycanthropy();
                }
            }
        }
        
    }
    private void AfterNapSuccess() {
        RemoveTraitFrom(actor, "Resting");
    }
    //private void PreNapFail() {
    //    currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //private void PreNapMissing() {
    //    currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    #endregion

    private bool CanSleepInBed(Character character, TileObject tileObject) {
        for (int i = 0; i < tileObject.users.Length; i++) {
            if (tileObject.users[i] != null) {
                RELATIONSHIP_EFFECT relEffect = character.GetRelationshipEffectWith(tileObject.users[i]);
                if (relEffect == RELATIONSHIP_EFFECT.NEGATIVE || relEffect == RELATIONSHIP_EFFECT.NONE) {
                    return false;
                }
            }
        }
        return true;
    }
}