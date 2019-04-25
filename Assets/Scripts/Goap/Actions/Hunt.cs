using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunt : GoapAction {
    public Hunt(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.HUNT_ACTION, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Eat_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = actor });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        Character target = poiTarget as Character;
        if (target.isDead) {
            SetState("Target Missing");
        } else {
            if (!isTargetCharacterMissing && actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) {
                List<Character> attackers = new List<Character>();
                attackers.Add(actor);

                List<Character> defenders = new List<Character>();
                defenders.Add(poiTarget as Character);

                float attackersChance = 0f;
                float defendersChance = 0f;

                CombatManager.Instance.GetCombatChanceOfTwoLists(attackers, defenders, out attackersChance, out defendersChance);

                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < attackersChance) {
                    //Actor Win
                    WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
                    resultWeights.AddElement("Target Injured", 30);
                    resultWeights.AddElement("Target Killed", 15);
                    string nextState = resultWeights.PickRandomElementGivenWeights();
                    if (nextState == "Target Killed") {
                        parentPlan.SetDoNotRecalculate(true);
                    }
                    SetState(nextState);
                } else {
                    //Target Win
                    SetState("Target Won");
                }
            } else {
                SetState("Target Missing");
            }
        }
    }
    protected override int GetCost() {
        Character target = poiTarget as Character;
        int cost = 0;
        if(target.race == RACE.GOBLIN) {
            cost = 9;
        } else if (target.race == RACE.HUMANS) {
            cost = 9;
        } else if (target.race == RACE.FAERY) {
            cost = 7;
        } else if (target.race == RACE.ELVES) {
            cost = 7;
        } else if (target.race == RACE.WOLF) {
            cost = 5;
        } else if (target.race == RACE.SPIDER) {
            cost = 11;
        } else if (target.race == RACE.DRAGON) {
            cost = 14;
        } else if (target.race == RACE.SKELETON) {
            cost = 25;
        } else if (target.race == RACE.ABOMINATION) {
            cost = 25;
        }
        if(target.role.roleType == CHARACTER_ROLE.SOLDIER || target.role.roleType == CHARACTER_ROLE.ADVENTURER) {
            cost += 3;
        }
        if(actor.race == target.race) {
            cost += 20;
        }
        return cost;
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
            if(actor.specificLocation == target.specificLocation && actor.faction == FactionManager.Instance.neutralFaction && target.race != RACE.SKELETON 
                && actor.GetTrait("Injured") == null) { //added checking for injured so that characters that are injured won't keep trying to hunt a character, then flee after seeing the target
                return true;
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    //public void PreTargetInjured() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterTargetInjured() {
        Character target = poiTarget as Character;
        Injured injured = new Injured();
        AddTraitTo(target, injured);
        AddTraitTo(actor, "Combat Recovery");
    }
    public void PreTargetKilled() {
        Character target = poiTarget as Character;
        //currentState.AddLogFiller(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        if(parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        target.Death();
    }
    public void PerTickTargetKilled() {
        actor.AdjustFullness(10);
    }
    public void PreTargetWon() {
        //currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        AddTraitTo(actor, "Combat Recovery");
    }
    //public void PreTargetMissing() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    #endregion
}
