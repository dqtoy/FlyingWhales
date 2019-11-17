using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Nap : GoapAction {

    public Nap() : base(INTERACTION_TYPE.NAP) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Nap Success", goapNode);
    }
    public override GoapActionInvalidity IsInvalid(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(actor, poiTarget, otherData);
        if (goapActionInvalidity.isInvalid == false) {
            if (CanSleepInBed(actor, poiTarget as TileObject) == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.logKey = "nap fail_description";
            } else if (poiTarget.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        LocationStructure targetStructure = target.gridTileLocation.structure;
        if(targetStructure.structureType == STRUCTURE_TYPE.DWELLING) {
            Dwelling dwelling = targetStructure as Dwelling;
            if (dwelling.IsResident(actor)) {
                return 8;
            } else {
                for (int i = 0; i < dwelling.residents.Count; i++) {
                    Character resident = dwelling.residents[i];
                    if (resident != actor) {
                        IRelationshipData characterRelationshipData = actor.relationshipContainer.GetRelationshipDataWith(resident);
                        if (characterRelationshipData != null) {
                            if (characterRelationshipData.relationshipStatus == RELATIONSHIP_EFFECT.POSITIVE) {
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
    public override void OnStopWhilePerforming(Character actor, IPointOfInterest target, object[] otherData) {
        base.OnStopWhilePerforming(actor, target, otherData);
        actor.traitContainer.RemoveTrait(actor, "Resting");
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }

            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreNapSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
    }
    private void PerTickNapSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustTiredness(30);

        //TODO:
        //if(_restingTrait.lycanthropyTrait == null) {
        //    if (currentState.currentDuration == currentState.duration) {
        //        //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
        //        bool isTargettedByDrinkBlood = false;
        //        for (int i = 0; i < actor.targettedByAction.Count; i++) {
        //            if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
        //                isTargettedByDrinkBlood = true;
        //                break;
        //            }
        //        }
        //        if (isTargettedByDrinkBlood) {
        //            currentState.OverrideDuration(currentState.duration + 1);
        //        }
        //    }
        //} else {
        //    bool isTargettedByDrinkBlood = false;
        //    for (int i = 0; i < actor.targettedByAction.Count; i++) {
        //        if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
        //            isTargettedByDrinkBlood = true;
        //            break;
        //        }
        //    }
        //    if (currentState.currentDuration == currentState.duration) {
        //        //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
        //        if (isTargettedByDrinkBlood) {
        //            currentState.OverrideDuration(currentState.duration + 1);
        //        } else {
        //            if (!_restingTrait.hasTransformed) {
        //                _restingTrait.CheckForLycanthropy(true);
        //            }
        //        }
        //    } else {
        //        if (!isTargettedByDrinkBlood) {
        //            _restingTrait.CheckForLycanthropy();
        //        }
        //    }
        //}
        
    }
    private void AfterNapSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
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
                RELATIONSHIP_EFFECT relEffect = character.relationshipContainer.GetRelationshipEffectWith(tileObject.users[i]);
                if (relEffect == RELATIONSHIP_EFFECT.NEGATIVE || relEffect == RELATIONSHIP_EFFECT.NONE) {
                    return false;
                }
            }
        }
        return true;
    }
}

public class NapData : GoapActionData {
    public NapData() : base(INTERACTION_TYPE.NAP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}