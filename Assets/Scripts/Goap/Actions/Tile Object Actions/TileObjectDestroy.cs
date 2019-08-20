using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObjectDestroy : GoapAction {
    public TileObjectDestroy(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TILE_OBJECT_DESTROY, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Destroy Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 10;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    #endregion

    #region State Effects
    private void PreDestroySuccess() {
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.SetIntelReaction(SuccessReactions);
    }
    private void AfterDestroySuccess() {
        //**After Effect 1**: Destroy target tile object
        poiTarget.gridTileLocation.structure.RemovePOI(poiTarget, actor);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
    #endregion

    #region Intel Reactions
    private List<string> SuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        TileObject tileObj = poiTarget as TileObject;

        RELATIONSHIP_EFFECT relWithActor = recipient.GetRelationshipEffectWith(actor);
        if (recipient == actor) {
            // - If informed: "I am embarrassed by my own actions."
            if (status == SHARE_INTEL_STATUS.INFORMED) {
                reactions.Add("I am embarrassed by my own actions.");
            }
        }
        //- Owns the item destroyed
        else if (tileObj.IsOwnedBy(recipient)) {
            if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                //-Witnesser has a negative relationship with Actor
                CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);  //-Relationship degradation with Actor
                if (status == SHARE_INTEL_STATUS.WITNESSED) {
                    //- If witnessed: Create an Assault job targeting the Actor
                    recipient.CreateKnockoutJob(actor);
                } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                    //- If informed: "[Actor Name] is getting even more unhinged day by day!"
                    reactions.Add(string.Format("{0} is getting even more unhinged day by day!", actor.name));
                }
            } else if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE || relWithActor == RELATIONSHIP_EFFECT.NONE) {
                //- Witnesser has a positive or neutral relationship with Actor
                CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);  //-Relationship degradation with Actor
                if (status == SHARE_INTEL_STATUS.INFORMED) {
                    //- If informed: "Perhaps it's best if I avoid [Actor Name] for now."
                    reactions.Add(string.Format("Perhaps it's best if I avoid {0} for now.", actor.name));
                }
            }
        }
        //- Others
        else {
            // - If informed: "This isn't relevant to me."
            if (status == SHARE_INTEL_STATUS.INFORMED) {
                reactions.Add("This isn't relevant to me.");
            }
        }


        return reactions;
    }
    #endregion
}
