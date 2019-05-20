using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeLove : GoapAction {

    public Character targetCharacter { get; private set; }

    public MakeLove(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.MAKE_LOVE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = poiTarget });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        targetCharacter.OnTargettedByAction(this);
        if (!isTargetMissing) {
            if (targetCharacter.currentParty == actor.ownParty && !targetCharacter.isStarving && !targetCharacter.isExhausted 
                && targetCharacter.GetTrait("Annoyed") == null) {
                SetState("Make Love Success");
            } else {
                SetState("Make Love Fail");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    protected override void AddDefaultObjectsToLog(Log log) {
        base.AddDefaultObjectsToLog(log);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public override void OnStopActionDuringCurrentState() {
        actor.ownParty.RemoveCharacter(targetCharacter);
    }
    public override void OnResultReturnedToActor() {
        base.OnResultReturnedToActor();
        targetCharacter.RemoveTargettedByAction(this);
        if (targetCharacter.currentAction == this) {
            targetCharacter.SetCurrentAction(null);
        }
    }
    #endregion

    #region Effects
    private void PreMakeLoveSuccess() {
        targetCharacter.SetCurrentAction(this);
        currentState.AddLogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void PerTickMakeLoveSuccess() {
        //**Per Tick Effect 1 * *: Actor's Happiness Meter +6
        actor.AdjustHappiness(6);
        //**Per Tick Effect 2**: Target's Happiness Meter +6
        targetCharacter.AdjustHappiness(6);
    }
    private void AfterMakeLoveSuccess() {
        //**After Effect 1**: If Actor and Target are Lovers, they both gain Cheery trait. If Actor and Target are Paramours, they both gain Ashamed trait.
        if (actor.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.LOVER)) {
            AddTraitTo(actor, "Cheery", targetCharacter);
            AddTraitTo(targetCharacter, "Cheery", actor);
        } else {
            AddTraitTo(actor, "Ashamed", targetCharacter);
            AddTraitTo(targetCharacter, "Ashamed", actor);
        }

        actor.ownParty.RemoveCharacter(targetCharacter);
        RemoveTraitFrom(targetCharacter, "Wooed");
    }
    private void PreMakeLoveFail() {
        currentState.AddLogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void AfterMakeLoveFail() {
        //**After Effect 1**: Actor gains Annoyed trait.
        AddTraitTo(actor, "Annoyed", actor);
        actor.ownParty.RemoveCharacter(targetCharacter);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    //#region Requirements
    //protected bool Requirement() {
    //    return poiTarget.state != POI_STATE.INACTIVE;
    //}
    //#endregion

    public void SetTargetCharacter(Character targetCharacter) {
        this.targetCharacter = targetCharacter;
    }
}
