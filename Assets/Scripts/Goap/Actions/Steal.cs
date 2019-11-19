using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Steal : GoapAction {

    public Steal() : base(INTERACTION_TYPE.STEAL) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
            TIME_IN_WORDS.AFTER_MIDNIGHT,
        };
        actionIconString = GoapActionStateDB.Steal_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.ITEM };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_ITEM, GOAP_EFFECT_TARGET.ACTOR));
    }
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, object[] otherData) {
        List <GoapEffect> ee = base.GetExpectedEffects(actor, target, otherData);
        SpecialToken token = target as SpecialToken;
        ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = token.specialTokenType.ToString(), target = GOAP_EFFECT_TARGET.TARGET });
        return ee;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Steal Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return Utilities.rng.Next(35, 56);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            if (poiTarget.gridTileLocation != null) {
                //return true;
                SpecialToken token = poiTarget as SpecialToken;
                return token.characterOwner == null || token.characterOwner != actor;
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreStealSuccess(ActualGoapNode goapNode) {
        //**Note**: This is a Theft crime
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget as SpecialToken, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //TODO: currentState.SetIntelReaction(State1Reactions);
    }
    private void AfterStealSuccess(ActualGoapNode goapNode) {
        goapNode.actor.PickUpToken(goapNode.poiTarget as SpecialToken, false);
    }
    private void PreTargetMissing(ActualGoapNode goapNode) {
        //goapNode.descriptionLog.AddToFillers(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        //goapNode.descriptionLog.AddToFillers(poiTarget as SpecialToken, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterTargetMissing(ActualGoapNode goapNode) {
        //actor.RemoveAwareness(poiTarget);
    }
    #endregion

    //#region Intel Reactions
    //private List<string> State1Reactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    SpecialToken stolenItem = poiTarget as SpecialToken;
    //    //Recipient is the owner of the item:
    //    if (recipient == stolenItem.characterOwner) {
    //        //- **Recipient Response Text**: "[Actor Name] stole from me? What a horrible person."
    //        reactions.Add(string.Format("{0} stole from me? What a horrible person.", actor.name));
    //        //- **Recipient Effect**: Remove Friend/Lover/Paramour relationship between Actor and Recipient.Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
    //        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //        List<RELATIONSHIP_TRAIT> traitsToRemove = recipient.relationshipContainer.GetRelationshipDataWith(actor).GetAllRelationshipOfEffect(RELATIONSHIP_EFFECT.POSITIVE);
    //        for (int i = 0; i < traitsToRemove.Count; i++) {
    //            RelationshipManager.Instance.RemoveRelationshipBetween(recipient, actor, traitsToRemove[i]);
    //        }
    //    }

    //    //Recipient and Actor is the same:
    //    else if (recipient == actor) {
    //        //- **Recipient Response Text**: "I know what I did."
    //        reactions.Add("I know what I did.");
    //        //-**Recipient Effect**: no effect
    //    }

    //    //Recipient and Actor have a positive relationship:
    //    else if (recipient.relationshipContainer.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
    //        //- **Recipient Response Text**: "[Actor Name] may have committed theft but I know that [he/she] is a good person."
    //        reactions.Add(string.Format("{0} may have committed theft but I know that {1} is a good person.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //        //-**Recipient Effect**: no effect
    //    }
    //    //Recipient and Actor have a negative relationship:
    //    else if (recipient.relationshipContainer.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //        //- **Recipient Response Text**: "[Actor Name] committed theft!? Why am I not surprised."
    //        reactions.Add(string.Format("{0} committed theft!? Why am I not surprised.", actor.name));
    //        //-**Recipient Effect**: Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
    //        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //    }
    //    //Recipient and Actor have no relationship but are from the same faction:
    //    else if (!recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo) && recipient.faction == actor.faction) {
    //        //- **Recipient Response Text**: "[Actor Name] committed theft!? That's illegal."
    //        reactions.Add(string.Format("{0} committed theft!? That's illegal.", actor.name));
    //        //- **Recipient Effect**: Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
    //        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class StealData : GoapActionData {
    public StealData() : base(INTERACTION_TYPE.STEAL) {
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (poiTarget.gridTileLocation != null) {
            //return true;
            SpecialToken token = poiTarget as SpecialToken;
            return token.characterOwner == null || token.characterOwner != actor;
        }
        return false;
    }
}