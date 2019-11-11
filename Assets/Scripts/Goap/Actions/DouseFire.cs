using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DouseFire : GoapAction {

    private LocationGridTile tile;

    public DouseFire(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DOUSE_FIRE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
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
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.WATER_BUCKET.ToString(), targetPOI = actor }, () => actor.HasTokenInInventory(SPECIAL_TOKEN.WATER_BUCKET));
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Burning", targetPOI = poiTarget });
    }
    protected override int GetBaseCost() {
        return 10;
    }
    protected override void MoveToDoAction(Character targetCharacter) {
        base.MoveToDoAction(targetCharacter);
        tile = poiTarget.gridTileLocation;
    }
    public override void Perform() {
        base.Perform();
        if (poiTarget.gridTileLocation != null && poiTarget.GetNormalTrait("Burning") != null && actor.GetToken(SPECIAL_TOKEN.WATER_BUCKET) != null) {
            SetState("Douse Fire Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override void AddFillersToLog(Log log) {
        base.AddFillersToLog(log);
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
    #endregion

    #region State Effects
    private void PreDouseFireSuccess() {
        if (poiTarget is TileObject) {
            if (poiTarget is GenericTileObject) {
                LocationGridTile tile = poiTarget.gridTileLocation;
                currentState.AddLogFiller(null, tile.structure.GetNameRelativeTo(actor) + " floor", LOG_IDENTIFIER.TARGET_CHARACTER);
            } else {
                if (poiTarget == actor) {
                    Character character = poiTarget as Character;
                    currentState.AddLogFiller(poiTarget, Utilities.GetPronounString(character.gender, PRONOUN_TYPE.REFLEXIVE, false), LOG_IDENTIFIER.TARGET_CHARACTER);
                } else {
                    currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                }
            }
        }
    }
    private void AfterDouseFireSuccess() {
        poiTarget.RemoveTrait("Burning", removedBy: actor);
        SpecialToken water = actor.GetToken(SPECIAL_TOKEN.WATER_BUCKET);
        if (water != null) {
            //Reduce water count by 1.
            actor.ConsumeToken(water);
        }
        poiTarget.AddTrait("Wet", actor);
    }
    //private void PreTargetMissing() {
    //    if (poiTarget is TileObject) {
    //        if (poiTarget is GenericTileObject) {
    //            currentState.AddLogFiller(null, tile.structure.GetNameRelativeTo(actor) + " floor", LOG_IDENTIFIER.TARGET_CHARACTER);
    //        } else {
    //            currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //        }
    //    }
    //}
    #endregion

    #region Requirements
    private bool Requirement() {
        return poiTarget.gridTileLocation != null;
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
