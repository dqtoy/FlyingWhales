using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakUp : GoapAction {

    public Character targetCharacter { get; private set; }

    public BreakUp(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.BREAK_UP, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Social_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        //**Effect 1**: Actor - Remove Lover relationship with Target
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Lover", targetPOI = actor });
        //**Effect 2**: Actor - Remove Paramour relationship with Target
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Paramour", targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Break Up Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region Effects
    private void PreBreakUpSuccess() {
        Character target = poiTarget as Character;
        if (actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
            //**Effect 1**: Actor - Remove Lover relationship with Character 2
            CharacterManager.Instance.RemoveRelationshipBetween(actor, target, RELATIONSHIP_TRAIT.LOVER);
            //if the relationship that was removed is lover, change home to a random unoccupied dwelling,
            //otherwise, no home. Reference: https://trello.com/c/JUSt9bEa/1938-broken-up-characters-should-live-in-separate-house
            actor.MigrateHomeStructureTo(null);
            actor.homeArea.AssignCharacterToDwellingInArea(actor);
        } else {
            //**Effect 2**: Actor - Remove Paramour relationship with Character 2
            CharacterManager.Instance.RemoveRelationshipBetween(actor, target, RELATIONSHIP_TRAIT.PARAMOUR);
        }
        //**Effect 3**: Target gains Heartbroken trait
        AddTraitTo(target, "Heartbroken", actor);
        currentState.SetIntelReaction(BreakupSuccessIntelReaction);
    }
    //private void PreTargetMissing() {
    //    //currentState.AddLogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    #endregion

    #region Requirement
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        Character target = poiTarget as Character;
        if (target == actor) {
            return false;
        }
        if (target.currentAlterEgoName != CharacterManager.Original_Alter_Ego) {
            return false;
        }
        if (target.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        if (!actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER) && !actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR)) {
            return false; //**Advertised To**: All characters with Lover or Paramour relationship with the character
        }
        return target.IsInOwnParty();
    }
    #endregion

    #region Intel Reactions
    private List<string> BreakupSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;
        bool isRecipientLoverOrParamourOfActor = actor.HasRelationshipOfTypeWith(recipient, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR);
        bool isRecipientLoverOrParamourOfTarget = targetCharacter.HasRelationshipOfTypeWith(recipient, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR);

        bool isRecipientInLoveWithActor = recipient.HasRelationshipOfTypeWith(actor, false, RELATIONSHIP_TRAIT.LOVER);
        bool isRecipientInLoveWithTarget = recipient.HasRelationshipOfTypeWith(targetCharacter, false, RELATIONSHIP_TRAIT.LOVER);

        RELATIONSHIP_EFFECT relWithActor = recipient.GetRelationshipEffectWith(actor);
        RELATIONSHIP_EFFECT relWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);

        if (isOldNews) {
            //Old News
            reactions.Add("This is old news.");
        } else {
            //Not Yet Old News
            if (awareCharactersOfThisAction.Contains(recipient)) {
                //- If Recipient is Aware
                reactions.Add("I know that already.");
            } else {
                //- Recipient is Actor
                if (recipient == actor) {
                    reactions.Add("I had to do it.");
                }
                //- Recipient is Target
                else if (recipient == targetCharacter) {
                    reactions.Add("Yes, we're done. You don't need to remind me.");
                }
                //- Recipient is Lover or Paramour of Actor
                else if (isRecipientLoverOrParamourOfActor) {
                    recipient.AddTrait("Cheery");
                    reactions.Add(string.Format("This is great news! My relationship with {0} will be better now.", actor.name));
                }
                //- Recipient is Lover or Paramour of Target
                else if (isRecipientLoverOrParamourOfActor) {
                    recipient.AddTrait("Cheery");
                    reactions.Add(string.Format("This is great news! My relationship with {0} will be better now.", targetCharacter.name));
                }
                //- Is in Love with Actor
                else if (isRecipientInLoveWithActor) {
                    recipient.AddTrait("Cheery");
                    reactions.Add(string.Format("This means I have a chance with {0} now.", actor.name));
                }
                //- Is in Love with Target
                else if (isRecipientInLoveWithTarget) {
                    recipient.AddTrait("Cheery");
                    reactions.Add(string.Format("This means I have a chance with {0} now.", targetCharacter.name));
                }
                //- Other Positive Relationship with Actor
                else if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                    reactions.Add(string.Format("Break up sucks! I hope {0} feel better soon.", actor.name));
                }
                //- Other Positive Relationship with Target
                else if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                    reactions.Add(string.Format("Break up sucks! I hope {0} feel better soon.", targetCharacter.name));
                }
                //- Negative Relationship with Actor
                else if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                    reactions.Add(string.Format("{0} is a terrible person and does not deserve to be happy.", actor.name));
                }
                //- Negative Relationship with Target
                else if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                    reactions.Add(string.Format("{0} is a terrible person and does not deserve to be happy.", targetCharacter.name));
                }
                //- Others
                else {
                    reactions.Add("How is this relevant to me?");
                }
            }
        }
        return reactions;
    }
    #endregion
}
