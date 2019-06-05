using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealFromCharacter : GoapAction {

    private SpecialToken _targetItem;
    private Character _targetCharacter;

    public StealFromCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.STEAL_CHARACTER, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        //validTimeOfDays = new TIME_IN_WORDS[] {
        //    TIME_IN_WORDS.EARLY_NIGHT,
        //    TIME_IN_WORDS.LATE_NIGHT,
        //    TIME_IN_WORDS.AFTER_MIDNIGHT,
        //};
        actionIconString = GoapActionStateDB.Hostile_Icon;
        _targetCharacter = poiTarget as Character;
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        if (actor.GetNormalTrait("Kleptomaniac") != null) {
            AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
        }
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            if (_targetCharacter.isHoldingItem) {
                SetState("Steal Success");
            } else {
                SetState("Steal Fail");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        if (actor.GetNormalTrait("Kleptomaniac") != null) {
            return Utilities.rng.Next(5, 46);
        }
        return Utilities.rng.Next(35, 56);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        //exclude characters that the actor knows has no items.
        Kleptomaniac kleptomaniacTrait = actor.GetNormalTrait("Kleptomaniac") as Kleptomaniac;
        if (kleptomaniacTrait != null && kleptomaniacTrait.noItemCharacters.Contains(poiTarget as Character)) {
            return false;
        }
        if (poiTarget != actor) {
            return true;
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreStealSuccess() {
        _targetItem = _targetCharacter.items[UnityEngine.Random.Range(0, _targetCharacter.items.Count)];
        //**Note**: This is a Theft crime
        SetCommittedCrime(CRIME.THEFT, new Character[] { actor });
        currentState.AddLogFiller(_targetItem, _targetItem.name, LOG_IDENTIFIER.ITEM_1);
        currentState.SetIntelReaction(State1Reactions);
    }
    private void AfterStealSuccess() {
        actor.ObtainTokenFrom(_targetCharacter, _targetItem, false);
        if (actor.GetNormalTrait("Kleptomaniac") != null) {
            actor.AdjustHappiness(60);
        }
    }
    private void PreStealFail() {
        Trait trait = actor.GetNormalTrait("Kleptomaniac");
        if (trait != null) {
            Kleptomaniac kleptomaniac = trait as Kleptomaniac;
            kleptomaniac.AddNoItemCharacter(poiTarget as Character);
        }
        currentState.SetIntelReaction(State2Reactions);
    }
    #endregion

    #region Intel Reactions
    private List<string> State1Reactions(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;
        //Recipient and Target is the same:
        if (recipient == targetCharacter) {
            //- **Recipient Response Text**: "[Actor Name] stole from me? What a horrible person."
            reactions.Add(string.Format("{0} stole from me? What a horrible person.", actor.name));
            //Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
            recipient.ReactToCrime(CRIME.THEFT, this, actorAlterEgo, null, this);
        }
        //Recipient and Actor is the same:
        else if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I did."
            reactions.Add("I know what I did.");
            //-**Recipient Effect**: no effect
        }
        //Recipient and Actor have a positive relationship:
        else if (recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "[Actor Name] may have committed theft but I know that [he/she] is a good person."
            reactions.Add(string.Format("{0} may have committed theft but I know that {1} is a good person.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            //-**Recipient Effect * *: no effect
        }
        //Recipient and Actor have a negative relationship:
        else if (recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.NEGATIVE)) {
            //- **Recipient Response Text**: "[Actor Name] committed theft!? Why am I not surprised."
            reactions.Add(string.Format("{0} committed theft!? Why am I not surprised.", actor.name));
            //-**Recipient Effect**: Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
            recipient.ReactToCrime(CRIME.THEFT, this, actorAlterEgo, null, this);
        }
        //Recipient and Actor have no relationship but are from the same faction:
        else if (!recipient.HasRelationshipWith(actor) && recipient.faction == actor.faction) {
            //- **Recipient Response Text**: "[Actor Name] committed theft!? That's illegal."
            reactions.Add(string.Format("{0} committed theft!? That's illegal.", actor.name));
            //- **Recipient Effect**: Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
            recipient.ReactToCrime(CRIME.THEFT, this, actorAlterEgo, null, this);
        }
        
        return reactions;
    }
    private List<string> State2Reactions(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;
        //Recipient and Target is the same:
        if (recipient == targetCharacter) {
            //- **Recipient Response Text**: "Hahaha! Good thing I'm not carrying anything with me at that time. What a loser."
            reactions.Add("Hahaha! Good thing I'm not carrying anything with me at that time. What a loser.");
            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between Recipient and Actor
            CharacterManager.Instance.RelationshipDegradation(actor, recipient);
        }
        //Recipient and Actor is the same:
        else if (recipient == actor) {
            //- **Recipient Response Text**: "At least I didn't get caught. Too bad I got nothing either."
            reactions.Add("At least I didn't get caught. Too bad I got nothing either.");
            //-**Recipient Effect**: no effect
        }
        //Recipient and Actor have a positive relationship:
        else if (recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "Well, nothing was taken, right? Let it go."
            reactions.Add("Well, nothing was taken, right? Let it go.");
            //-**Recipient Effect * *: no effect
        }
        //Recipient and Actor have a negative relationship:
        else if (recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.NEGATIVE)) {
            //- **Recipient Response Text**: "[Actor Name] committed theft!? Why am I not surprised."
            reactions.Add(string.Format("{0} committed theft!? Why am I not surprised.", actor.name));
            //- **Recipient Effect**: no effect
        }
        //Recipient and Actor have no relationship but are from the same faction:
        else if (!recipient.HasRelationshipWith(actor) && recipient.faction == actor.faction) {
            //- **Recipient Response Text**: "[Actor Name] attempted theft!? I better watch my back around [him/her]."
            reactions.Add(string.Format("{0} attempted theft!? I better watch my back around {1}.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
            //- **Recipient Effect**: Recipient and Actor are now Enemies.
            CharacterManager.Instance.CreateNewRelationshipBetween(recipient, actor, RELATIONSHIP_TRAIT.ENEMY);
        }
        return reactions;
    }
    #endregion
}
