using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeCharacter : GoapAction {

    public JudgeCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.JUDGE_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        //actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        //validTimeOfDays = new TIME_IN_WORDS[] {
        //    TIME_IN_WORDS.MORNING,
        //    TIME_IN_WORDS.AFTERNOON,
        //    TIME_IN_WORDS.EARLY_NIGHT,
        //    TIME_IN_WORDS.LATE_NIGHT,
        //};
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.NONE, targetPOI = poiTarget });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            WeightedDictionary<string> weights = new WeightedDictionary<string>();
            //weights.AddElement("Target Executed", 10);
            weights.AddElement("Target Released", 10);
            if (poiTarget.factionOwner == actor.faction) {
                weights.AddElement("Target Exiled", 10);
            }
            SetState(weights.PickRandomElementGivenWeights());
        }
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region State Effects
    public void PreTargetExecuted() {
        if (parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        SetCannotCancelAction(true);
        //**Effect 1**: Remove target's Restrained trait
        //**Effect 2**: Target dies
        (poiTarget as Character).Death();

        RemoveTraitFrom(poiTarget, "Restrained");
    }
    public void PreTargetReleased() {
        //**Effect 1**: Remove target's Restrained trait
        RemoveTraitFrom(poiTarget, "Restrained");
        //**Effect 2**: If target is from a different faction or unaligned, target is not hostile with characters from the Actor's faction until Target leaves the location. Target is forced to create a Return Home plan
        if (poiTarget.factionOwner == FactionManager.Instance.neutralFaction || poiTarget.factionOwner != actor.faction) {
            ForceTargetReturnHome();
        }
        //**Effect 3**: If target is from the same faction, remove any Criminal type trait from him.
        else {
            RemoveTraitsOfType(poiTarget, TRAIT_TYPE.CRIMINAL);
        }
    }
    public void PreTargetExiled() {
        //**Effect 1**: Remove target's Restrained trait
        RemoveTraitFrom(poiTarget, "Restrained");
        //**Effect 2**: Target becomes unaligned and will have his Home Location set to a random different location
        Character target = poiTarget as Character;
        target.ChangeFactionTo(FactionManager.Instance.neutralFaction);
        List<Area> choices = new List<Area>(LandmarkManager.Instance.allAreas);
        choices.Remove(target.homeArea);
        Area newHome = choices[Random.Range(0, choices.Count)];
        target.MigrateHomeTo(newHome);

        //**Effect 3**: Target is not hostile with characters from the Actor's faction until Target leaves the location. Target is forced to create a Return Home plan
        ForceTargetReturnHome();

        //**Effect 4**: Remove any Criminal type trait from him.
        RemoveTraitsOfType(target, TRAIT_TYPE.CRIMINAL);
    }
    #endregion

    private void ForceTargetReturnHome() {
        Character target = poiTarget as Character;
        target.AdjustIgnoreHostilities(1); //target should ignore hostilities or be ignored by other hostiles, until it returns home.
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.RETURN_HOME, target, poiTarget);
        goapAction.SetTargetStructure();
        target.AddOnLeaveAreaAction(() => target.AdjustIgnoreHostilities(-1));
        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        goapPlan.ConstructAllNodes();
        target.AddPlan(goapPlan, true);
    }
}
