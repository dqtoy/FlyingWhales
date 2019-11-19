using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Drink : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.CONSUME; } }

    public Drink() : base(INTERACTION_TYPE.DRINK) {
        //if (actor.traitContainer.GetNormalTrait("Drunkard") != null) {
        //    validTimeOfDays = null;
        //} else {
        //    validTimeOfDays = new TIME_IN_WORDS[] {
        //        TIME_IN_WORDS.EARLY_NIGHT,
        //    };
        //}
        
        actionIconString = GoapActionStateDB.Drink_Icon;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drink Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        //**Cost**: 15 - 26
        return Utilities.rng.Next(15, 27);
    }
    public override void OnStopWhilePerforming(Character actor, IPointOfInterest target, object[] otherData) {
        base.OnStopWhilePerforming(actor, target, otherData);
        actor.AdjustDoNotGetLonely(-1);
    }
    #endregion

    #region State Effects
    public void PreDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustDoNotGetLonely(1);
    }
    public void PerTickDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustHappiness(500);
    }
    public void AfterDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustDoNotGetLonely(-1);
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Drunk");
    }
    //public void PreDrinkPoisoned() {
    //    actor.AdjustDoNotGetLonely(1);
    //    RemoveTraitFrom(poiTarget, "Poisoned");
    //    Log log = null;
    //    WeightedDictionary<string> result = poisonedTrait.GetResultWeights();
    //    string res = result.PickRandomElementGivenWeights();
    //    if (res == "Sick") {
    //        string logKey = "drink poisoned_sick";
    //        poisonedResult = "Sick";
    //        if (actor.traitContainer.GetNormalTrait("Robust") != null) {
    //            poisonedResult = "Robust";
    //            logKey = "drink poisoned_robust";
    //        }
    //        log = new Log(GameManager.Instance.Today(), "GoapAction", "Drink", logKey, this);
    //        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //    } else if (res == "Death") {
    //        log = new Log(GameManager.Instance.Today(), "GoapAction", "Drink", "drink poisoned_killed", this);
    //        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //        poisonedResult = "Death";
    //    }
    //    currentState.OverrideDescriptionLog(log);
    //}
    //public void PerTickDrinkPoisoned() {
    //    actor.AdjustHappiness(200);
    //}
    //public void AfterDrinkPoisoned() {
    //    actor.AdjustDoNotGetLonely(-1);
    //    if (poisonedResult == "Sick") {
    //        for (int i = 0; i < poisonedTrait.responsibleCharacters.Count; i++) {
    //            AddTraitTo(actor, poisonedResult, poisonedTrait.responsibleCharacters[i]);
    //        }
    //    } else if (poisonedResult == "Death") {
    //        if (parentPlan.job != null) {
    //            parentPlan.job.SetCannotCancelJob(true);
    //        }
    //        SetCannotCancelAction(true);
    //        actor.Death("poisoned", deathFromAction: this);
    //    }
    //}
    //public void PreTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            return poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.INN && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && actor.traitContainer.GetNormalTrait("Agoraphobic") == null;
        }
        return false;
    }
    #endregion
}

public class DrinkData : GoapActionData {
    public DrinkData() : base(INTERACTION_TYPE.DRINK) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return poiTarget.gridTileLocation != null &&  poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.INN && poiTarget.IsAvailable() && actor.traitContainer.GetNormalTrait("Agoraphobic") == null;
    }
}