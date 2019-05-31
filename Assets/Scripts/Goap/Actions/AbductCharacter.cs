using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbductCharacter : GoapAction {
    public AbductCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ABDUCT_ACTION, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        //_isStealthAction = true;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = poiTarget }, HasNonPositiveDisablerTrait);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            SetState("Abduct Success");
            //if (!HasOtherCharacterInRadius()) {
            //    SetState("Abduct Success");
            //} else {
            //    parentPlan.SetDoNotRecalculate(true);
            //    SetState("Abduct Fail");
            //}
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 3;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if(actor != poiTarget) {
            Character target = poiTarget as Character;
            return target.GetNormalTrait("Restrained") == null;
        }
        return false;
    }
    #endregion

    //#region Preconditions
    //private bool HasNonPositiveDisablerTrait() {
    //    Character target = poiTarget as Character;
    //    return target.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER);
    //}
    //#endregion

    #region State Effects
    //public void PreAbductSuccess() {
        //currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterAbductSuccess() {
        if (parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        Character target = poiTarget as Character;
        Restrained restrainedTrait = new Restrained();
        target.AddTrait(restrainedTrait, actor);

        currentState.SetIntelReaction(AbductSuccessIntelReaction);

        AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
    }
    //public void PreTargetMissing() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Intel Reactions
    private List<string> AbductSuccessIntelReaction(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        //Recipient and Actor are the same
        if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I've done!"
            reactions.Add("I know what I've done!");
            //- **Recipient Effect**: no effect
        }
        //Recipient considers Target a personal Enemy:
        else if (recipient.HasRelationshipOfTypeWith(poiTargetAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Target Name] deserves that!"
            reactions.Add(string.Format("{0} deserves that!", target.name));
            //- **Recipient Effect**: no effect
        }
        //Recipient considers Target a personal Friend, Paramour, Lover or Relative:
        else if (recipient.HasRelationshipOfTypeWith(poiTargetAlterEgo, true, RELATIONSHIP_TRAIT.FRIEND, RELATIONSHIP_TRAIT.PARAMOUR, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "Where is [Actor Name] taking [Target Name]!? Please let me know if you find out."
            reactions.Add(string.Format("Where is {0} taking {1}!? Please let me know if you find out.", actor.name, target.name));
            //- **Recipient Effect**: no effect
        }
        //Recipient considers Actor a personal Enemy:
        else if (recipient.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Actor Name] is truly ruthless."
            reactions.Add(string.Format("{0} is truly ruthless.", actor.name));
            //- **Recipient Effect**: no effect
        }
        //Recipient and Target have no relationship but are from the same faction:
        else if (!recipient.HasRelationshipWith(poiTargetAlterEgo) && recipient.faction.id == poiTargetAlterEgo.faction.id) {
            //- **Recipient Response Text**: "Where is [Actor Name] taking [Target Name]!? Please let me know if you find out."
            reactions.Add(string.Format("Where is {0} taking {1}!? Please let me know if you find out.", actor.name, target.name));
            //- **Recipient Effect**: no effect
        }
        return reactions;
    }
    #endregion
}