using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveFromPartyReaction : ActionEffectReaction {

    public override string GetReactionFrom(Character character, Intel intel, GoapEffect effect) {
        EventIntel ei = intel as EventIntel;
        if (effect.conditionString() == ei.actor.homeArea.name) {
            //If not yet there, add actor and target to awareness list of character.
            character.AddAwareness(ei.actor);
            character.AddAwareness(ei.target);

            Character targetCharacter = ei.target as Character;

            GoapPlan plan = ei.plan;
            bool justAddedPlan = false;
            CharacterAwareness actorAwareness = character.GetAwareness(ei.actor) as CharacterAwareness;
            if (!actorAwareness.knownPlans.Contains(plan)) {
                actorAwareness.AddKnownPlan(plan);
                justAddedPlan = true;
            }

            if (character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.ENEMY)) {
                //If the target is an enemy of abducted or restrained character: 
                if (justAddedPlan) {
                    return string.Format("{0} is in trouble? {1} deserves it.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.REFLEXIVE, false));
                } else {
                    return string.Format("So {0} really did get abducted? {1} deserves it.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.REFLEXIVE, false));
                }
            } else if (character.HasRelationshipOfEffectWith(targetCharacter, TRAIT_EFFECT.POSITIVE)) {
                if (character.characterClass.className == "Soldier" || character.characterClass.className == "Adventurer") {
                    //If the target is an enemy of abducted or restrained character: 
                    //Character will create a Release Plan. Add the target as Relevant Target.
                    character.StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Abducted" }, targetCharacter);
                    if (justAddedPlan) {
                        return string.Format("{0} is in trouble? I must save {1}.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.REFLEXIVE, false));
                    } else {
                        return string.Format("Thank you for letting me know where {0} was taken. I must save {1}.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.REFLEXIVE, false));
                    }
                } else {
                    //Character will create a Release Help Plan. Add the target as Relevant Target.
                    character.StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Abducted" }, targetCharacter);
                    if (justAddedPlan) {
                        return string.Format("{0} is in trouble? I must find someone who can save {1}.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.REFLEXIVE, false));
                    } else {
                        return string.Format("Thank you for letting me know where {0} was taken. I must find someone who can save {1}.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.REFLEXIVE, false));
                    }
                }
            } else {
                if (justAddedPlan) {
                    return "I'm not really interested in this information.";
                } else {
                    return "You've already informed me about this and I still don't care.";
                }
            }
        }
        return base.GetReactionFrom(character, intel, effect);

    }
    private string AbductReaction(Character character, Intel intel, GoapEffect effect) {
        string reaction = string.Empty;
        EventIntel ei = intel as EventIntel;
        //If not yet there, add actor and target to awareness list of character.
        character.AddAwareness(ei.actor);
        character.AddAwareness(ei.target);

        Character targetCharacter = ei.target as Character;

        GoapPlan plan = ei.plan;
        bool justAddedPlan = false;
        CharacterAwareness actorAwareness = character.GetAwareness(ei.actor) as CharacterAwareness;
        if (!actorAwareness.knownPlans.Contains(plan)) {
            actorAwareness.AddKnownPlan(plan);
            justAddedPlan = true;
        }

        //If the character is from the same faction as the actor and they do not have any positive relationships: 
        if (character.faction.id == ei.actor.faction.id && !character.HasRelationshipOfEffectWith(ei.actor, TRAIT_EFFECT.POSITIVE)) {
            if (character.faction.morality == MORALITY.GOOD ||  character.faction.morality == MORALITY.NEUTRAL) { //- If the faction is a Good or Neutral Type:
                if (character.isLeader || character.characterClass.className == "Noble" || character.characterClass.className == "Soldier") {
                    if (justAddedPlan) {
                        reaction = string.Format("{0} abducted someone? That criminal will be brought to justice!", ei.actor.name);
                    } else {
                        reaction = string.Format("So {0} really went ahead and abducted someone? That criminal will be brought to justice!", ei.actor.name);
                    }
                    //- Add Criminal trait to the Actor.
                    ei.actor.AddTrait("Criminal");
                } else if (character.characterClass.className == "Adventurer" || character.characterClass.className == "Civilian") {
                    if (justAddedPlan) {
                        reaction = string.Format("{0} abducted someone? I better report this to the authorities!", ei.actor.name);
                    } else {
                        reaction = string.Format("So {0} really went ahead and abducted someone? I better report this to the authorities!", ei.actor.name);
                    }
                    //TODO: - Create a Report Crime plan for the character.
                }
            } else if (character.faction.morality == MORALITY.EVIL) {
                if (justAddedPlan) {
                    reaction = string.Format("Didn't someone already tell me something about this already?");
                } else {
                    reaction = string.Format("{0} abducted someone? So what?", ei.actor.name);
                }
            }
        } else if (character.faction.id == ei.actor.faction.id && character.HasRelationshipOfEffectWith(ei.actor, TRAIT_EFFECT.POSITIVE)) {
            //If the character is from the same faction as the actor and they have a positive relationship: 
            if (character.faction.morality == MORALITY.GOOD ||  character.faction.morality == MORALITY.NEUTRAL) { //- If the faction is a Good or Neutral Type:
                if (justAddedPlan) {
                    // - If just added to actor's **known plans**: "[Character Name] abducted someone? I'll have to confront [him/her] about this."
                    reaction = string.Format("{0} abducted someone? I'll have to confront {1} about this.", ei.actor.name, Utilities.GetPronounString(ei.actor.gender, PRONOUN_TYPE.REFLEXIVE, false));
                } else {
                    //- If already previously in actor's **known plans**: "So [Character Name] really went ahead and abducted someone? I'll have to confront [him/her] about this."
                    reaction = string.Format("So {0} really went ahead and abducted someone? I'll have to confront {1} about this.", ei.actor.name, Utilities.GetPronounString(ei.actor.gender, PRONOUN_TYPE.REFLEXIVE, false));
                }
                //TODO: - Create an Argue plan for the character.
            } else if (character.faction.morality == MORALITY.EVIL) { //- If the faction is an Evil Type:
                if (justAddedPlan) {
                    reaction = string.Format("{0} abducted someone? Good for {1}.", ei.actor.name, Utilities.GetPronounString(ei.actor.gender, PRONOUN_TYPE.REFLEXIVE, false));
                } else {
                    reaction = string.Format("So {0} really went ahead and abducted someone? Good for {1}.", ei.actor.name, Utilities.GetPronounString(ei.actor.gender, PRONOUN_TYPE.REFLEXIVE, false));
                }
            }
        } else if (character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.ENEMY)) {
            //If the target is an enemy of abducted or restrained character: 
            if (justAddedPlan) {
                reaction = string.Format("{0} is in trouble? {1} deserves it.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.REFLEXIVE, false));
            } else {
                reaction = string.Format("So {0} really did get abducted? {1} deserves it.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.REFLEXIVE, false));
            }
        } else if (character.HasRelationshipOfEffectWith(targetCharacter, TRAIT_EFFECT.POSITIVE)) {
            //Otherwise, if the target has a positive relationship with abducted or restrained character: 
            if (justAddedPlan) {
                reaction = string.Format("{0} is in trouble? Where are they taking {1}!?", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.REFLEXIVE, false));
            } else {
                reaction = string.Format("So {0} really did get abducted? Where are they taking {1}!?", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.REFLEXIVE, false));
            }
        } else {
            //If none of the above fits:
            if (justAddedPlan) {
                //- If just added to actor's **known plans**: "[Character Name] is in trouble? Where are they taking [him/her]!?"
                reaction = string.Format("{0} is in trouble? Where are they taking {1}!?", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.REFLEXIVE, false));
            } else {
                //- If already previously in actor's **known plans**: "So [Character Name] really did get abducted? Where are they taking [him/her]!?"
                reaction = string.Format("So {0} really did get abducted? Where are they taking {1}!?", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.REFLEXIVE, false));
            }
        }
        return reaction;
    }
}
