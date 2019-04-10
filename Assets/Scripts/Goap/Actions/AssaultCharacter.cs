using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultCharacter : GoapAction {
    public AssaultCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ASSAULT_ACTION_NPC, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        //List<Character> attackers = new List<Character>();
        //attackers.Add(actor);

        //List<Character> defenders = new List<Character>();
        //defenders.Add(poiTarget as Character);

        //float attackersChance = 0f;
        //float defendersChance = 0f;

        //CombatManager.Instance.GetCombatChanceOfTwoLists(attackers, defenders, out attackersChance, out defendersChance);

        //WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
        //int chance = UnityEngine.Random.Range(0, 100);
        //if (chance < attackersChance) {
        //    //Actor Win
        //    resultWeights.AddElement("Target Injured", 30);
        //    resultWeights.AddElement("Target Knocked Out", 20);
        //    resultWeights.AddElement("Target Killed", 5);
        //} else {
        //    //Target Win
        //    resultWeights.AddElement(Character_Killed_Hunter, 20);
        //    resultWeights.AddElement(Character_Injured_Hunter, 40);
        //}

        //string nextState = resultWeights.PickRandomElementGivenWeights();
        //SetState(nextState);

        if (actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) {
            WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
            resultWeights.AddElement("Target Injured", 30);
            resultWeights.AddElement("Target Knocked Out", 20);
            resultWeights.AddElement("Target Killed", 5);

            string nextState = resultWeights.PickRandomElementGivenWeights();
            if(nextState == "Target Killed") {
                parentPlan.SetDoNotRecalculate(true);
            }
            SetState(nextState);
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 1;
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
        return actor != poiTarget;
    }
    #endregion

    #region State Effects
    public void PreTargetInjured() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        AddTraitTo(actor, "Combat Recovery");
    }
    public void AfterTargetInjured() {
        Character target = poiTarget as Character;
        AddTraitTo(target, "Injured");
    }
    public void PreTargetKnockedOut() {
        //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is an Assault crime
        if (!actor.IsHostileWith(poiTarget as Character)) {
            SetCommittedCrime(CRIME.ASSAULT);
        }
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        AddTraitTo(actor, "Combat Recovery");
    }
    public void AfterTargetKnockedOut() {
        Character target = poiTarget as Character;
        AddTraitTo(target, "Unconscious");
    }
    public void PreTargetKilled() {
        //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is a Murder crime
        if (!actor.IsHostileWith(poiTarget as Character)) {
            SetCommittedCrime(CRIME.MURDER);
        }
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        AddTraitTo(actor, "Combat Recovery");
    }
    public void AfterTargetKilled() {
        Character target = poiTarget as Character;
        target.Death();
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion
}
