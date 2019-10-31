using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whip : GoapAction {

    public Whip(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.WHIP, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget }, MustBeRestrained);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Injured", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Lethargic", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Whip Success");
    }
    protected override int GetCost() {
        return 25;
    }
    #endregion

    #region State Effects
    private void PreWhipSuccess() {
        
    }
    public void AfterWhipSuccess() {
        //**After Effect 1**: Target gains Injured status
        AddTraitTo(poiTarget, "Injured", actor);
        //**After Effect 2**: Target gains Lethargic status
        AddTraitTo(poiTarget, "Lethargic", actor);
        //**After Effect 3**: Target is released from imprisonment and crime is removed from trait list
        poiTarget.RemoveAllTraitsByType(TRAIT_TYPE.CRIMINAL);
        RemoveTraitFrom(poiTarget, "Restrained", actor);
    }
    #endregion


    #region Preconditions
    private bool MustBeRestrained() {
        return poiTarget.GetNormalTrait("Restrained") != null;
    }
    #endregion
}

public class WhipData : GoapActionData {
    public WhipData() : base(INTERACTION_TYPE.WHIP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }
}