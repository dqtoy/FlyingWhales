using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultCharacter : GoapAction {
    public AssaultCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ASSAULT_ACTION_NPC, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        //List<Character> attackers = new List<Character>();
        //attackers.Add(actor);

        //List<Character> defenders = new List<Character>();
        //defenders.Add(poiTarget as Character);

        //float attackersChance = 0f;
        //float defendersChance = 0f;

        //CombatManager.Instance.GetCombatChanceOfTwoLists(attackers, defenders, out attackersChance, out defendersChance);

        //WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
        //int chance = UnityEngine.Random.Range(0, 100);
        //if (chance < attackersChance) {
        //    //Actor Win
        //    resultWeights.AddElement("Target Injured", 30);
        //    resultWeights.AddElement("Target Knocked Out", 20);
        //    resultWeights.AddElement("Target Killed", 5);
        //} else {
        //    //Target Win
        //    resultWeights.AddElement(Character_Killed_Hunter, 20);
        //    resultWeights.AddElement(Character_Injured_Hunter, 40);
        //}

        //string nextState = resultWeights.PickRandomElementGivenWeights();
        //SetState(nextState);

        if (!isTargetCharacterMissing) {
            WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
            resultWeights.AddElement("Target Injured", 10);
            resultWeights.AddElement("Target Knocked Out", 40);
            resultWeights.AddElement("Target Killed", 5);

            string nextState = resultWeights.PickRandomElementGivenWeights();
            if(nextState == "Target Killed") {
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
        //currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        AddTraitTo(actor, "Combat Recovery");
        currentState.SetIntelReaction(State1And2Reactions);
    }
    public void AfterTargetInjured() {
        Character target = poiTarget as Character;
        Injured injured = new Injured();
        AddTraitTo(target, injured, actor);
    }
    public void PreTargetKnockedOut() {
        //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is an Assault crime
        if (!actor.IsHostileWith(poiTarget as Character)
            //Assaulting a criminal as part of apprehending him should not be considered a crime
            && (parentPlan.job == null || parentPlan.job.name != "Apprehend")) {
            SetCommittedCrime(CRIME.ASSAULT);
        }
        //currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        AddTraitTo(actor, "Combat Recovery");
        currentState.SetIntelReaction(State1And2Reactions);
    }
    public void AfterTargetKnockedOut() {
        Character target = poiTarget as Character;
        Unconscious unconscious = new Unconscious();
        AddTraitTo(target, unconscious, actor);
    }
    public void PreTargetKilled() {
        //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is a Murder crime
        if (!actor.IsHostileWith(poiTarget as Character)
            //Assaulting a criminal as part of apprehending him should not be considered a crime
            && (parentPlan.job == null || parentPlan.job.name != "Apprehend")) { 
            SetCommittedCrime(CRIME.MURDER);
        }
        //currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        AddTraitTo(actor, "Combat Recovery");
        currentState.SetIntelReaction(State3Reactions);
    }
    public void AfterTargetKilled() {
        if (parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        Character target = poiTarget as Character;
        target.Death();
    }
    //public void PreTargetMissing() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    #endregion

    #region Intel Reactions
    private List<string> State1And2Reactions(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        //Recipient and Target have a negative relationship:
        if (recipient.HasRelationshipOfEffectWith(target, TRAIT_EFFECT.NEGATIVE)) {
            //- **Recipient Response Text**: "[Target Name] deserves that!"
            reactions.Add(string.Format("{0} deserves that!", target.name));
            //-**Recipient Effect**: no effect
        }

        //Recipient and Actor are from the same faction and they dont have a positive relationship. 
        else if (recipient.faction == actor.faction && recipient != actor && !recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)
            && !actor.IsHostileWith(target)) {
            //Target is not considered Hostile to Recipient and Actor's faction:
            //- **Recipient Response Text**: "[Actor Name] committed an assault!?"
            reactions.Add(string.Format("{0} committed an assault!?", actor.name));
            //-**Recipient Effect**:  Apply Crime System handling as if the Recipient witnessed Actor commit an Assault.
            recipient.ReactToCrime(CRIME.ASSAULT, actor, null, false);
        }

        //Recipient and Actor are from the same faction and they have a positive relationship:
        else if (recipient.faction == actor.faction && recipient != actor && recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "I'm sure there's a reason [Actor Name] did that."
            reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
            //-**Recipient Effect * *: no effect
        }

        //Recipient and Actor are from the same faction and they dont have a positive relationship. Target is considered Hostile to Recipient and Actor's faction:
        else if (recipient.faction == actor.faction && recipient != actor && !recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)
            && actor.IsHostileWith(target)) {
            //- **Recipient Response Text**: "I'm sure there's a reason [Actor Name] did that."
            reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
            //-**Recipient Effect * *: no effect
        }

        //Recipient and Target have a positive relationship:
        else if (recipient.HasRelationshipOfEffectWith(target, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "Poor [Target Name]! I hope [he/she]'s okay."
            reactions.Add(string.Format("Poor {0}! I hope {1}'s okay.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            //-**Recipient Effect**: no effect
        }

        //Recipient and Actor are the same
        else if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I've done!"
            reactions.Add(string.Format("I know what I've done!", actor.name));
            //-**Recipient Effect**:  no effect
        }
        return reactions;
    }
    private List<string> State3Reactions(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        //Recipient and Target have a positive relationship:
        if (recipient.HasRelationshipOfEffectWith(target, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "That despicable [Actor Name] killed [Target Name]! [He/She] is a murderer!"
            reactions.Add(string.Format("That despicable {0} killed {1}, {2} is a murderer!", actor.name, target.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            //-**Recipient Effect**: Remove any positive relationships between Actor and Recipient. Add Enemy relationship if they are not yet enemies. Apply Crime System handling as if the Recipient witnessed Actor commit a Murder.
            recipient.ReactToCrime(CRIME.MURDER, actor, null, false); //removal of relationships should be handled by crime system
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
            recipient.ReactToCrime(CRIME.MURDER, actor, null, false);
        }

        //Recipient and Actor are from the same faction and they dont have a positive relationship. 
        else if (recipient.faction == actor.faction && !recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            //Target is considered Hostile to Recipient and Actor's faction:
            if (actor.IsHostileWith(target)) {
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
        //Recipient and Actor are the same
        else if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I've done!"
            reactions.Add(string.Format("I know what I've done!", actor.name));
            //-**Recipient Effect**:  no effect
        }
        return reactions;
    }
    #endregion
}
