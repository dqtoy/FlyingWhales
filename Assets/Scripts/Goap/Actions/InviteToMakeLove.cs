using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InviteToMakeLove : GoapAction {

    public InviteToMakeLove(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.INVITE_TO_MAKE_LOVE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        Character targetCharacter = poiTarget as Character;
        if (!isTargetMissing && targetCharacter.IsInOwnParty()) {
            if (actor is SeducerSummon) {
                SeducerSummon seducer = actor as SeducerSummon;
                if (UnityEngine.Random.Range(0, 100) < seducer.seduceChance) {
                    SetState("Invite Success");
                } else {
                    SetState("Invite Fail");
                }
            } else {
                if (!targetCharacter.isStarving && !targetCharacter.isExhausted
                && targetCharacter.GetNormalTrait("Annoyed") == null && !targetCharacter.HasOtherCharacterInParty()
                && targetCharacter.stateComponent.currentState == null) {
                    SetState("Invite Success");
                } else {
                    SetState("Invite Fail");
                }
            }
            
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        TIME_IN_WORDS currentTime = GameManager.GetCurrentTimeInWordsOfTick();
        if (currentTime == TIME_IN_WORDS.EARLY_NIGHT || currentTime == TIME_IN_WORDS.LATE_NIGHT)
        {
            return Utilities.rng.Next(15, 36);
        }
        return Utilities.rng.Next(30, 56);
    }
    #endregion

    #region Effects
    private void PreInviteSuccess() {
        resumeTargetCharacterState = false;
        Character target = poiTarget as Character;
        List<TileObject> validBeds = new List<TileObject>();
        if (actor is SeducerSummon) {
            //target a bed at the inn
            validBeds.AddRange(target.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.INN).GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
        } else {
            bool isParamour = actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR);
            //if the characters are paramours
            if (isParamour) {
                //check if they have lovers
                bool actorHasLover = actor.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER, false);
                bool targetHasLover = target.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER, false);
                //if one of them doesn't have any lovers
                if (!actorHasLover || !targetHasLover) {
                    Character characterWithoutLover;
                    if (!actorHasLover) {
                        characterWithoutLover = actor;
                    } else {
                        characterWithoutLover = target;
                    }
                    //pick the bed of the character that doesn't have a lover
                    validBeds.AddRange(characterWithoutLover.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
                }
                //if both of them have lovers
                else {
                    //check if any of their lovers are currently at the structure that their bed is at
                    //if they are not, add that bed to the choices
                    Character actorLover = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
                    if (actorLover.currentStructure != actor.homeStructure) {
                        //lover is not at home structure, add bed to valid choices
                        validBeds.AddRange(actor.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
                    }
                    Character targetLover = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
                    if (targetLover.currentStructure != target.homeStructure) {
                        //lover is not at home structure, add bed to valid choices
                        validBeds.AddRange(target.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
                    }
                }
            } else {
                validBeds.AddRange(actor.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
            }
        }

        

        //if no beds are valid from the above logic.
        if (validBeds.Count == 0) {
            //pick a random bed in a structure that is unowned (No residents)
            List<LocationStructure> unownedStructures = actor.homeArea.GetStructuresAtLocation(true).Where(x => (x is Dwelling && (x as Dwelling).residents.Count == 0)
            || x.structureType == STRUCTURE_TYPE.INN).ToList();
            for (int i = 0; i < unownedStructures.Count; i++) {
                validBeds.AddRange(unownedStructures[i].GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
            }
        }
        IPointOfInterest chosenBed = validBeds[Random.Range(0, validBeds.Count)];

        MakeLove makeLove = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.MAKE_LOVE, actor, chosenBed, false) as MakeLove;
        makeLove.SetTargetCharacter(target);
        makeLove.Initialize();
        GoapNode startNode = new GoapNode(null, makeLove.cost, makeLove);
        GoapPlan makeLovePlan = new GoapPlan(startNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY }, GOAP_CATEGORY.HAPPINESS);
        makeLovePlan.ConstructAllNodes();
        actor.AddPlan(makeLovePlan, true);
        AddTraitTo(target, "Wooed", actor);
        actor.ownParty.AddCharacter(target);
        target.marker.PlayIdle();
        target.marker.Rotate(Quaternion.Euler(0f, 0f, 0f), true);
        currentState.SetIntelReaction(InviteSuccessReactions);
    }
    private void PreInviteFail() {
        if (actor is SeducerSummon) {
            //Start Combat between actor and target
            Character target = poiTarget as Character;
            target.marker.AddHostileInRange(actor, CHARACTER_STATE.COMBAT);
        } else {
            //**After Effect 1**: Actor gains Annoyed trait.
            AddTraitTo(actor, "Annoyed", actor);
        }
        currentState.SetIntelReaction(InviteFailReactions);
    }
    #endregion

    #region Requirements
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
        if (target.stateComponent.currentState is CombatState) { //do not invite characters that are currently in combat
            return false;
        }
        if (!(actor is SeducerSummon)) { //ignore relationships if succubus
            if (!actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER) && !actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR)) {
                return false; //only lovers and paramours can make love
            }
        }
        return target.IsInOwnParty();
    }
    #endregion

    #region Intel Reactions
    private List<string> InviteSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;
        Character actorLover = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Character targetLover = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Character actorParamour = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);
        Character targetParamour = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);

        if (isOldNews) {
            reactions.Add("This is old news.");
        } else {
            //- Recipient is the Actor
            if (recipient == actor) {
                if (targetLover == recipient) {
                    reactions.Add("That's private!");
                } else if (targetParamour == recipient) {
                    reactions.Add("Don't tell anyone. *wink**wink*");
                }
            }
            //- Recipient is the Target
            else if (recipient == target) {
                if (actorLover == recipient) {
                    reactions.Add("That's private!");
                } else if (actorParamour == recipient) {
                    reactions.Add("Don't you dare judge me!");
                }
            }
            //- Recipient is Actor's Lover
            else if (recipient == actorLover) {
                string response = string.Empty;
                if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                    response = string.Format("I've had enough of {0}'s shenanigans!", actor.name);
                    recipient.CreateUndermineJobOnly(actor, "informed", status);
                } else {
                    response = string.Format("I'm still the one {0} comes home to.", actor.name);
                }
                if (recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR)) {
                    if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                        response += string.Format(" {0} seduced both of us. {1} must pay for this.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                        recipient.CreateUndermineJobOnly(target, "informed", status);
                    } else {
                        response += string.Format(" I already know that {0} is a harlot.", target.name);
                    }
                } else if (recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.RELATIVE)) {
                    if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                        response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false));
                        recipient.CreateUndermineJobOnly(target, "informed", status);
                    } else {
                        response += string.Format(" {0} is my blood. Blood is thicker than water.", target.name);
                    }
                } else if (recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.FRIEND)) {
                    if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                        response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false));
                        recipient.CreateUndermineJobOnly(target, "informed", status);
                    } else {
                        response += string.Format(" My friendship with {0} is much stronger than this incident.", target.name);
                    }
                } else if (recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.ENEMY)) {
                    response += string.Format(" I always knew that {0} is a snake. {1} must pay for this!", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                    recipient.CreateUndermineJobOnly(target, "informed", status);
                } else if (!recipient.HasRelationshipWith(target)) {
                    if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                        response += string.Format(" {0} is a snake. {1} must pay for this!", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                        recipient.CreateUndermineJobOnly(target, "informed", status);
                    } else {
                        response += string.Format(" I'm not even going to bother myself with {0}.", target.name);
                    }
                }
                reactions.Add(response);
            }
            //- Recipient is Target's Lover
            else if (recipient == targetLover) {
                string response = string.Empty;
                if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                    response = string.Format("I've had enough of {0}'s shenanigans!", target.name);
                    recipient.CreateUndermineJobOnly(target, "informed", status);
                } else {
                    response = string.Format("I'm still the one {0} comes home to.", target.name);
                }
                if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.PARAMOUR)) {
                    if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        response += string.Format(" {0} seduced both of us. {1} must pay for this.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else {
                        response += string.Format(" I already know that {0} is a harlot.", actor.name);
                    }
                } else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.RELATIVE)) {
                    if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else {
                        response += string.Format(" {0} is my blood. Blood is thicker than water.", actor.name);
                    }
                } else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.FRIEND)) {
                    if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else {
                        response += string.Format(" My friendship with {0} is much stronger than this incident.", actor.name);
                    }
                } else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
                    response += string.Format(" I always knew that {0} is a snake. {1} must pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                    recipient.CreateUndermineJobOnly(actor, "informed", status);
                } else if (!recipient.HasRelationshipWith(actor)) {
                    if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        response += string.Format(" {0} is a snake. {1} must pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else {
                        response += string.Format(" I'm not even going to bother myself with {0}.", actor.name);
                    }
                }
                reactions.Add(response);
            }
            //- Recipient is Actor/Target's Paramour
            else if (recipient == actorParamour || recipient == targetParamour) {
                reactions.Add("I have no right to complain. Bu..but I wish that we could be like that.");
                AddTraitTo(recipient, "Heartbroken");
            }
            //- Recipient has a positive relationship with Actor's Lover and Actor's Lover is not the Target
            else if (actorLover != null && recipient.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.POSITIVE && actorLover != target) {
                if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                    reactions.Add(string.Format("{0} is cheating on {1}?! I must let {2} know.", actor.name, actorLover.name, Utilities.GetPronounString(actorLover.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    recipient.CreateShareInformationJob(actorLover, this);
                } else {
                    reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", actor.name, actorLover.name));
                }
            }
            //- Recipient has a positive relationship with Target's Lover and Target's Lover is not the Actor
            else if (targetLover != null && recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.POSITIVE && targetLover != actor) {
                if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                    reactions.Add(string.Format("{0} is cheating on {1}?! I must let {2} know.", target.name, targetLover.name, Utilities.GetPronounString(targetLover.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    recipient.CreateShareInformationJob(targetLover, this);
                } else {
                    reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", target.name, targetLover.name));
                }
            }
            //- Recipient has a negative relationship with Actor's Lover and Actor's Lover is not the Target
            else if (actorLover != null && recipient.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NEGATIVE && actorLover != target) {
                reactions.Add(string.Format("{0} is cheating on {1}? {2} got what {3} deserves.", actor.name, actorLover.name, Utilities.GetPronounString(actorLover.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actorLover.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            }
            //- Recipient has a negative relationship with Target's Lover and Target's Lover is not the Actor
            else if (targetLover != null && recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NEGATIVE && targetLover != actor) {
                reactions.Add(string.Format("{0} is cheating on {1}? {2} got what {3} deserves.", actor.name, targetLover.name, Utilities.GetPronounString(targetLover.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(targetLover.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            }
            //- Recipient has a no relationship with Actor's Lover and Actor's Lover is not the Target
            else if (actorLover != null && recipient.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NONE && actorLover != target) {
                reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", actor.name, actorLover.name));
                CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);
            }
            //- Recipient has no relationship with Target's Lover and Target's Lover is not the Actor
            else if (targetLover != null && recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NONE && targetLover != actor) {
                reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", target.name, targetLover.name));
                CharacterManager.Instance.RelationshipDegradation(target, recipient, this);
            }
            //- Else Catcher
            else {
                reactions.Add("That is none of my business.");
            }
        }
        return reactions;
    }
    private List<string> InviteFailReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;
        Character actorLover = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Character targetLover = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Character actorParamour = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);
        Character targetParamour = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);

        if (isOldNews) {
            reactions.Add("This is old news.");
        } else {
            //- Recipient is the Actor
            if (recipient == actor) {
                if (targetLover == recipient) {
                    reactions.Add("That's private!");
                } else if (targetParamour == recipient) {
                    reactions.Add("Don't tell anyone. *wink**wink*");
                }
            }
            //- Recipient is the Target
            else if (recipient == target) {
                if (actorLover == recipient) {
                    reactions.Add("I wasn't in the mood.");
                } else if (actorParamour == recipient) {
                    reactions.Add("I wasn't in the mood.");
                }
            }
            //- Recipient is Actor's Lover
            else if (recipient == actorLover) {
                if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                    reactions.Add(string.Format("I've had enough of {0}'s shenanigans!", actor.name));
                    recipient.CreateUndermineJobOnly(actor, "informed", status);
                } else {
                    reactions.Add("I'm relieved it didn't push through.");
                }
            }
            //- Recipient is Target's Lover
            else if (recipient == targetLover) {
                if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                    reactions.Add(string.Format("I've had enough of {0}'s shenanigans!", target.name));
                    recipient.CreateUndermineJobOnly(target, "informed", status);
                } else {
                    reactions.Add("I'm relieved it didn't push through.");
                }
            }
            //- Recipient has a positive relationship with Actor's Lover and Actor's Lover is not the Target
            else if (actorLover != null && recipient.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.POSITIVE && actorLover != target) {
                if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                    reactions.Add(string.Format("{0} wanted to cheat on {1}?! I don't like it one bit.", actor.name, actorLover.name));
                } else {
                    reactions.Add(string.Format("{0} wanted to cheat on {1}?! I always knew it.", actor.name, actorLover.name));
                }
            }
            //- Recipient has a positive relationship with Target's Lover and Target's Lover is not the Actor
            else if (targetLover != null && recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.POSITIVE && targetLover != actor) {
                if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                    reactions.Add(string.Format("{0} wanted to cheat on {1}?! I don't like it one bit.", target.name, targetLover.name));
                } else {
                    reactions.Add(string.Format("{0} wanted to cheat on {1}?! I always knew it.", target.name, targetLover.name));
                }
            }
            //- Recipient has a negative relationship with Actor's Lover and Actor's Lover is not the Target
            else if (actorLover != null && recipient.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NEGATIVE && actorLover != target) {
                reactions.Add(string.Format("{0} wanted to cheat on {1}? {2} got what {3} deserves.", actor.name, actorLover.name, Utilities.GetPronounString(actorLover.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actorLover.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            }
            //- Recipient has a negative relationship with Target's Lover and Target's Lover is not the Actor
            else if (targetLover != null && recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NEGATIVE && targetLover != actor) {
                reactions.Add(string.Format("{0} wanted to cheat on {1}? {2} got what {3} deserves.", target.name, targetLover.name, Utilities.GetPronounString(targetLover.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(targetLover.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            }
            //- Recipient has a no relationship with Actor's Lover and Actor's Lover is not the Target
            else if (actorLover != null && recipient.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NONE && actorLover != target) {
                reactions.Add(string.Format("{0} wanted to cheat on {1}? I don't want to get involved.", actor.name, actorLover.name));
            }
            //- Recipient has no relationship with Target's Lover and Target's Lover is not the Actor
            else if (targetLover != null && recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NONE && targetLover != actor) {
                reactions.Add(string.Format("{0} wanted to cheat on {1}? I don't want to get involved.", target.name, targetLover.name));
            }
            //- Else Catcher
            else {
                reactions.Add("That is none of my business.");
            }
        }
        return reactions;
    }
    #endregion
}
