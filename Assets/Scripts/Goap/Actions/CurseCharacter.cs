using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurseCharacter : GoapAction {

    public CurseCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CURSE_CHARACTER, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Social_Icon;
        shouldAddLogs = false; //set to false because this action has a special case for logs
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Ritualized", targetPOI = actor }, () => HasTrait(actor, "Ritualized"));
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Cursed", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        SetState("Curse Success");
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 3;
    }
    #endregion

    #region State Effects
    public void AfterCurseSuccess() {
        //**After Effect 1**: Target gains Cursed trait.
        Cursed cursed = new Cursed();
        AddTraitTo(poiTarget, cursed);
        //**After Effect 2**: Actor loses Ritualized trait.
        RemoveTraitFrom(actor, "Ritualized");

        Log actorLog = new Log(GameManager.Instance.Today(), "GoapAction", this.GetType().ToString(), currentState.name.ToLower() + "_description_actor");
        actorLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        actorLog.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        Log targetLog = new Log(GameManager.Instance.Today(), "GoapAction", this.GetType().ToString(), currentState.name.ToLower() + "_description_target");
        targetLog.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        actor.AddHistory(actorLog);
        (poiTarget as Character).AddHistory(targetLog);

    }
    #endregion
}
