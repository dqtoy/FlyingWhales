using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class DouseFire : GoapAction {

    public DouseFire() : base(INTERACTION_TYPE.DOUSE_FIRE) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.WATER_BUCKET.ToString(), target = GOAP_EFFECT_TARGET.ACTOR }, HasWaterBucketInInventory);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Burning", target = GOAP_EFFECT_TARGET.TARGET });
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 10;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Douse Fire Success", goapNode);
    }
    public override void AddFillersToLog(Log log, Character actor, IPointOfInterest poiTarget, object[] otherData, LocationStructure targetStructure) {
        base.AddFillersToLog(log, actor, poiTarget, otherData, targetStructure);
        if (poiTarget is TileObject) {
            if (poiTarget is GenericTileObject) {
                LocationGridTile tile = poiTarget.gridTileLocation;
                log.AddToFillers(null, tile.structure.GetNameRelativeTo(actor) + " floor", LOG_IDENTIFIER.TARGET_CHARACTER);
            } else {
                if (poiTarget == actor) {
                    Character character = poiTarget as Character;
                    log.AddToFillers(poiTarget, Utilities.GetPronounString(character.gender, PRONOUN_TYPE.REFLEXIVE, false), LOG_IDENTIFIER.TARGET_CHARACTER);
                } else {
                    log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                }
            }
        }
    }
    public override GoapActionInvalidity IsInvalid(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        GoapActionInvalidity actionInvalidity = base.IsInvalid(actor, poiTarget, otherData);
        if (actionInvalidity.isInvalid == false) {
            if (poiTarget.traitContainer.GetNormalTrait("Burning") != null && actor.GetToken(SPECIAL_TOKEN.WATER_BUCKET) != null) {
                actionInvalidity.isInvalid = true;
            }
        }
        return actionInvalidity;
    }
    #endregion

    #region State Effects
    private void PreDouseFireSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is TileObject) {
            GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
            if (goapNode.poiTarget is GenericTileObject) {
                LocationGridTile tile = goapNode.poiTarget.gridTileLocation;
                currentState.AddLogFiller(null, tile.structure.GetNameRelativeTo(goapNode.actor) + " floor", LOG_IDENTIFIER.TARGET_CHARACTER);
            } else {
                if (goapNode.poiTarget == goapNode.actor) {
                    Character character = goapNode.poiTarget as Character;
                    currentState.AddLogFiller(goapNode.poiTarget, Utilities.GetPronounString(character.gender, PRONOUN_TYPE.REFLEXIVE, false), LOG_IDENTIFIER.TARGET_CHARACTER);
                } else {
                    currentState.AddLogFiller(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                }
            }
        }
    }
    private void AfterDouseFireSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Burning", removedBy: goapNode.actor);
        SpecialToken water = goapNode.actor.GetToken(SPECIAL_TOKEN.WATER_BUCKET);
        if (water != null) {
            //Reduce water count by 1.
            goapNode.actor.ConsumeToken(water);
        }
        goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Wet", goapNode.actor);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasWaterBucketInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.HasTokenInInventory(SPECIAL_TOKEN.WATER_BUCKET);
    }
    #endregion

}

public class DouseFireData : GoapActionData {
    public DouseFireData() : base(INTERACTION_TYPE.DOUSE_FIRE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.gridTileLocation != null;
    }
}
