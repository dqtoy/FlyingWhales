﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArgueCharacter : GoapAction {

    private LocationStructure _targetStructure;
    public override LocationStructure targetStructure {
        get { return _targetStructure; }
    }

    public ArgueCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ARGUE_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Social_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Charmed", targetPOI = actor }, ShouldNotBeCharmed);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.ADD_TRAIT, conditionKey = "Annoyed", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) {
            Character target = poiTarget as Character;
            if (IsTargetUnable(target)) {
                SetState("Argue Fail");
            } else {
                SetState("Argue Success");
            }
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        int cost = 5; //Base cost: +5
        Character targetCharacter = poiTarget as Character;
        Charmed charmed = actor.GetTrait("Charmed") as Charmed;
        if (charmed != null && charmed.responsibleCharacter != null && charmed.responsibleCharacter.id == targetCharacter.id) {
            //If Actor is charmed and target is the charmer: +10
            cost += 10; 
        }
        if (actor.faction.id != targetCharacter.faction.id) {
            //Target is not from the same faction: -1
            cost -= 1; 
        }
        if (actor.race != targetCharacter.race) {
            //Target is a different race: -1
            cost -= 1;
        }
        CharacterRelationshipData relData = actor.GetCharacterRelationshipData(targetCharacter);
        if (relData != null) {
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.ENEMY)) {
                //Target is an Enemy: -2
                cost -= 2;
            }
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.LOVER)) {
                //Target is a Lover: +2
                cost += 2;
            }
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.RELATIVE)) {
                //Target is a Relative: +2
                cost += 2;
            }
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.FRIEND)) {
                //Target is a Friend: +3
                cost += 3;
            }
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.PARAMOUR)) {
                //Target is a Paramour: +3
                cost += 3;
            }
        }
        cost += Utilities.rng.Next(0, 4); //Then add a random cost between 0 to 4.
        return cost;
    }
    public override void DoAction(GoapPlan plan) {
        SetTargetStructure();
        base.DoAction(plan);
    }
    public override void SetTargetStructure() {
        //TODO: Change to known location when plan data has been set up
        _targetStructure = poiTarget.gridTileLocation.structure;
        base.SetTargetStructure();
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    #endregion

    #region Preconditions
    private bool ShouldNotBeCharmed() {
        return actor.GetTrait("Charmed") != null;
    }
    #endregion

    #region State Effects
    private void PreArgueSuccess() {
        actor.AdjustDoNotGetLonely(1);
        //Character target = poiTarget as Character;
        //if (target.currentParty.icon.isTravelling && target.currentParty.icon.travelLine == null) {
        //    target.SetCurrentAction(null);
        //    target.marker.StopMovement();
        //}
        //if (target.marker.isStillMovingToAnotherTile) {
        //    target.marker.SetOnArriveAtTileAction(() => target.FaceTarget(actor));
        //} else {
        //    target.FaceTarget(actor);
        //}
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void PerTickArgueSuccess() {
        actor.AdjustHappiness(6);
    }
    private void AfterArgueSuccess() {
        actor.AdjustDoNotGetLonely(-1);
        (poiTarget as Character).AddTrait("Annoyed");
    }
    private void PreArgueFail() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        if (actor != poiTarget) {
            Character target = poiTarget as Character;
            if (IsTargetUnable(target)) return false;
            return target.role.roleType != CHARACTER_ROLE.BEAST;
        }
        return false;
    }
    private bool IsTargetUnable(Character target) {
        if (target.currentAction != null
                && (target.currentAction.goapType == INTERACTION_TYPE.SLEEP || target.currentAction.goapType == INTERACTION_TYPE.SLEEP_OUTSIDE)) {
            return true;
        }
        if (target.GetTrait("Unconscious") != null) {
            return true;
        }
        return false;
    }
    #endregion


}
