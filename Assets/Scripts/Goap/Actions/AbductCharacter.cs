using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbductCharacter : GoapAction {
    private int _numOfTries;
    public AbductCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ABDUCT_ACTION, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        _numOfTries = 0;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = poiTarget }, HasNonPositiveDisablerTrait);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        if (!isTargetMissing) {
            if (!HasOtherCharacterInRadius()) {
                SetState("Abduct Success");
            } else {
                parentPlan.SetDoNotRecalculate(true);
                SetState("Target Missing");
            }
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 3;
    }
    public override bool IsHalted() {
        if (_numOfTries < 18 && HasOtherCharacterInRadius()) {
            _numOfTries++;
            return true;
        }
        return base.IsHalted();
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    public override void DoAction(GoapPlan plan) {
        SetTargetStructure();
        base.DoAction(plan);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if(actor != poiTarget) {
            Character target = poiTarget as Character;
            return target.GetTrait("Abducted") == null;
        }
        return false;
    }
    #endregion

    //#region Preconditions
    //private bool HasNonPositiveDisablerTrait() {
    //    Character target = poiTarget as Character;
    //    return target.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER);
    //}
    //#endregion

    #region State Effects
    //public void PreAbductSuccess() {
        //currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterAbductSuccess() {
        Character target = poiTarget as Character;
        Restrained restrainedTrait = new Restrained();
        target.AddTrait(restrainedTrait, actor);

        AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
    }
    //public void PreTargetMissing() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Special Note: Stealth
    public bool HasOtherCharacterInRadius() {
        List<LocationGridTile> tiles = (poiTarget as Character).currentStructure.location.areaMap.GetTilesInRadius(poiTarget.gridTileLocation, 3);
        for (int i = 0; i < tiles.Count; i++) {
            //if (tiles[i].occupant != null && tiles[i].occupant != actor && tiles[i].occupant != poiTarget) {
            //    return true;
            //}
            LocationGridTile currTile = tiles[i];
            List<Character> characters = new List<Character>(currTile.charactersHere);
            characters.Remove(actor);
            characters.Remove(poiTarget as Character);
            for (int j = 0; j < characters.Count; j++) {
                Character currCharacter = characters[j];
                //check each character, if a character has a disabler trait, do not consider them as a character that will notice this action
                if (!currCharacter.HasTraitOf(TRAIT_TYPE.DISABLER)) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion
}