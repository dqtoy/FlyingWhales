using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatCharacter : GoapAction {
    public string chatResult { get; private set; }

    public ChatCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CHAT_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Social_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        onlyShowNotifOfDescriptionLog = true;
    }

    #region Overrides
    public override void PerformActualAction() {
        //base.PerformActualAction();
        isPerformingActualAction = true;

        Character targetCharacter = poiTarget as Character;
        actor.SetIsChatting(true);
        targetCharacter.SetIsChatting(true);
        actor.marker.UpdateActionIcon();
        targetCharacter.marker.UpdateActionIcon();
        CHARACTER_MOOD thisCharacterMood = actor.currentMoodType;
        CHARACTER_MOOD targetCharacterMood = targetCharacter.currentMoodType;

        WeightedFloatDictionary<string> weights = new WeightedFloatDictionary<string>();
        weights.AddElement("Quick Chat", 250);

        CharacterRelationshipData relData = actor.GetCharacterRelationshipData(targetCharacter);
        RELATIONSHIP_EFFECT relationshipEffectWithTarget = actor.GetRelationshipEffectWith(targetCharacter);
        //**if no relationship yet, may become friends**
        if (relData == null) {
            int weight = 0;
            if (thisCharacterMood == CHARACTER_MOOD.DARK) {
                weight += -50;
            } else if (thisCharacterMood == CHARACTER_MOOD.BAD) {
                weight += -20;
            } else if (thisCharacterMood == CHARACTER_MOOD.GOOD) {
                weight += 20;
            } else if (thisCharacterMood == CHARACTER_MOOD.GREAT) {
                weight += 50;
            }
            if (targetCharacterMood == CHARACTER_MOOD.DARK) {
                weight += -50;
            } else if (targetCharacterMood == CHARACTER_MOOD.BAD) {
                weight += -20;
            } else if (targetCharacterMood == CHARACTER_MOOD.GOOD) {
                weight += 20;
            } else if (targetCharacterMood == CHARACTER_MOOD.GREAT) {
                weight += 50;
            }
            if (weight > 0) {
                weights.AddElement("Become Friends", weight);
            }
        } else {
            //**if no relationship other than relative, may become enemies**
            List<RelationshipTrait> relTraits = actor.GetAllRelationshipTraitWith(targetCharacter);
            if (relTraits.Count == 1 && relTraits[0] is Relative) {
                int weight = 0;
                if (thisCharacterMood == CHARACTER_MOOD.DARK) {
                    weight += 50;
                } else if (thisCharacterMood == CHARACTER_MOOD.BAD) {
                    weight += 20;
                } else if (thisCharacterMood == CHARACTER_MOOD.GOOD) {
                    weight += -20;
                } else if (thisCharacterMood == CHARACTER_MOOD.GREAT) {
                    weight += -50;
                }
                if (targetCharacterMood == CHARACTER_MOOD.DARK) {
                    weight += 50;
                } else if (targetCharacterMood == CHARACTER_MOOD.BAD) {
                    weight += 20;
                } else if (targetCharacterMood == CHARACTER_MOOD.GOOD) {
                    weight += -20;
                } else if (targetCharacterMood == CHARACTER_MOOD.GREAT) {
                    weight += -50;
                }
                if (weight > 0) {
                    weights.AddElement("Become Enemies", weight);
                }
            }

            //**if already has a positive relationship, knowledge may be transferred**
            if (relationshipEffectWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                weights.AddElement("Share Information", 200);
            }

            //**if already has a negative relationship, Argument may occur**
            if (relationshipEffectWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                weights.AddElement("Argument", 500);

                //**if already has a negative relationship, relationship may be Resolve Enmityd**
                int weight = 0;
                if (thisCharacterMood == CHARACTER_MOOD.DARK) {
                    weight += -50;
                } else if (thisCharacterMood == CHARACTER_MOOD.BAD) {
                    weight += -20;
                } else if (thisCharacterMood == CHARACTER_MOOD.GOOD) {
                    weight += 20;
                } else if (thisCharacterMood == CHARACTER_MOOD.GREAT) {
                    weight += 50;
                }
                if (targetCharacterMood == CHARACTER_MOOD.DARK) {
                    weight += -50;
                } else if (targetCharacterMood == CHARACTER_MOOD.BAD) {
                    weight += -20;
                } else if (targetCharacterMood == CHARACTER_MOOD.GOOD) {
                    weight += 20;
                } else if (targetCharacterMood == CHARACTER_MOOD.GREAT) {
                    weight += 50;
                }
                if (weight > 0) {
                    weights.AddElement("Resolve Enmity", weight);
                }
            }
        }

        //Flirtation
        float FlirtationWeight = actor.GetFlirtationWeightWith(targetCharacter, relData, thisCharacterMood, targetCharacterMood);
        if (FlirtationWeight > 0f) {
            weights.AddElement("Flirt", FlirtationWeight);
        }

        //Become Lovers weight
        float becomeLoversWeight = actor.GetBecomeLoversWeightWith(targetCharacter, relData, thisCharacterMood, targetCharacterMood);
        if (becomeLoversWeight > 0f) {
            weights.AddElement("Become Lovers", becomeLoversWeight);
        }

        //Become Paramours
        float becomeParamoursWeight = actor.GetBecomeParamoursWeightWith(targetCharacter, relData, thisCharacterMood, targetCharacterMood);
        if (becomeParamoursWeight > 0f) {
            weights.AddElement("Become Paramours", becomeParamoursWeight);
        }

        chatResult = weights.PickRandomElementGivenWeights();
        if (chatResult == "Become Friends") {
            //may become friends
            CharacterManager.Instance.CreateNewRelationshipBetween(actor, targetCharacter, RELATIONSHIP_TRAIT.FRIEND);
        } else if (chatResult == "Become Enemies") {
            //may become enemies
            CharacterManager.Instance.CreateNewRelationshipBetween(actor, targetCharacter, RELATIONSHIP_TRAIT.ENEMY);
        } else if (chatResult == "Share Information") {
            //TODO: mechanics to be added later
        } else if (chatResult == "Resolve Enmity") {
            //relationship may be resolved
            List<RelationshipTrait> negativeTraits = actor.GetAllRelationshipOfEffectWith(targetCharacter, TRAIT_EFFECT.NEGATIVE);
            RelationshipTrait chosenTrait = negativeTraits[UnityEngine.Random.Range(0, negativeTraits.Count)];
            CharacterManager.Instance.RemoveOneWayRelationship(actor, targetCharacter, chosenTrait.relType);
        } else if (chatResult == "Flirt") {
            //store flirtation count in both characters
            //Log: "[Character Name 1] and [Character Name 2] flirted."
            actor.FlirtWith(targetCharacter);
        } else if (chatResult == "Become Lovers") {
            //Log: "[Character Name 1] and [Character Name 2] have become lovers."
            CharacterManager.Instance.CreateNewRelationshipBetween(actor, targetCharacter, RELATIONSHIP_TRAIT.LOVER);
        } else if (chatResult == "Become Paramours") {
            //Log: "[Character Name 1] and [Character Name 2] have developed an affair!"
            CharacterManager.Instance.CreateNewRelationshipBetween(actor, targetCharacter, RELATIONSHIP_TRAIT.PARAMOUR);
        }

        //Log chatLog = new Log(GameManager.Instance.Today(), "GoapAction", "ChatCharacter", chatResult.ToLower(), this);
        //chatLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //chatLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        ////chatLog.AddLogToInvolvedObjects();
        //currentState.OverrideDescriptionLog(chatLog);

        //if (!PlayerManager.Instance.player.ShowNotificationFrom(actor, chatLog)) {
        //    PlayerManager.Instance.player.ShowNotificationFrom(targetCharacter, chatLog);
        //}

        weights.LogDictionaryValues("Chat Weights of " + actor.name + " and " + targetCharacter.name);
        Debug.Log(actor.name + " and " + targetCharacter.name + "'s chat result is " + chatResult);

        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(2);
        SchedulingManager.Instance.AddEntry(dueDate, () => actor.EndChatCharacter(targetCharacter));

        SetState(chatResult);
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region State Effects
    private void AfterQuickChat() {
        currentState.SetIntelReaction(QuickChatIntelReaction);
    }
    private void AfterFlirt() {
        currentState.SetIntelReaction(FlirtIntelReaction);
    }
    private void AfterBecomeParamours() {
        currentState.SetIntelReaction(BecomeParamoursIntelReaction);
    }
    #endregion

    #region Intel Reactions
    private List<string> FlirtIntelReaction(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;
        Character actorLover = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Character targetLover = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);

        //Recipient and Actor is the same:
        if (recipient == actor) {
            //- **Recipient Response Text**: Please do not tell anyone else about this. I beg you!
            reactions.Add("Please do not tell anyone else about this. I beg you!");
            //-**Recipient Effect * *: no effect
        }
        //Recipient is the lover or paramour of Actor and Recipient is not Target:
        else if (recipient != target && actor.HasRelationshipOfTypeWith(recipient, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR)) {
            //- **Recipient Response Text**: [Actor Name] is a cur!
            reactions.Add(string.Format("{0} is a cur!", actor.name));
            //- **Recipient Effect**: Add Annoyed trait to Recipient. Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.
            AddTraitTo(recipient, "Annoyed", actor);
            CharacterManager.Instance.RelationshipDegradation(recipient, actor, this);
        }
        //Recipient is the lover or paramour of Target and Recipient is not Actor:
        else if (recipient != actor && target.HasRelationshipOfTypeWith(recipient, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR)) {
            //- **Recipient Response Text**: [Actor Name] is a snake!
            reactions.Add(string.Format("{0} is a snake!", actor.name));
            //- **Recipient Effect**: Add Annoyed trait to Recipient. Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.
            AddTraitTo(recipient, "Annoyed", actor);
            CharacterManager.Instance.RelationshipDegradation(recipient, actor, this);
        }
        //Actor has a Lover. Actor's Lover is not the Target. Recipient does not have a positive relationship with Actor. Recipient has a relationship (positive or negative) with Actor's Lover.
        else if (recipient != actor && recipient != target && actorLover != null && actorLover != target
            && recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.NEGATIVE && recipient.HasRelationshipWith(actorLover)) {
            //- **Recipient Response Text**: I should let [Actor's Lover's Name] know about this.
            reactions.Add(string.Format("I should let {0} know about this.", actorLover.name));
            //- **Recipient Effect**: Recipient will perform Share Information Job targeting Actor's Lover using this event as the information.
            //Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.

            if (!recipient.jobQueue.HasJobWithOtherData("Share Information", this)) {
                GoapPlanJob job = new GoapPlanJob("Share Information", INTERACTION_TYPE.SHARE_INFORMATION, actorLover, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.SHARE_INFORMATION, new object[] { this }}
                        });
                //job.SetCannotOverrideJob(true);
                job.SetCancelOnFail(true);
                recipient.jobQueue.AddJobInQueue(job, true, false);
            }

            CharacterManager.Instance.RelationshipDegradation(recipient, actor, this);
        }
        //Target has a Lover. Target's Lover is not the Actor. Recipient does not have a positive relationship with Target. Recipient has a positive relationship with Target's Lover.
        else if (recipient != actor && recipient != target && targetLover != null && targetLover != actor
            && recipient.GetRelationshipEffectWith(target) == RELATIONSHIP_EFFECT.NEGATIVE && recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.POSITIVE) {
            //- **Recipient Response Text**: I should let [Target's Lover's Name] know about this.
            reactions.Add(string.Format("I should let {0} know about this.", targetLover.name));
            //- **Recipient Effect**: Recipient will perform Share Information Job targeting Target's Lover using this event as the information.
            //Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.

            if (!recipient.jobQueue.HasJobWithOtherData("Share Information", this)) {
                GoapPlanJob job = new GoapPlanJob("Share Information", INTERACTION_TYPE.SHARE_INFORMATION, targetLover, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.SHARE_INFORMATION, new object[] { this }}
                        });
                //job.SetCannotOverrideJob(true);
                job.SetCancelOnFail(true);
                recipient.jobQueue.AddJobInQueue(job, true, false);
            }

            CharacterManager.Instance.RelationshipDegradation(recipient, actor, this);
        }
        //Recipient and Actor are from the same faction and Actor has a Lover. Actor's Lover is not the Target.
        else if (recipient.faction == actor.faction && actorLover != null && actorLover != target) {
            //- **Recipient Response Text**: [Actor Name] is playing with fire.
            reactions.Add(string.Format("{0} is playing with fire.", actor.name));
            //- **Recipient Effect**: Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.
            CharacterManager.Instance.RelationshipDegradation(recipient, actor, this);
        }
        //Else Catcher:
        else {
            //- **Recipient Response Text**: I don't care what those two do with their personal lives.
            reactions.Add("I don't care what those two do with their personal lives.");
            //- **Recipient Effect**: no effect
        }
        return reactions;
    }

    private List<string> BecomeParamoursIntelReaction(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;
        Character actorLover = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Character targetLover = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);

        //Recipient and Actor is the same or Recipient and Target is the same:
        if (recipient == actor || recipient == target) {
            //- **Recipient Response Text**: Please do not tell anyone else about this. I beg you!
            reactions.Add("Please do not tell anyone else about this. I beg you!");
            //-**Recipient Effect * *: no effect
        }
        //Recipient considers Actor as a Lover:
        else if (recipient.HasRelationshipOfTypeWith(actor, false, RELATIONSHIP_TRAIT.LOVER)) {
            //- **Recipient Response Text**: [Actor Name] is cheating on me!?
            reactions.Add(string.Format("{0} is cheating on me!?", actor.name));
            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between and Recipient and Target.
            //Add an Undermine Job to Recipient versus Target (choose at random). Add a Breakup Job to Recipient versus Actor.
            CharacterManager.Instance.RelationshipDegradation(recipient, target, this);
            recipient.ForceCreateUndermineJob(target);
            recipient.CreateBreakupJob(actor);
        }
        //Recipient considers Target as a Lover:
        else if (recipient.HasRelationshipOfTypeWith(target, false, RELATIONSHIP_TRAIT.LOVER)) {
            //- **Recipient Response Text**: [Target Name] is cheating on me!?
            reactions.Add(string.Format("{0} is cheating on me!?", target.name));
            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between and Recipient and Actor.
            //Add an Undermine Job to Recipient versus Actor (choose at random). Add a Breakup Job to Recipient versus Target.
            CharacterManager.Instance.RelationshipDegradation(recipient, actor, this);
            recipient.ForceCreateUndermineJob(actor);
            recipient.CreateBreakupJob(target);
        }
        //Actor has a Lover. Actor's Lover is not the Target. Recipient does not have a positive relationship with Actor. Recipient has a relationship (positive or negative) with Actor's Lover.
        else if (recipient != actor && recipient != target && actorLover != null && actorLover != target
            && recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.NEGATIVE && recipient.HasRelationshipWith(actorLover)) {
            //- **Recipient Response Text**: I should let [Actor's Lover's Name] know about this.
            reactions.Add(string.Format("I should let {0} know about this.", actorLover.name));
            //- **Recipient Effect**: Recipient will perform Share Information Job targeting Actor's Lover using this event as the information.
            //Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.

            if (!recipient.jobQueue.HasJobWithOtherData("Share Information", this)) {
                GoapPlanJob job = new GoapPlanJob("Share Information", INTERACTION_TYPE.SHARE_INFORMATION, actorLover, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.SHARE_INFORMATION, new object[] { this }}
                        });
                //job.SetCannotOverrideJob(true);
                job.SetCancelOnFail(true);
                recipient.jobQueue.AddJobInQueue(job, true, false);
            }

            CharacterManager.Instance.RelationshipDegradation(recipient, actor, this);
        }
        //Target has a Lover. Target's Lover is not the Actor. Recipient does not have a positive relationship with Target. Recipient has a positive relationship with Target's Lover.
        else if (recipient != actor && recipient != target && targetLover != null && targetLover != actor
            && recipient.GetRelationshipEffectWith(target) == RELATIONSHIP_EFFECT.NEGATIVE && recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.POSITIVE) {
            //- **Recipient Response Text**: I should let [Target's Lover's Name] know about this.
            reactions.Add(string.Format("I should let {0} know about this.", targetLover.name));
            //- **Recipient Effect**: Recipient will perform Share Information Job targeting Target's Lover using this event as the information.
            //Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.

            if (!recipient.jobQueue.HasJobWithOtherData("Share Information", this)) {
                GoapPlanJob job = new GoapPlanJob("Share Information", INTERACTION_TYPE.SHARE_INFORMATION, targetLover, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.SHARE_INFORMATION, new object[] { this }}
                        });
                //job.SetCannotOverrideJob(true);
                job.SetCancelOnFail(true);
                recipient.jobQueue.AddJobInQueue(job, true, false);
            }

            CharacterManager.Instance.RelationshipDegradation(recipient, actor, this);
        }
        //Recipient and Actor are from the same faction and Actor has a Lover. Actor's Lover is not the Target.
        else if (recipient.faction == actor.faction && actorLover != null && actorLover != target) {
            //- **Recipient Response Text**: [Actor Name] is playing with fire.
            reactions.Add(string.Format("{0} is playing with fire.", actor.name));
            //- **Recipient Effect**: Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.
            CharacterManager.Instance.RelationshipDegradation(recipient, actor, this);
        }
        //Else Catcher:
        else {
            //- **Recipient Response Text**: I don't care what those two do with their personal lives.
            reactions.Add("I don't care what those two do with their personal lives.");
            //- **Recipient Effect**: no effect
        }
        return reactions;
    }

    private List<string> QuickChatIntelReaction(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        //Recipient and Actor is the same or Recipient and Target is the same:
        if (recipient == actor || recipient == target) {
            //- **Recipient Response Text**: I know what I did.
            reactions.Add("I know what I did.");
            //-**Recipient Effect * *: no effect
        }
        //Recipient considers Actor as enemy:
        else if (recipient.HasRelationshipOfTypeWith(actor, false, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: Is [Actor Name] talking trash about me again?
            reactions.Add(string.Format("Is {0} talking trash about me again?", actor.name));
            //- **Recipient Effect**: no effect
        }
        //Recipient considers Target as enemy:
        else if (recipient.HasRelationshipOfTypeWith(target, false, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: Is [Target Name] talking trash about me again?
            reactions.Add(string.Format("Is {0} talking trash about me again?", target.name));
            //- **Recipient Effect**: no effect
        }
        //Else:
        else {
            //- **Recipient Response Text**: This isn't relevant to me.
            reactions.Add("This isn't relevant to me.");
            //- **Recipient Effect**: no effect
        }
        return reactions;
    }
    #endregion
}
