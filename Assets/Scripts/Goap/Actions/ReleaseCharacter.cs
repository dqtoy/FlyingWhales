using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseCharacter : GoapAction {

    public ReleaseCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RELEASE_CHARACTER, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = HasAbductedOrRestrainedTrait;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.TOOL.ToString(), targetPOI = actor }, HasItemTool);
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Abducted", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
    }
    public override void Perform() {
        base.Perform();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            SetState("Release Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetBaseCost() {
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
            return target.GetNormalTrait("Restrained") != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreReleaseSuccess() {
        //currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.SetIntelReaction(ReleaseSuccessIntelReaction);
    }
    public void AfterReleaseSuccess() {
        Character target = poiTarget as Character;
        RemoveTraitFrom(target, "Restrained");
        RemoveTraitFrom(target, "Abducted");
    }
    //public void PreTargetMissing() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Intel Reactions
    private List<string> ReleaseSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        RELATIONSHIP_EFFECT relWithTarget = recipient.GetRelationshipEffectWith(poiTargetAlterEgo);

        //Recipient and Actor are the same
        if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I've done!"
            reactions.Add("I know what I've done!");
            //- **Recipient Effect**:  no effect
        }
        //Recipient considers Target a personal Enemy:
        else if (recipient.HasRelationshipOfTypeWith(poiTargetAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Target Name] should not have been released!"
            reactions.Add(string.Format("{0} should not have been released!", target.name));
            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between Recipient and Actor
            CharacterManager.Instance.RelationshipDegradation(actorAlterEgo, recipient);
        }
        //Recipient considers Actor a personal Enemy:
        else if (recipient.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Actor Name] probably has an ulterior motive for doing that."
            reactions.Add(string.Format("{0} probably has an ulterior motive for doing that.", actor.name));
            //- **Recipient Effect**:  no effect
        }
        //Recipient has a positive relationship with the target:
        else if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
            //- **Recipient Response Text**: "I am relieved that [Target Name] has been released."
            reactions.Add(string.Format("I am relieved that {0} has been released.", target.name));
            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between Recipient and Actor
            CharacterManager.Instance.RelationshipDegradation(actorAlterEgo, recipient);
        }
        //Recipient and Target have no relationship but are from the same faction:
        else if (relWithTarget == RELATIONSHIP_EFFECT.NONE && recipient.faction == poiTargetAlterEgo.faction) {
            //- **Recipient Response Text**: "I am relieved that [Target Name] has been released."
            reactions.Add(string.Format("I am relieved that {0} has been released.", target.name));
            //- **Recipient Effect**:  no effect
        } 
        else {
            reactions.Add("This does not concern me.");
        }
        return reactions;
    }
    #endregion
}

public class ReleaseCharacterData : GoapActionData {
    public ReleaseCharacterData() : base(INTERACTION_TYPE.RELEASE_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget is Character) {
            Character target = poiTarget as Character;
            return target.GetNormalTrait("Restrained") != null;
        }
        return false;
    }
}
