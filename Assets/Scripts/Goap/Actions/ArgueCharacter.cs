using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Traits;

public class ArgueCharacter : GoapAction {

    private LocationStructure _targetStructure;
    public override LocationStructure targetStructure {
        get { return _targetStructure; }
    }

    public ArgueCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ARGUE_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Charmed", targetPOI = actor }, ShouldNotBeCharmed);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Annoyed", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            Character target = poiTarget as Character;
            if (IsTargetUnable(target)) {
                SetState("Argue Fail");
            } else {
                SetState("Argue Success");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        int cost = 5; //Base cost: +5
        Character targetCharacter = poiTarget as Character;
        Charmed charmed = actor.traitContainer.GetNormalTrait("Charmed") as Charmed;
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
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    public override void SetTargetStructure() {
        _targetStructure = poiTarget.gridTileLocation.structure;
        base.SetTargetStructure();
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Argue Success") {
            actor.AdjustDoNotGetLonely(-1);
            AddTraitTo(poiTarget, "Annoyed", actor);
        }
    }
    #endregion

    #region Preconditions
    private bool ShouldNotBeCharmed() {
        return actor.traitContainer.GetNormalTrait("Charmed") != null;
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
        //currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void PerTickArgueSuccess() {
        actor.AdjustHappiness(6);
    }
    private void AfterArgueSuccess() {
        actor.AdjustDoNotGetLonely(-1);
        AddTraitTo(poiTarget, "Annoyed", actor);
    }
    //private void PreArgueFail() {
        //currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    //private void PreTargetMissing() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    #endregion

    #region Requirement
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
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
        if (target.traitContainer.GetNormalTrait("Unconscious") != null) {
            return true;
        }
        return false;
    }
    #endregion


}
