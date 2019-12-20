using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;  
using Traits;

public class Invite : GoapAction {

    public Invite() : base(INTERACTION_TYPE.INVITE) {
        actionIconString = GoapActionStateDB.Flirt_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.DEMON };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.INVITED, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Invite Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return 1;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = poiTarget as Character;
            if (actor is SeducerSummon) {
                SeducerSummon seducer = actor as SeducerSummon;
                if (UnityEngine.Random.Range(0, 100) > seducer.seduceChance || targetCharacter.ownParty.isCarryingAnyPOI
                     || targetCharacter.stateComponent.currentState != null || targetCharacter.IsAvailable() == false) {
                    goapActionInvalidity.isInvalid = true;
                    goapActionInvalidity.stateName = "Invite Fail";
                }
            } else {
                int acceptChance = 100;
                if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Chaste") != null) {
                    acceptChance = 25;
                }
                if (UnityEngine.Random.Range(0, 100) > acceptChance || targetCharacter.needsComponent.isStarving || targetCharacter.needsComponent.isExhausted
                || targetCharacter.traitContainer.GetNormalTrait<Trait>("Annoyed") != null || targetCharacter.ownParty.isCarryingAnyPOI
                || targetCharacter.stateComponent.currentState != null || targetCharacter.IsAvailable() == false) {
                    goapActionInvalidity.isInvalid = true;
                    goapActionInvalidity.stateName = "Invite Fail";
                }
            }
        }
        return goapActionInvalidity;
    }
    public override void OnInvalidAction(ActualGoapNode node) {
        base.OnInvalidAction(node);
        if (node.actor is SeducerSummon) {
            Character target = node.poiTarget as Character;
            target.marker.AddHostileInRange(node.actor, false);
        }
    }
    #endregion

    #region Effects
    public void PreInviteSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Wooed", goapNode.actor);
        goapNode.actor.ownParty.AddPOI(goapNode.poiTarget);
    }
    //public void PreInviteFail(ActualGoapNode goapNode) {
    //    currentState.SetIntelReaction(InviteFailReactions);
    //}
    public void AfterInviteFail(ActualGoapNode goapNode) {
        if (goapNode.actor is SeducerSummon) {
            //Start Combat between actor and target
            Character target = goapNode.poiTarget as Character;
            target.marker.AddHostileInRange(goapNode.actor, false);
        } else {
            //**After Effect 1**: Actor gains Annoyed trait.
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Annoyed");
        }
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            Character target = poiTarget as Character;
            if (target == actor) {
                return false;
            }
            if (target.currentAlterEgoName != CharacterManager.Original_Alter_Ego) { //do not woo characters that have transformed to other alter egos
                return false;
            }
            if (target.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
                return false;
            }
            if (target.stateComponent.currentState is CombatState) { //do not invite characters that are currently in combat
                return false;
            }
            if (target.returnedToLife) { //do not woo characters that have been raised from the dead
                return false;
            }
            if (target.currentParty.icon.isTravellingOutside || target.currentRegion != actor.currentRegion) {
                return false; //target is outside the map
            }
            return target.IsInOwnParty();
        }
        return false;
    }
    #endregion

    //#region Intel Reactions
    //private List<string> InviteSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character target = poiTarget as Character;
    //    Relatable actorLover = actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER);
    //    Relatable targetLover = target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER);
    //    Relatable actorParamour = actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);
    //    Relatable targetParamour = target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);

    //    if (isOldNews) {
    //        reactions.Add("This is old news.");
    //    } else {
    //        //- Recipient is the Actor
    //        if (recipient == actor) {
    //            if (targetLover == recipient.currentAlterEgo) {
    //                reactions.Add("That's private!");
    //            } else if (targetParamour == recipient.currentAlterEgo) {
    //                reactions.Add("Don't tell anyone. *wink**wink*");
    //            }
    //        }
    //        //- Recipient is the Target
    //        else if (recipient == target) {
    //            if (actorLover == recipient.currentAlterEgo) {
    //                reactions.Add("That's private!");
    //            } else if (actorParamour == recipient.currentAlterEgo) {
    //                reactions.Add("Don't you dare judge me!");
    //            }
    //        }
    //        //- Recipient is Actor's Lover
    //        else if (recipient.currentAlterEgo == actorLover) {
    //            string response = string.Empty;
    //            if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                response = string.Format("I've had enough of {0}'s shenanigans!", actor.name);
    //                recipient.CreateUndermineJobOnly(actor, "informed", status);
    //            } else {
    //                response = string.Format("I'm still the one {0} comes home to.", actor.name);
    //            }
    //            if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                    response += string.Format(" {0} seduced both of us. {1} must pay for this.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                    recipient.CreateUndermineJobOnly(target, "informed", status);
    //                } else {
    //                    response += string.Format(" I already know that {0} is a harlot.", target.name);
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.RELATIVE)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                    response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false));
    //                    recipient.CreateUndermineJobOnly(target, "informed", status);
    //                } else {
    //                    response += string.Format(" {0} is my blood. Blood is thicker than water.", target.name);
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                    response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false));
    //                    recipient.CreateUndermineJobOnly(target, "informed", status);
    //                } else {
    //                    response += string.Format(" My friendship with {0} is much stronger than this incident.", target.name);
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //                response += string.Format(" I always knew that {0} is a snake. {1} must pay for this!", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                recipient.CreateUndermineJobOnly(target, "informed", status);
    //            } else if (!recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                    response += string.Format(" {0} is a snake. {1} must pay for this!", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                    recipient.CreateUndermineJobOnly(target, "informed", status);
    //                } else {
    //                    response += string.Format(" I'm not even going to bother myself with {0}.", target.name);
    //                }
    //            }
    //            reactions.Add(response);
    //        }
    //        //- Recipient is Target's Lover
    //        else if (recipient.currentAlterEgo == targetLover) {
    //            string response = string.Empty;
    //            if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                response = string.Format("I've had enough of {0}'s shenanigans!", target.name);
    //                recipient.CreateUndermineJobOnly(target, "informed", status);
    //            } else {
    //                response = string.Format("I'm still the one {0} comes home to.", target.name);
    //            }
    //            if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    response += string.Format(" {0} seduced both of us. {1} must pay for this.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                    recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                } else {
    //                    response += string.Format(" I already know that {0} is a harlot.", actor.name);
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.RELATIVE)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false));
    //                    recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                } else {
    //                    response += string.Format(" {0} is my blood. Blood is thicker than water.", actor.name);
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false));
    //                    recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                } else {
    //                    response += string.Format(" My friendship with {0} is much stronger than this incident.", actor.name);
    //                }
    //            } else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //                response += string.Format(" I always knew that {0} is a snake. {1} must pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                recipient.CreateUndermineJobOnly(actor, "informed", status);
    //            } else if (!recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo)) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    response += string.Format(" {0} is a snake. {1} must pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
    //                    recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                } else {
    //                    response += string.Format(" I'm not even going to bother myself with {0}.", actor.name);
    //                }
    //            }
    //            reactions.Add(response);
    //        }
    //        //- Recipient is Actor/Target's Paramour
    //        else if (recipient.currentAlterEgo == actorParamour || recipient.currentAlterEgo == targetParamour) {
    //            reactions.Add("I have no right to complain. Bu..but I wish that we could be like that.");
    //            AddTraitTo(recipient, "Heartbroken");
    //        }
    //        //- Recipient has a positive relationship with Actor's Lover and Actor's Lover is not the Target
    //        else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.POSITIVE && actorLover != target.currentAlterEgo) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                AlterEgoData actorEgo = actorLover as AlterEgoData;
    //                reactions.Add(string.Format("{0} is cheating on {1}?! I must let {2} know.", actor.name, actorLover.relatableName, Utilities.GetPronounString(actorEgo.owner.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                recipient.CreateShareInformationJob(actorEgo.owner, this);
    //            } else {
    //                reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", actor.name, actorLover.relatableName));
    //            }
    //        }
    //        //- Recipient has a positive relationship with Target's Lover and Target's Lover is not the Actor
    //        else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.POSITIVE && targetLover != actor.currentAlterEgo) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                AlterEgoData targetEgo = actorLover as AlterEgoData;
    //                reactions.Add(string.Format("{0} is cheating on {1}?! I must let {2} know.", target.name, targetLover.relatableName, Utilities.GetPronounString(targetEgo.owner.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                recipient.CreateShareInformationJob(targetEgo.owner, this);
    //            } else {
    //                reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", target.name, targetLover.relatableName));
    //            }
    //        }
    //        //- Recipient has a negative relationship with Actor's Lover and Actor's Lover is not the Target
    //        else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NEGATIVE && actorLover != target.currentAlterEgo) {
    //            AlterEgoData actorEgo = actorLover as AlterEgoData;
    //            reactions.Add(string.Format("{0} is cheating on {1}? {2} got what {3} deserves.", actor.name, actorLover.relatableName, Utilities.GetPronounString(actorEgo.owner.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actorEgo.owner.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //        }
    //        //- Recipient has a negative relationship with Target's Lover and Target's Lover is not the Actor
    //        else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NEGATIVE && targetLover != actor.currentAlterEgo) {
    //            AlterEgoData targetEgo = targetLover as AlterEgoData;
    //            reactions.Add(string.Format("{0} is cheating on {1}? {2} got what {3} deserves.", actor.name, targetLover.relatableName, Utilities.GetPronounString(targetEgo.owner.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(targetEgo.owner.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //        }
    //        //- Recipient has a no relationship with Actor's Lover and Actor's Lover is not the Target
    //        else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NONE && actorLover != target.currentAlterEgo) {
    //            reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", actor.name, actorLover.relatableName));
    //            RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //        }
    //        //- Recipient has no relationship with Target's Lover and Target's Lover is not the Actor
    //        else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NONE && targetLover != actor.currentAlterEgo) {
    //            reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", target.name, targetLover.relatableName));
    //            RelationshipManager.Instance.RelationshipDegradation(target, recipient, this);
    //        }
    //        //- Else Catcher
    //        else {
    //            reactions.Add("That is none of my business.");
    //        }
    //    }
    //    return reactions;
    //}
    //private List<string> InviteFailReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character target = poiTarget as Character;
    //    Relatable actorLover = actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER);
    //    Relatable targetLover = target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER);
    //    Relatable actorParamour = actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);
    //    Relatable targetParamour = target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);

    //    if (isOldNews) {
    //        reactions.Add("This is old news.");
    //    } else {
    //        //- Recipient is the Actor
    //        if (recipient == actor) {
    //            if (targetLover == recipient.currentAlterEgo) {
    //                reactions.Add("That's private!");
    //            } else if (targetParamour == recipient.currentAlterEgo) {
    //                reactions.Add("Don't tell anyone. *wink**wink*");
    //            }
    //        }
    //        //- Recipient is the Target
    //        else if (recipient == target) {
    //            if (actorLover == recipient.currentAlterEgo) {
    //                reactions.Add("I wasn't in the mood.");
    //            } else if (actorParamour == recipient.currentAlterEgo) {
    //                reactions.Add("I wasn't in the mood.");
    //            }
    //        }
    //        //- Recipient is Actor's Lover
    //        else if (recipient.currentAlterEgo == actorLover) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                reactions.Add(string.Format("I've had enough of {0}'s shenanigans!", actor.name));
    //                recipient.CreateUndermineJobOnly(actor, "informed", status);
    //            } else {
    //                reactions.Add("I'm relieved it didn't push through.");
    //            }
    //        }
    //        //- Recipient is Target's Lover
    //        else if (recipient.currentAlterEgo == targetLover) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                reactions.Add(string.Format("I've had enough of {0}'s shenanigans!", target.name));
    //                recipient.CreateUndermineJobOnly(target, "informed", status);
    //            } else {
    //                reactions.Add("I'm relieved it didn't push through.");
    //            }
    //        }
    //        //- Recipient has a positive relationship with Actor's Lover and Actor's Lover is not the Target
    //        else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.POSITIVE && actorLover != target.currentAlterEgo) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                reactions.Add(string.Format("{0} wanted to cheat on {1}?! I don't like it one bit.", actor.name, actorLover.relatableName));
    //            } else {
    //                reactions.Add(string.Format("{0} wanted to cheat on {1}?! I always knew it.", actor.name, actorLover.relatableName));
    //            }
    //        }
    //        //- Recipient has a positive relationship with Target's Lover and Target's Lover is not the Actor
    //        else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.POSITIVE && targetLover != actor.currentAlterEgo) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                reactions.Add(string.Format("{0} wanted to cheat on {1}?! I don't like it one bit.", target.name, targetLover.relatableName));
    //            } else {
    //                reactions.Add(string.Format("{0} wanted to cheat on {1}?! I always knew it.", target.name, targetLover.relatableName));
    //            }
    //        }
    //        //- Recipient has a negative relationship with Actor's Lover and Actor's Lover is not the Target
    //        else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NEGATIVE && actorLover != target.currentAlterEgo) {
    //            AlterEgoData ego = actorLover as AlterEgoData;
    //            reactions.Add(string.Format("{0} wanted to cheat on {1}? {2} got what {3} deserves.", actor.name, actorLover.relatableName, Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //        }
    //        //- Recipient has a negative relationship with Target's Lover and Target's Lover is not the Actor
    //        else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NEGATIVE && targetLover != actor.currentAlterEgo) {
    //            AlterEgoData ego = actorLover as AlterEgoData;
    //            reactions.Add(string.Format("{0} wanted to cheat on {1}? {2} got what {3} deserves.", target.name, targetLover.relatableName, Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //        }
    //        //- Recipient has a no relationship with Actor's Lover and Actor's Lover is not the Target
    //        else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NONE && actorLover != target.currentAlterEgo) {
    //            reactions.Add(string.Format("{0} wanted to cheat on {1}? I don't want to get involved.", actor.name, actorLover.relatableName));
    //        }
    //        //- Recipient has no relationship with Target's Lover and Target's Lover is not the Actor
    //        else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NONE && targetLover != actor.currentAlterEgo) {
    //            reactions.Add(string.Format("{0} wanted to cheat on {1}? I don't want to get involved.", target.name, targetLover.relatableName));
    //        }
    //        //- Else Catcher
    //        else {
    //            reactions.Add("That is none of my business.");
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class InviteData : GoapActionData {
    public InviteData() : base(INTERACTION_TYPE.INVITE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
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
        if (target.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
            return false;
        }
        if (target.stateComponent.currentState is CombatState) { //do not invite characters that are currently in combat
            return false;
        }
        return target.IsInOwnParty();
    }
}
