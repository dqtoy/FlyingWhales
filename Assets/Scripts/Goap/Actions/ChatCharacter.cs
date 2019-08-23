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

        targetCharacter.marker.LookAt(actor.marker.transform.position, true); //For Trailer Build Only

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
                weight += -30;
            } else if (thisCharacterMood == CHARACTER_MOOD.BAD) {
                weight += -10;
            } else if (thisCharacterMood == CHARACTER_MOOD.GOOD) {
                weight += 10;
            } else if (thisCharacterMood == CHARACTER_MOOD.GREAT) {
                weight += 30;
            }
            if (targetCharacterMood == CHARACTER_MOOD.DARK) {
                weight += -30;
            } else if (targetCharacterMood == CHARACTER_MOOD.BAD) {
                weight += -10;
            } else if (targetCharacterMood == CHARACTER_MOOD.GOOD) {
                weight += 10;
            } else if (targetCharacterMood == CHARACTER_MOOD.GREAT) {
                weight += 30;
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
                    weight += 30;
                } else if (thisCharacterMood == CHARACTER_MOOD.BAD) {
                    weight += 10;
                } else if (thisCharacterMood == CHARACTER_MOOD.GOOD) {
                    weight += -10;
                } else if (thisCharacterMood == CHARACTER_MOOD.GREAT) {
                    weight += -30;
                }
                if (targetCharacterMood == CHARACTER_MOOD.DARK) {
                    weight += 30;
                } else if (targetCharacterMood == CHARACTER_MOOD.BAD) {
                    weight += 10;
                } else if (targetCharacterMood == CHARACTER_MOOD.GOOD) {
                    weight += -10;
                } else if (targetCharacterMood == CHARACTER_MOOD.GREAT) {
                    weight += -30;
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

#if TRAILER_BUILD
        if (actor.name == "Jamie" && targetCharacter.name == "Fiona") {
            chatResult = "Flirt"; //For Trailer Only
        }
        if (actor.name == "Jamie" && targetCharacter.name == "Audrey") {
            chatResult = "Argument"; //For Trailer Only
        }
#endif

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

        //weights.LogDictionaryValues("Chat Weights of " + actor.name + " and " + targetCharacter.name);
        //Debug.Log(actor.name + " and " + targetCharacter.name + "'s chat result is " + chatResult);

        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(2);
        SchedulingManager.Instance.AddEntry(dueDate, () => actor.EndChatCharacter(targetCharacter), actor);

        SetState(chatResult);

        Plagued actorPlagued = actor.GetNormalTrait("Plagued") as Plagued;
        Plagued targetPlagued = poiTarget.GetNormalTrait("Plagued") as Plagued;
        //Plagued chances
        if ((actorPlagued == null || targetPlagued == null) && (actorPlagued != null || targetPlagued != null)) {
            string plaguedSummary = "Chat with plagued character.";
            //if either the actor or the target is not yet plagued and one of them is plagued, check for infection chance
            if (actorPlagued != null) {
                plaguedSummary += "\nPlagued character is " + actor.name + ", will roll to infect " + poiTarget.name;
                //actor has plagued trait
                int roll = Random.Range(0, 100);
                plaguedSummary += "\nRoll is: " + roll.ToString() + ", Chance is: " + actorPlagued.GetChatInfectChance().ToString();
                if (roll < actorPlagued.GetChatInfectChance()) {
                    //target will be infected with plague
                    plaguedSummary += "\nWill infect " + poiTarget.name + " with plague!";
                    if (AddTraitTo(poiTarget, "Plagued", actor)) {
                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_plague");
                        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddLogToInvolvedObjects();
                    }
                }
                Debug.Log(plaguedSummary);
            } else if (targetPlagued != null) {
                plaguedSummary += "\nPlagued character is " + poiTarget.name + ", will roll to infect " + actor.name;
                //target has plagued trait
                int roll = Random.Range(0, 100);
                plaguedSummary += "\nRoll is: " + roll.ToString() + ", Chance is: " + targetPlagued.GetChatInfectChance().ToString();
                if (roll < targetPlagued.GetChatInfectChance()) {
                    //actor will be infected with plague
                    plaguedSummary += "\nWill infect " + actor.name + " with plague!";
                    if (AddTraitTo(actor, "Plagued", poiTarget as Character)) {
                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_plague");
                        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddLogToInvolvedObjects();
                    }
                }
                Debug.Log(plaguedSummary);
            }
        }
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region State Effects
    private void PreQuickChat() {
        currentState.SetIntelReaction(QuickChatIntelReaction);
    }
    private void PreShareInformation() {
        currentState.SetIntelReaction(ShareInformationIntelReaction);
    }
    private void PreBecomeFriends() {
        currentState.SetIntelReaction(BecomeFriendsIntelReaction);
    }
    private void PreArgument() {
        currentState.SetIntelReaction(ArgumentIntelReaction);
    }
    private void PreFlirt() {
        currentState.SetIntelReaction(FlirtIntelReaction);
    }
    private void PreBecomeLovers() {
        currentState.SetIntelReaction(BecomeLoversIntelReaction);
    }
    private void PreBecomeParamours() {
        currentState.SetIntelReaction(BecomeParamoursIntelReaction);
    }
    private void PreResolveEnmity() {
        currentState.SetIntelReaction(ResolveEnmityIntelReaction);
    }
    #endregion

    private void CreatePoisonTable(Character character) {
        Character target = poiTarget as Character;
        IPointOfInterest targetTable = target.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.TABLE)[0];
        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.UNDERMINE_ENEMY, INTERACTION_TYPE.TABLE_POISON, targetTable);
        //job.SetCannotOverrideJob(true);
        job.SetCannotCancelJob(true);
        //job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
        character.jobQueue.AddJobInQueue(job, true);
        character.jobQueue.ProcessFirstJobInQueue(character);
    }

    #region Intel Reactions
    private List<string> QuickChatIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;


 	    if (recipient == actor || recipient == target) {
            //- **Recipient Response Text**: I know what I did.
            reactions.Add("I know what I did.");
            //-**Recipient Effect * *: no effect
        } 
        else if(recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.PARAMOUR) || recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR)) {
            reactions.Add("Wait what?! What were they talking about?!");
        } 
        else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.LOVER) || recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
            reactions.Add("Huh? What were they talking about?");
        } 
        else {
            reactions.Add("Is this important?");
        }
        return reactions;
    }
    private List<string> ShareInformationIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        //Recipient and Actor is the same or Recipient and Target is the same:
        if (recipient == actor || recipient == target) {
            //- **Recipient Response Text**: I know what I did.
            reactions.Add("I know what I did.");
            //-**Recipient Effect * *: no effect
        } 
        else {
            reactions.Add("I wonder what they discussed.");
        }
        return reactions;
    }
    private List<string> BecomeFriendsIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        //Recipient and Actor is the same or Recipient and Target is the same:
        if (recipient == actor || recipient == target) {
            //- **Recipient Response Text**: I know what I did.
            reactions.Add("I know what I did.");
            //-**Recipient Effect * *: no effect
        } 
        else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY) && recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.ENEMY)) {
            reactions.Add("Are they conspiring against me?!");
        } 
        else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
            reactions.Add(string.Format("So {0} is gathering allies, huh?", actor.name));
        } 
        else if (recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.ENEMY)) {
            reactions.Add(string.Format("So {0} is gathering allies, huh?", target.name));
        }
        else {
            reactions.Add("Is this important?");
        }
        return reactions;
    }
    private List<string> ArgumentIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        //Recipient and Actor is the same or Recipient and Target is the same:
        if (recipient == actor || recipient == target) {
            //- **Recipient Response Text**: I know what I did.
            reactions.Add("I know what I did.");
            //-**Recipient Effect * *: no effect
        }
        else {
            reactions.Add("I wonder what they argued about.");
        }
        return reactions;
    }
    private List<string> FlirtIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;
        Character actorLover = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Character actorParamour = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);
        Character targetLover = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Character targetParamour = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);

#if TRAILER_BUILD
        if (recipient.name == "Audrey" && actor.name == "Jamie" && target.name == "Fiona") {
            reactions.Add(string.Format("This is the last straw! I'm leaving that cur {0}, and this godforsaken town!", actor.name));
            recipient.CancelAllJobsAndPlans();
            recipient.marker.GoTo(recipient.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS).GetRandomUnoccupiedTile(), () => CreatePoisonTable(recipient));
            return reactions;
        }
#endif

        //Recipient and Actor is the same:
        if (recipient == actor) {
            if (recipient == targetLover) {
                reactions.Add("Hey! That's private!");
            } else if (recipient == targetParamour) {
                reactions.Add("Don't tell anyone. *wink**wink*");
            }
        } 
        else if (recipient == target) {
            if (recipient == actorLover) {
                reactions.Add("Hey! That's private!");
            } else if (recipient == actorParamour) {
                reactions.Add("Don't you dare judge me!");
            }
        } 
        else if (recipient == actorLover) {
            if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                reactions.Add(string.Format("I've had enough of {0}'s shenanigans!", actor.name));
            } else {
                reactions.Add("It's just harmless flirtation.");
            }
        } 
        else if (recipient == targetLover) {
            if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                reactions.Add(string.Format("I've had enough of {0}'s shenanigans!", target.name));
            } else {
                reactions.Add("It's just harmless flirtation.");
            }
        } 
        else if (recipient == actorParamour || recipient == targetParamour) {
            reactions.Add("I thought I was the only snake in town.");
            AddTraitTo(recipient, "Annoyed");
        } 
        else {
            reactions.Add("This isn't relevant to me..");
        }
        return reactions;
    }
    private List<string> BecomeLoversIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;
        Character actorParamour = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);
        Character targetParamour = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);
        if (recipient == actor) {
            if(recipient == targetParamour) {
                reactions.Add(string.Format("Yes that's true! I am so happy {0} finally chose me. This is what I've been dreaming for and at last, it came true!", target.name));
            } else {
                reactions.Add("Yes that's true and I am very happy. I hope we can be happy together, forever.");
            }
        }
        else if (recipient == target) {
            if (recipient == actorParamour) {
                reactions.Add(string.Format("Yes that's true! I am so happy {0} finally chose me. This is what I've been dreaming for and at last, it came true!", actor.name));
            } else {
                reactions.Add("Yes that's true and I am very happy. I hope we can be happy together, forever.");
            }
        } 
        else if (recipient == actorParamour) {
            if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                reactions.Add(string.Format("I'm done being the appetizer in {0}'s full course meal!", actor.name));
            } else {
                reactions.Add(string.Format("Why can't I let {0} go? Perhaps, I'm truly, madly, deeply in love with {0}.", Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
            }
            AddTraitTo(recipient, "Heartbroken");
        } 
        else if (recipient == targetParamour) {
            if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                reactions.Add(string.Format("I'm done being the appetizer in {0}'s full course meal!", target.name));
            } else {
                reactions.Add(string.Format("Why can't I let {0} go? Perhaps, I'm truly, madly, deeply in love with {0}.", Utilities.GetPronounString(target.gender, PRONOUN_TYPE.OBJECTIVE, false)));
            }
            AddTraitTo(recipient, "Heartbroken");
        } 
        else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY) || recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.ENEMY)) {
            reactions.Add("That won't last. Mark my words!");
            AddTraitTo(recipient, "Annoyed");
        }
        else {
            reactions.Add("I guess there are two less lonely people in the world.");
        }
        return reactions;
    }

    private List<string> BecomeParamoursIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
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
            CharacterManager.Instance.RelationshipDegradation(target, recipient, this);
            recipient.ForceCreateUndermineJob(target, "snake");
            //recipient.CreateBreakupJob(actor);
        }
        //Recipient considers Target as a Lover:
        else if (recipient.HasRelationshipOfTypeWith(target, false, RELATIONSHIP_TRAIT.LOVER)) {
            //- **Recipient Response Text**: [Target Name] is cheating on me!?
            reactions.Add(string.Format("{0} is cheating on me!?", target.name));
            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between and Recipient and Actor.
            //Add an Undermine Job to Recipient versus Actor (choose at random). Add a Breakup Job to Recipient versus Target.
            CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);
            recipient.ForceCreateUndermineJob(actor, "snake");
            //recipient.CreateBreakupJob(target);
        }
        //Actor has a Lover. Actor's Lover is not the Target. Recipient does not have a positive relationship with Actor. Recipient has a relationship (positive or negative) with Actor's Lover.
        else if (recipient != actor && recipient != target && actorLover != null && actorLover != target
            && recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.NEGATIVE && recipient.HasRelationshipWith(actorLover)) {
            //- **Recipient Response Text**: I should let [Actor's Lover's Name] know about this.
            reactions.Add(string.Format("I should let {0} know about this.", actorLover.name));
            //- **Recipient Effect**: Recipient will perform Share Information Job targeting Actor's Lover using this event as the information.
            //Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.

            recipient.CreateShareInformationJob(actorLover, this);

            //CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);
        }
        //Target has a Lover. Target's Lover is not the Actor. Recipient does not have a positive relationship with Target. Recipient has a positive relationship with Target's Lover.
        else if (recipient != actor && recipient != target && targetLover != null && targetLover != actor
            && recipient.GetRelationshipEffectWith(target) == RELATIONSHIP_EFFECT.NEGATIVE && recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.POSITIVE) {
            //- **Recipient Response Text**: I should let [Target's Lover's Name] know about this.
            reactions.Add(string.Format("I should let {0} know about this.", targetLover.name));
            //- **Recipient Effect**: Recipient will perform Share Information Job targeting Target's Lover using this event as the information.
            //Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.

            recipient.CreateShareInformationJob(targetLover, this);

            CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);
        }
        //Recipient and Actor are from the same faction and Actor has a Lover. Actor's Lover is not the Target.
        else if (recipient.faction == actor.faction && actorLover != null && actorLover != target) {
            //- **Recipient Response Text**: [Actor Name] is playing with fire.
            reactions.Add(string.Format("{0} is playing with fire.", actor.name));
            //- **Recipient Effect**: Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.
            CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);
        }
        //Else Catcher:
        else {
            //- **Recipient Response Text**: I don't care what those two do with their personal lives.
            reactions.Add("I don't care what those two do with their personal lives.");
            //- **Recipient Effect**: no effect
        }
        return reactions;
    }
    private List<string> ResolveEnmityIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;
        //Character actorParamour = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);
        //Character targetParamour = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);

        //Recipient and Actor is the same or Recipient and Target is the same:
        if (recipient == actor || recipient == target) {
            //- **Recipient Response Text**: I know what I did.
            reactions.Add("I'm thankful we cleared that out.");
            //-**Recipient Effect * *: no effect
        } 
        else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE || recipient.GetRelationshipEffectWith(target) == RELATIONSHIP_EFFECT.POSITIVE) {
            reactions.Add("I'm glad they cleared things out.");
        } 
        else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.NEGATIVE) {
            reactions.Add(string.Format("{0} has tricked another one.", actor.name));
        } 
        else if (recipient.GetRelationshipEffectWith(target) == RELATIONSHIP_EFFECT.NEGATIVE) {
            reactions.Add(string.Format("{0} has tricked another one.", target.name));
        } 
        else {
            reactions.Add("Good for them.");
        }
        return reactions;
    }
    #endregion
}
