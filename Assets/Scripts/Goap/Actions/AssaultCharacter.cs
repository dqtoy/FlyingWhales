using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultCharacter : GoapAction {

    private Character winner;
    private Character loser;

    public AssaultCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ASSAULT_ACTION_NPC, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        Character targetCharacter = poiTarget as Character;
        if (!isTargetMissing && targetCharacter.IsInOwnParty() && !targetCharacter.isDead) {

            float attackersChance = 0f;
            float defendersChance = 0f;

            CombatManager.Instance.GetCombatChanceOfTwoLists(new List<Character>() { actor }, new List<Character>() { targetCharacter }, out attackersChance, out defendersChance);

            string nextState = CombatEncounterEvents(actor, targetCharacter, UnityEngine.Random.Range(0, 100) < attackersChance);
            if (nextState == "Target Killed") {
                parentPlan.SetDoNotRecalculate(true);
            }
            SetState(nextState);
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
    public override void DoAction(GoapPlan plan) {
        SetTargetStructure();
        base.DoAction(plan);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor != poiTarget;
    }
    #endregion

    #region State Effects
    public void PreTargetInjured() {
        //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is an Assault crime
        if (!actor.IsHostileWith(poiTarget as Character)
           //Assaulting a criminal as part of apprehending him should not be considered a crime
           && (parentPlan.job == null || parentPlan.job.name != "Apprehend")) {
            SetCommittedCrime(CRIME.MURDER);
        }
        currentState.AddLogFiller(loser, loser.name, LOG_IDENTIFIER.CHARACTER_3);
        AddTraitTo(winner, "Combat Recovery");
        Injured injured = new Injured();
        AddTraitTo(loser, injured, winner);
        currentState.SetIntelReaction(State1And2Reactions);
    }
    public void AfterTargetInjured() {
        //moved this to pre effect, because if put here, will cause infinite loop:
        // - loser will gain injured trait
        // - if the loser is the actor of this action
        // - will switch to flee state
        // - which will then stop the current action which is this, but still perform the after action
        // - so this loop will never end.

        //Injured injured = new Injured();
        //AddTraitTo(loser, injured, winner);
    }
    public void PreTargetKnockedOut() {
        //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is an Assault crime
        if (!actor.IsHostileWith(poiTarget as Character)
            //Assaulting a criminal as part of apprehending him should not be considered a crime
            && (parentPlan.job == null || parentPlan.job.name != "Apprehend")) {
            SetCommittedCrime(CRIME.ASSAULT);
        }
        currentState.AddLogFiller(loser, loser.name, LOG_IDENTIFIER.CHARACTER_3);
        AddTraitTo(winner, "Combat Recovery");
        currentState.SetIntelReaction(State1And2Reactions);
    }
    public void AfterTargetKnockedOut() {
        if(parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        resumeTargetCharacterState = false; //do not resume the target character's current state after being knocked out
        Character target = poiTarget as Character;
        Unconscious unconscious = new Unconscious();
        AddTraitTo(loser, unconscious, winner);
    }
    public void PreTargetKilled() {
        //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is a Murder crime
        if (!actor.IsHostileWith(poiTarget as Character)
            //Assaulting a criminal as part of apprehending him should not be considered a crime
            && (parentPlan.job == null || parentPlan.job.name != "Apprehend")) { 
            SetCommittedCrime(CRIME.MURDER);
        }
        currentState.AddLogFiller(loser, loser.name, LOG_IDENTIFIER.CHARACTER_3);
        AddTraitTo(winner, "Combat Recovery");
        currentState.SetIntelReaction(State3Reactions);
    }
    public void AfterTargetKilled() {
        if (parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        SetCannotCancelAction(true);
        loser.Death();
    }
    //public void PreTargetMissing() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    #endregion

    #region Combat
    private string CombatEncounterEvents(Character actor, Character target, bool actorWon) {
        //Reference: https://trello.com/c/uY7JokJn/1573-combat-encounter-event

        if (actorWon) {
            winner = actor;
            loser = target;
        } else {
            winner = target;
            loser = actor;
        }

        //**Character That Lost**
        //40 Weight: Gain Unconscious trait (reduce to 0 if already Unconscious)
        //10 Weight: Gain Injured trait and enter Flee mode (reduce to 0 if already Injured)
        //5 Weight: death
        WeightedDictionary<string> loserResults = new WeightedDictionary<string>();
        if (loser.GetTrait("Unconscious") == null) {
            loserResults.AddElement("Unconscious", 40);
        }
        if (loser.GetTrait("Injured") == null) {
            loserResults.AddElement("Injured", 10);
        }
        loserResults.AddElement("Death", 5);

        string result = loserResults.PickRandomElementGivenWeights();
        switch (result) {
            case "Unconscious":
                return "Target Knocked Out";
            case "Injured":
                return "Target Injured";
            case "Death":
                return "Target Killed";
        }
        throw new System.Exception("No state for result " + result);
    }
    #endregion

    #region Intel Reactions
    private List<string> State1And2Reactions(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

         //Recipient and Actor are the same
        if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I've done!"
            reactions.Add(string.Format("I know what I've done!", actor.name));
            //-**Recipient Effect**:  no effect
        }
        //Recipient and Target have a negative relationship:
        else if (recipient.HasRelationshipOfEffectWith(target, TRAIT_EFFECT.NEGATIVE)) {
            //- **Recipient Response Text**: "[Target Name] deserves that!"
            reactions.Add(string.Format("{0} deserves that!", target.name));
            //-**Recipient Effect**: no effect
        }

        //Recipient and Actor are from the same faction and they dont have a positive relationship. 
        else if (recipient.faction == actor.faction && !recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)
            && committedCrime != CRIME.NONE) {
            //Target is not considered Hostile to Recipient and Actor's faction:
            //- **Recipient Response Text**: "[Actor Name] committed an assault!?"
            reactions.Add(string.Format("{0} committed an assault!?", actor.name));
            //-**Recipient Effect**:  Apply Crime System handling as if the Recipient witnessed Actor commit an Assault.
            recipient.ReactToCrime(CRIME.ASSAULT, actor);
        }

        //Recipient and Actor are from the same faction and they have a positive relationship:
        else if (recipient.faction == actor.faction && recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "I'm sure there's a reason [Actor Name] did that."
            reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
            //-**Recipient Effect * *: no effect
        }

        //Recipient and Actor are from the same faction and they dont have a positive relationship. Target is considered Hostile to Recipient and Actor's faction:
        else if (recipient.faction == actor.faction && !recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)
            && committedCrime != CRIME.NONE) {
            //- **Recipient Response Text**: "I'm sure there's a reason [Actor Name] did that."
            reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
            //-**Recipient Effect * *: no effect
        }

        //Recipient and Target have a positive relationship or Recipient and Target are from the same faction and they dont have a negative relationship:
        else if (recipient.HasRelationshipOfEffectWith(target, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE) ||
            (recipient.faction == target.faction && !recipient.HasRelationshipOfEffectWith(target, TRAIT_EFFECT.NEGATIVE))) {
            //- **Recipient Response Text**: "Poor [Target Name]! I hope [he/she]'s okay."
            reactions.Add(string.Format("Poor {0}! I hope {1}'s okay.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            //-**Recipient Effect**: no effect
        }       
        return reactions;
    }
    private List<string> State3Reactions(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        //Recipient and Actor are the same
        if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I've done!"
            reactions.Add(string.Format("I know what I've done!", actor.name));
            //-**Recipient Effect**:  no effect
        }

        //Recipient and Target have a positive relationship:
        else if (recipient.HasRelationshipOfEffectWith(target, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "That despicable [Actor Name] killed [Target Name]! [He/She] is a murderer!"
            reactions.Add(string.Format("That despicable {0} killed {1}, {2} is a murderer!", actor.name, target.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            //-**Recipient Effect**: Remove any positive relationships between Actor and Recipient. Add Enemy relationship if they are not yet enemies. Apply Crime System handling as if the Recipient witnessed Actor commit a Murder.
            recipient.ReactToCrime(CRIME.MURDER, actor); //removal of relationships should be handled by crime system
            if (!recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
                CharacterManager.Instance.CreateNewRelationshipBetween(recipient, actor, RELATIONSHIP_TRAIT.ENEMY);
            }
        }

        //Recipient and Target have a negative relationship:
        else if (recipient.HasRelationshipOfEffectWith(target, TRAIT_EFFECT.NEGATIVE)) {
            //- **Recipient Response Text**: "I am glad that [Actor Name] dealt with [Killed Character Name]!"
            reactions.Add(string.Format("I am glad that {0} dealt with {1}!", actor.name, target.name));
            //-**Recipient Effect**: If Actor and Recipient have no relationships yet, they will become friends.
            if (!recipient.HasRelationshipWith(actor)) {
                CharacterManager.Instance.CreateNewRelationshipBetween(recipient, actor, RELATIONSHIP_TRAIT.FRIEND);
            }
        }

        //Recipient and Actor are from the same faction and they have a positive relationship:
        else if (recipient.faction == actor.faction && recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "[Actor Name] killed somebody! This is horrible!"
            reactions.Add(string.Format("{0} killed somebody! This is horrible!", actor.name));
            //-**Recipient Effect**: Apply Crime System handling as if the Recipient witnessed Actor commit a Murder.
            recipient.ReactToCrime(CRIME.MURDER, actor);
        }

        //Recipient and Actor are from the same faction and they dont have a positive relationship. 
        else if (recipient.faction == actor.faction && !recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            //Target is considered Hostile to Recipient and Actor's faction:
            if (committedCrime == CRIME.NONE) {
                //- **Recipient Response Text**: "I'm sure there's a reason [Actor Name] did that."
                reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
                //-**Recipient Effect**: no effect
            }

            //Target is not considered Hostile to Recipient and Actor's faction:
            else {
                //- **Recipient Response Text**: "[Actor Name] killed somebody! This is horrible!"
                reactions.Add(string.Format("{0} killed somebody! This is horrible!", actor.name));
                //-**Recipient Effect**: Apply Crime System handling as if the Recipient witnessed Actor commit a Murder.
                recipient.ReactToCrime(CRIME.MURDER, actor);
            }
        }
        
        return reactions;
    }
    #endregion
}
