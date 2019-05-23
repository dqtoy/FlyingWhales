using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseCharacter : GoapAction {

    public ReleaseCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = HasAbductedOrRestrainedTrait;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.TOOL.ToString(), targetPOI = actor }, HasItemTool);
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Abducted", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            SetState("Release Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    #endregion

    #region Preconditions
    private bool HasItemTool() {
        return actor.isHoldingItem && actor.GetToken("Tool") != null;
    }
    #endregion

    #region Requirements
    protected bool HasAbductedOrRestrainedTrait() {
        if (poiTarget is Character) {
            Character target = poiTarget as Character;
            //return target.GetTraitOr("Abducted", "Restrained") != null;
            return target.GetTrait("Restrained") != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    //public void PreReleaseSuccess() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterReleaseSuccess() {
        Character target = poiTarget as Character;
        RemoveTraitFrom(target, "Restrained");
        RemoveTraitFrom(target, "Abducted");
        currentState.SetIntelReaction(ReleaseSuccessIntelReaction);
    }
    //public void PreTargetMissing() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Intel Reactions
    private List<string> ReleaseSuccessIntelReaction(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        //Recipient and Actor are the same
        if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I've done!"
            reactions.Add("I know what I've done!");
            //- **Recipient Effect**:  no effect
        }
        //Recipient considers Target a personal Enemy:
        else if (recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Target Name] should not have been released!"
            reactions.Add(string.Format("{0} should not have been released!", target.name));
            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between Recipient and Actor
            CharacterManager.Instance.RelationshipDegradation(actor, recipient);
        }
        //Recipient considers Actor a personal Enemy:
        else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Actor Name] probably has an ulterior motive for doing that."
            reactions.Add(string.Format("{0} probably has an ulterior motive for doing that.", actor.name));
            //- **Recipient Effect**:  no effect
        }
        //Recipient has a positive relationship with the target:
        else if (recipient.HasRelationshipOfEffectWith(target, TRAIT_EFFECT.POSITIVE)) {
            //- **Recipient Response Text**: "I am relieved that [Target Name] has been released."
            reactions.Add(string.Format("I am relieved that {0} has been released.", target.name));
            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between Recipient and Actor
            CharacterManager.Instance.RelationshipDegradation(actor, recipient);
        }
        //Recipient and Target have no relationship but are from the same faction:
        else if (!recipient.HasRelationshipWith(target) && recipient.faction == target.faction) {
            //- **Recipient Response Text**: "I am relieved that [Target Name] has been released."
            reactions.Add(string.Format("I am relieved that {0} has been released.", target.name));
            //- **Recipient Effect**:  no effect
        }
        return reactions;
    }
    #endregion
}
