using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class NonActionEventsComponent {
    public Character owner { get; private set; }

    private const string Warm_Chat = "Warm Chat";
    private const string Awkward_Chat = "Awkward Chat";
    private const string Argument = "Argument";
    private const string Insult = "Insult";
    private const string Praise = "Praise";

    private WeightedDictionary<string> chatWeights;

    public NonActionEventsComponent(Character owner) {
        this.owner = owner;
        chatWeights = new WeightedDictionary<string>();
        Messenger.AddListener<Character, Character, string>(Signals.OPINION_DECREASED, OnOpinionDecreased);
    }

    #region Utilities
    public bool CanInteract(Character target) {
        if (target.isDead
            || !target.canWitness
            || !owner.canWitness
            || target.role.roleType == CHARACTER_ROLE.BEAST
            || owner.role.roleType == CHARACTER_ROLE.BEAST
            //|| target.faction.isPlayerFaction
            //|| owner.faction.isPlayerFaction
            //|| target.characterClass.className == "Zombie"
            //|| owner.characterClass.className == "Zombie"
            //|| (owner.currentActionNode != null && owner.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)
            //|| (target.currentActionNode != null && target.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)
            //|| owner.isChatting
            //|| target.isChatting
            ) {
            return false;
        }
        return true;
    }
    #endregion

    #region Chat
    public bool NormalChatCharacter(Character target) {
        //if (!CanInteract(target)) {
        //    return false;
        //}
        if (UnityEngine.Random.Range(0, 100) < 50) {
            if (!owner.IsHostileWith(target)) {
                TriggerChatCharacter(target);
                return true;
            }
        }
        return false;
    }
    public bool ForceChatCharacter(Character target) {
        //if (!CanInteract(target)) {
        //    return false;
        //}
        if (!owner.IsHostileWith(target)) {
            TriggerChatCharacter(target);
            return true;
        }
        return false;
    }
    private void TriggerChatCharacter(Character target) {
        string strLog = owner.name + " chat with " + target.name;
        chatWeights.Clear();
        chatWeights.AddElement(Warm_Chat, 100);
        chatWeights.AddElement(Awkward_Chat, 30);
        chatWeights.AddElement(Argument, 20);
        chatWeights.AddElement(Insult, 20);
        chatWeights.AddElement(Praise, 20);

        strLog += "\n\n" + chatWeights.GetWeightsSummary("BASE WEIGHTS");

        MOOD_STATE actorMood = owner.moodComponent.moodState;
        MOOD_STATE targetMood = target.moodComponent.moodState;
        string actorOpinionLabel = owner.opinionComponent.GetOpinionLabel(target);
        string targetOpinionLabel = target.opinionComponent.GetOpinionLabel(owner);
        int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(owner, target);

        if (actorMood == MOOD_STATE.LOW) {
            chatWeights.AddWeightToElement(Warm_Chat, -20);
            chatWeights.AddWeightToElement(Argument, 15);
            chatWeights.AddWeightToElement(Insult, 20);
            strLog += "\n\nActor Mood is Low, modified weights...";
            strLog += "\nWarm Chat: -20, Argument: +15, Insult: +20";
        } else if (actorMood == MOOD_STATE.CRITICAL) {
            chatWeights.AddWeightToElement(Warm_Chat, -40);
            chatWeights.AddWeightToElement(Argument, 30);
            chatWeights.AddWeightToElement(Insult, 50);
            strLog += "\n\nActor Mood is Critical, modified weights...";
            strLog += "\nWarm Chat: -40, Argument: +30, Insult: +50";
        }

        if (targetMood == MOOD_STATE.LOW) {
            chatWeights.AddWeightToElement(Warm_Chat, -20);
            chatWeights.AddWeightToElement(Argument, 15);
            strLog += "\n\nTarget Mood is Low, modified weights...";
            strLog += "\nWarm Chat: -20, Argument: +15";
        } else if (targetMood == MOOD_STATE.CRITICAL) {
            chatWeights.AddWeightToElement(Warm_Chat, -40);
            chatWeights.AddWeightToElement(Argument, 30);
            strLog += "\n\nTarget Mood is Critical, modified weights...";
            strLog += "\nWarm Chat: -40, Argument: +30";
        }

        if (actorOpinionLabel == OpinionComponent.Close_Friend || actorOpinionLabel == OpinionComponent.Friend) {
            chatWeights.AddWeightToElement(Awkward_Chat, -15);
            strLog += "\n\nActor's opinion of Target is Close Friend or Friend, modified weights...";
            strLog += "\nAwkward Chat: -15";
        } else if (actorOpinionLabel == OpinionComponent.Enemy || actorOpinionLabel == OpinionComponent.Rival) {
            chatWeights.AddWeightToElement(Awkward_Chat, 15);
            strLog += "\n\nActor's opinion of Target is Enemy or Rival, modified weights...";
            strLog += "\nAwkward Chat: +15";
        }

        if (targetOpinionLabel == OpinionComponent.Close_Friend || targetOpinionLabel == OpinionComponent.Friend) {
            chatWeights.AddWeightToElement(Awkward_Chat, -15);
            strLog += "\n\nTarget's opinion of Actor is Close Friend or Friend, modified weights...";
            strLog += "\nAwkward Chat: -15";
        } else if (targetOpinionLabel == OpinionComponent.Enemy || targetOpinionLabel == OpinionComponent.Rival) {
            chatWeights.AddWeightToElement(Awkward_Chat, 15);
            strLog += "\n\nTarget's opinion of Actor is Enemy or Rival, modified weights...";
            strLog += "\nAwkward Chat: +15";
        }

        if(compatibility != -1) {
            strLog += "\n\nActor and Target Compatibility is " + compatibility + ", modified weights...";
            if (compatibility == 0) {
                chatWeights.AddWeightToElement(Awkward_Chat, 15);
                chatWeights.AddWeightToElement(Argument, 20);
                chatWeights.AddWeightToElement(Insult, 15);
                strLog += "\nAwkward Chat: +15, Argument: +20, Insult: +15";
            } else if (compatibility == 1) {
                chatWeights.AddWeightToElement(Awkward_Chat, 10);
                chatWeights.AddWeightToElement(Argument, 10);
                chatWeights.AddWeightToElement(Insult, 10);
                strLog += "\nAwkward Chat: +10, Argument: +10, Insult: +10";
            } else if (compatibility == 2) {
                chatWeights.AddWeightToElement(Awkward_Chat, 5);
                chatWeights.AddWeightToElement(Argument, 5);
                chatWeights.AddWeightToElement(Insult, 5);
                strLog += "\nAwkward Chat: +5, Argument: +5, Insult: +5";
            } else if (compatibility == 3) {
                chatWeights.AddWeightToElement(Praise, 5);
                strLog += "\nPraise: +5";
            } else if (compatibility == 4) {
                chatWeights.AddWeightToElement(Praise, 10);
                strLog += "\nPraise: +10";
            } else if (compatibility == 5) {
                chatWeights.AddWeightToElement(Praise, 20);
                strLog += "\nPraise: +20";
            }
        }

        if (owner.traitContainer.GetNormalTrait<Trait>("Hothead") != null) {
            chatWeights.AddWeightToElement(Argument, 15);
            strLog += "\n\nActor is Hotheaded, modified weights...";
            strLog += "\nArgument: +15";
        }
        if (target.traitContainer.GetNormalTrait<Trait>("Hothead") != null) {
            chatWeights.AddWeightToElement(Argument, 15);
            strLog += "\n\nTarget is Hotheaded, modified weights...";
            strLog += "\nArgument: +15";
        }

        if (owner.traitContainer.GetNormalTrait<Trait>("Diplomatic") != null) {
            chatWeights.AddWeightToElement(Insult, -30);
            chatWeights.AddWeightToElement(Praise, 30);
            strLog += "\n\nActor is Diplomatic, modified weights...";
            strLog += "\nInsult: -30, Praise: +30";
        }

        strLog += "\n\n" + chatWeights.GetWeightsSummary("FINAL WEIGHTS");

        string result = chatWeights.PickRandomElementGivenWeights();
        strLog += "\nResult: " + result;

        if (owner.traitContainer.GetNormalTrait<Trait>("Plagued") != null && target.traitContainer.GetNormalTrait<Trait>("Plagued") == null) {
            strLog += "\n\nCharacter has Plague, 25% chance to infect the Target";
            int roll = UnityEngine.Random.Range(0, 100);
            strLog += "\nRoll: " + roll;
            if (roll < 25) {
                target.interruptComponent.TriggerInterrupt(INTERRUPT.Plagued, target);
                // target.traitContainer.AddTrait(target, "Plagued", owner);
            }
        } else if (owner.traitContainer.GetNormalTrait<Trait>("Plagued") == null && target.traitContainer.GetNormalTrait<Trait>("Plagued") != null) {
            strLog += "\n\nTarget has Plague, 25% chance to infect the Character";
            int roll = UnityEngine.Random.Range(0, 100);
            strLog += "\nRoll: " + roll;
            if (roll < 25) {
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Plagued, owner);
                // owner.traitContainer.AddTrait(owner, "Plagued", target);
            }
        }
        
        owner.logComponent.PrintLogIfActive(strLog);

        bool adjustOpinionBothSides = false;
        int opinionValue = 0;

        if(result == Warm_Chat) {
            opinionValue = 2;
            adjustOpinionBothSides = true;
        } else if (result == Awkward_Chat) {
            opinionValue = -1;
            adjustOpinionBothSides = true;
        } else if (result == Argument) {
            opinionValue = -2;
            adjustOpinionBothSides = true;
        } else if (result == Insult) {
            opinionValue = -3;
        } else if (result == Praise) {
            opinionValue = 3;
        }

        if (adjustOpinionBothSides) {
            owner.opinionComponent.AdjustOpinion(target, result, opinionValue, "engaged in disastrous conversation");
            target.opinionComponent.AdjustOpinion(owner, result, opinionValue, "engaged in disastrous conversation");
        } else {
            //If adjustment of opinion is not on both sides, this must mean that the result is either Insult or Praise, so adjust opinion of target to actor
            target.opinionComponent.AdjustOpinion(owner, result, opinionValue);
        }

        GameDate dueDate = GameManager.Instance.Today();
        Log log = new Log(dueDate, "Interrupt", "Chat", result);
        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        owner.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        owner.SetIsConversing(true);
        target.SetIsConversing(true);

        dueDate.AddTicks(2);
        SchedulingManager.Instance.AddEntry(dueDate, () => owner.SetIsConversing(false), owner);
        SchedulingManager.Instance.AddEntry(dueDate, () => target.SetIsConversing(false), target);

    }
    #endregion

    #region Break Up
    //Char1 decreased his/her opinion of char2
    private void OnOpinionDecreased(Character char1, Character char2, string reason) {
        if(char1 == owner) {
            if (UnityEngine.Random.Range(0, 100) < 30) {
                if (owner.opinionComponent.GetTotalOpinion(char2) < -25) {
                    if (owner.relationshipContainer.HasRelationshipWith(char2, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR)) {
                        char1.interruptComponent.TriggerInterrupt(INTERRUPT.Break_Up, char2, reason);
                    }
                }
            }
        }
    }
    public void NormalBreakUp(Character target, string reason) {
        RELATIONSHIP_TYPE relationship = owner.relationshipContainer.GetRelationshipFromParametersWith(target, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
        TriggerBreakUp(target, relationship, reason);
    }
    private void TriggerBreakUp(Character target, RELATIONSHIP_TYPE relationship, string reason) {
        RelationshipManager.Instance.RemoveRelationshipBetween(owner, target, relationship);
        //upon break up, if one of them still has a Positive opinion of the other, he will gain Heartbroken trait
        if (!owner.isSerialKiller) { //owner.opinionComponent.GetTotalOpinion(target) >= 0
            owner.traitContainer.AddTrait(owner, "Heartbroken", target);
        }
        if (!target.isSerialKiller) { //target.opinionComponent.GetTotalOpinion(owner) >= 0
            target.traitContainer.AddTrait(target, "Heartbroken", owner);
        }
        RelationshipManager.Instance.CreateNewRelationshipBetween(owner, target, RELATIONSHIP_TYPE.EX_LOVER);

        Log log = null;
        if (reason != string.Empty) {
            log = new Log(GameManager.Instance.Today(), "Interrupt", "Break Up", "break_up_reason");
            log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
        } else {
            log = new Log(GameManager.Instance.Today(), "Interrupt", "Break Up", "break_up");
        }
        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        owner.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);

        if (relationship == RELATIONSHIP_TYPE.LOVER) {
            //**Effect 1**: Actor - Remove Lover relationship with Character 2
            //if the relationship that was removed is lover, change home to a random unoccupied dwelling,
            //otherwise, no home. Reference: https://trello.com/c/JUSt9bEa/1938-broken-up-characters-should-live-in-separate-house
            owner.MigrateHomeStructureTo(null);
            owner.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, owner);
            //if (owner.homeRegion.area != null) {
            //owner.homeRegion.area.AssignCharacterToDwellingInArea(owner);
            //}
        }
    }
    #endregion

    #region Flirt
    public bool NormalFlirtCharacter(Character target) {
        //if (!CanInteract(target)) {
        //    return false;
        //}
        if (!owner.IsHostileWith(target)) {
            string result = TriggerFlirtCharacter(target);
            GameDate dueDate = GameManager.Instance.Today();
            Log log = new Log(dueDate, "Interrupt", "Flirt", result);
            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            owner.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
            owner.SetIsConversing(true);
            target.SetIsConversing(true);

            dueDate.AddTicks(2);
            SchedulingManager.Instance.AddEntry(dueDate, () => owner.SetIsConversing(false), owner);
            SchedulingManager.Instance.AddEntry(dueDate, () => target.SetIsConversing(false), target);
            return true;
        }
        return false;
    }
    private string TriggerFlirtCharacter(Character target) {
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 50) {
            if (owner.traitContainer.GetNormalTrait<Trait>("Ugly") != null) {
                owner.opinionComponent.AdjustOpinion(target, "Base", -4, "engaged in disastrous flirting");
                target.opinionComponent.AdjustOpinion(owner, "Base", -2, "engaged in disastrous flirting");
                return "ugly";
            }
        }
        if(chance < 90) {
            if(!RelationshipManager.Instance.IsSexuallyCompatibleOneSided(target, owner)) {
                owner.opinionComponent.AdjustOpinion(target, "Base", -4, "engaged in disastrous flirting");
                target.opinionComponent.AdjustOpinion(owner, "Base", -2, "engaged in disastrous flirting");
                return "incompatible";
            }
        }
        owner.opinionComponent.AdjustOpinion(target, "Base", 2);
        target.opinionComponent.AdjustOpinion(owner, "Base", 4);

        int value = 0;
        string opinionLabel = owner.opinionComponent.GetOpinionLabel(target);
        if(opinionLabel == OpinionComponent.Acquaintance) {
            value = 1;
        }else if (opinionLabel == OpinionComponent.Friend || opinionLabel == OpinionComponent.Close_Friend) {
            value = 2;
        }
        if (value != 0
            && UnityEngine.Random.Range(0, 10) < value
            && owner.relationshipValidator.CanHaveRelationship(owner, target, RELATIONSHIP_TYPE.LOVER)
            && target.relationshipValidator.CanHaveRelationship(target, owner, RELATIONSHIP_TYPE.LOVER)) {
            RelationshipManager.Instance.CreateNewRelationshipBetween(owner, target, RELATIONSHIP_TYPE.LOVER);
        } else if (value != 0
            && UnityEngine.Random.Range(0, 10) < value
            && owner.relationshipValidator.CanHaveRelationship(owner, target, RELATIONSHIP_TYPE.AFFAIR)
            && target.relationshipValidator.CanHaveRelationship(target, owner, RELATIONSHIP_TYPE.AFFAIR)) {
            RelationshipManager.Instance.CreateNewRelationshipBetween(owner, target, RELATIONSHIP_TYPE.AFFAIR);
            return "flirted_back";
        }
        return "flirted_back";
    }
    #endregion
}
