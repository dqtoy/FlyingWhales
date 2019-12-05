﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class DropResource : GoapAction {
    public DropResource() : base(INTERACTION_TYPE.DROP_RESOURCE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.IN_PARTY, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), IsResourcePileCarried);
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
        //AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_FOOD, GOAP_EFFECT_TARGET.TARGET));
        //AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_WOOD, GOAP_EFFECT_TARGET.TARGET));
        //AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_STONE, GOAP_EFFECT_TARGET.TARGET));
        //AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_METAL, GOAP_EFFECT_TARGET.TARGET));
    }
    //protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, object[] otherData) {
    //    List<GoapEffect> ee = base.GetExpectedEffects(actor, target, otherData);
    //    ResourcePile pile = target as ResourcePile;
    //    switch (pile.providedResource) {
    //        case RESOURCE.FOOD:
    //            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_FOOD, "0", true, GOAP_EFFECT_TARGET.TARGET));
    //            break;
    //        case RESOURCE.WOOD:
    //            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_WOOD, "0", true, GOAP_EFFECT_TARGET.TARGET));
    //            break;
    //        case RESOURCE.STONE:
    //            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_STONE, "0", true, GOAP_EFFECT_TARGET.TARGET));
    //            break;
    //        case RESOURCE.METAL:
    //            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_METAL, "0", true, GOAP_EFFECT_TARGET.TARGET));
    //            break;
    //    }
    //    return ee;
    //}
    //public override List<Precondition> GetPreconditions(IPointOfInterest target, object[] otherData) {
    //    List<Precondition> p = base.GetPreconditions(target, otherData);
    //    ResourcePile pile = target as ResourcePile;
    //    p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.IN_PARTY, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), IsResourcePileCarried));

    //    //switch (pile.providedResource) {
    //    //    case RESOURCE.FOOD:
    //    //        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_FOOD, "0", true, GOAP_EFFECT_TARGET.ACTOR), IsActorFoodEnough));
    //    //        break;
    //    //    case RESOURCE.WOOD:
    //    //        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_WOOD, "0", true, GOAP_EFFECT_TARGET.ACTOR), IsActorWoodEnough));
    //    //        break;
    //    //    case RESOURCE.STONE:
    //    //        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_STONE, "0", true, GOAP_EFFECT_TARGET.ACTOR), IsActorWoodEnough));
    //    //        break;
    //    //    case RESOURCE.METAL:
    //    //        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_METAL, "0", true, GOAP_EFFECT_TARGET.ACTOR), IsActorWoodEnough));
    //    //        break;
    //    //}
    //    return p;
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drop Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 3;
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        object[] otherData = node.otherData;
        if (otherData != null && otherData.Length == 1 && otherData[0] is IPointOfInterest) {
            return (otherData[0] as IPointOfInterest).gridTileLocation.structure;
        } else {
            return node.actor.specificLocation.mainStorage;
        }
        //return base.GetTargetStructure(node);
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        object[] otherData = goapNode.otherData;
        if (otherData != null && otherData.Length == 1 && otherData[0] is IPointOfInterest) {
            return otherData[0] as IPointOfInterest;
        }
        return null;
    }
    public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        List<LocationGridTile> unoccupiedTiles = goapNode.actor.specificLocation.mainStorage.unoccupiedTiles;
        return unoccupiedTiles[UnityEngine.Random.Range(0, unoccupiedTiles.Count)];
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.ownParty.RemovePOI(poiTarget);
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.ownParty.RemovePOI(poiTarget);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsTargetMissingOverride(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName);
        if (defaultTargetMissing == false) {
            //check the target's traits, if any of them can make this action invalid
            for (int i = 0; i < poiTarget.traitContainer.allTraits.Count; i++) {
                Trait trait = poiTarget.traitContainer.allTraits[i];
                if (trait.TryStopAction(goapType, actor, poiTarget, ref goapActionInvalidity)) {
                    break; //a trait made this action invalid, stop loop
                }
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region Preconditions
    private bool IsActorWoodEnough(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor.supply > actor.role.reservedSupply) {
            WoodPile supplyPile = poiTarget as WoodPile;
            int supplyToBeDeposited = actor.supply - actor.role.reservedSupply;
            if((supplyToBeDeposited + supplyPile.resourceInPile) >= 100) {
                return true;
            }
        }
        return false;
    }
    private bool IsActorFoodEnough(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.food > 0;
    }
    private bool IsResourcePileCarried(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.ownParty.IsPOICarried(poiTarget);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (actor.ownParty.IsPOICarried(poiTarget)) {
                return true;
            }
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            //Cannot be deposited if already in the storage
            LocationStructure structure = poiTarget.gridTileLocation.structure;
            if (structure == structure.location.mainStorage) {
                return false;
            }
            if (structure.location.mainStorage.unoccupiedTiles.Count <= 0) {
                return false;
            }
            return actor.homeArea == structure.location;
        }
        return false;
    }
    #endregion

    #region State Effects
    //public void PreDropSuccess(ActualGoapNode goapNode) {
    //    //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
    //    int givenSupply = goapNode.actor.supply - goapNode.actor.role.reservedSupply;
    //    //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //    goapNode.descriptionLog.AddToFillers(null, givenSupply.ToString(), LOG_IDENTIFIER.STRING_1);
    //}
    public void AfterDropSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        ResourcePile poiTarget = goapNode.poiTarget as ResourcePile;
        object[] otherData = goapNode.otherData;
        ResourcePile poiToBeDeposited = null;
        if (otherData != null && otherData.Length == 1 && otherData[0] is ResourcePile) {
            poiToBeDeposited = otherData[0] as ResourcePile;
        }
        if(poiToBeDeposited.gridTileLocation == goapNode.targetTile) {
            //Deposit resource pile
            if(poiToBeDeposited.resourceInPile < LandmarkManager.MAX_RESOURCE_PILE) {
                poiToBeDeposited.AdjustResourceInPile(poiTarget.resourceInPile);
                actor.ownParty.RemovePOI(poiTarget, false);
            } else {
                actor.ownParty.RemovePOI(poiTarget);
            }
        } else {
            actor.ownParty.RemovePOI(poiTarget);
        }
    }
    #endregion


    private bool IsTargetMissingOverride(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (actor.ownParty.IsPOICarried(poiTarget)) {
            return false;
        }
        if (poiTarget.IsAvailable() == false || poiTarget.gridTileLocation == null || actor.specificLocation != poiTarget.specificLocation) {
            return true;
        }
        if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != poiTarget.gridTileLocation && actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation) == false) {
                return true;
            }
        } else if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != node.targetTile && actor.gridTileLocation.IsNeighbour(node.targetTile) == false) {
                return true;
            }
        }
        return false;
    }
}

public class DropResourceData : GoapActionData {
    public DropResourceData() : base(INTERACTION_TYPE.DROP_RESOURCE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        return actor.homeArea == poiTarget.gridTileLocation.structure.location;
    }
}
