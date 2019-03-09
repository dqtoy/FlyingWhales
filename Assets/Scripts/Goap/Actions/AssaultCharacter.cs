using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultCharacter : GoapAction {
    public AssaultCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ASSAULT_ACTION_NPC, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
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

        WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
        resultWeights.AddElement("Target Injured", 30);
        resultWeights.AddElement("Target Knocked Out", 20);
        resultWeights.AddElement("Target Killed", 5);

        string nextState = resultWeights.PickRandomElementGivenWeights();
        SetState(nextState);
    }
    protected override int GetCost() {
        return 1;
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
    }
    public void AfterTargetInjured() {
        Character target = poiTarget as Character;
        target.AddTrait("Injured");
    }
    public void PreTargetKnockedOut() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterTargetKnockedOut() {
        Character target = poiTarget as Character;
        target.AddTrait("Unconscious");
    }
    public void PreTargetKilled() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterTargetKilled() {
        Character target = poiTarget as Character;
        target.Death();
    }
    #endregion
}
