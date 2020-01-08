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


    public NonActionEventsComponent(Character owner) {
        this.owner = owner;
        Messenger.AddListener<Character, Character>(Signals.OPINION_DECREASED, OnOpinionDecreased);
    }

    #region Chat
    public bool NormalChatCharacter(Character target) {
        if (target.isDead
            || !target.canWitness
            || !owner.canWitness
            || target.role.roleType == CHARACTER_ROLE.BEAST
            || owner.role.roleType == CHARACTER_ROLE.BEAST
            || target.faction.isPlayerFaction
            || owner.faction.isPlayerFaction
            || target.characterClass.className == "Zombie"
            || owner.characterClass.className == "Zombie"
            || (owner.currentActionNode != null && owner.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)
            || (target.currentActionNode != null && target.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)
            || owner.isChatting
            || target.isChatting) {
            return false;
        }
        if (UnityEngine.Random.Range(0, 100) < 50) {
            if (!owner.IsHostileWith(target)) {
                TriggerChatCharacter(target);
                return true;
            }
        }
        return false;
    }
    public bool ForceChatCharacter(Character target) {
        if (target.isDead
            || !target.canWitness
            || !owner.canWitness
            || target.role.roleType == CHARACTER_ROLE.BEAST
            || owner.role.roleType == CHARACTER_ROLE.BEAST
            || target.faction.isPlayerFaction
            || owner.faction.isPlayerFaction
            || target.characterClass.className == "Zombie"
            || owner.characterClass.className == "Zombie"
            || (owner.currentActionNode != null && owner.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)
            || (target.currentActionNode != null && target.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)
            || owner.isChatting
            || target.isChatting) {
            return false;
        }
        if (!owner.IsHostileWith(target)) {
            TriggerChatCharacter(target);
            return true;
        }
        return false;
    }
    private void TriggerChatCharacter(Character target) {
        string strLog = owner.name + " chat with " + target.name;
        WeightedDictionary<string> weights = new WeightedDictionary<string>();
        weights.AddElement(Warm_Chat, 100);
        weights.AddElement(Awkward_Chat, 30);
        weights.AddElement(Argument, 20);
        weights.AddElement(Insult, 20);
        weights.AddElement(Praise, 20);

        strLog += "\n\n" + weights.GetWeightsSummary("Base Weights");

        CHARACTER_MOOD actorMood = owner.currentMoodType;
        CHARACTER_MOOD targetMood = target.currentMoodType;
        string actorOpinionLabel = owner.opinionComponent.GetOpinionLabel(target);
        string targetOpinionLabel = target.opinionComponent.GetOpinionLabel(owner);
        int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(owner, target);

        if (actorMood == CHARACTER_MOOD.BAD) {
            weights.AddWeightToElement(Warm_Chat, -20);
            weights.AddWeightToElement(Argument, 15);
            weights.AddWeightToElement(Insult, 20);
            strLog += "\n\nActor Mood is Low, modified weights...";
            strLog += "\nWarm Chat: -20, Argument: +15, Insult: +20";
        } else if (actorMood == CHARACTER_MOOD.DARK) {
            weights.AddWeightToElement(Warm_Chat, -40);
            weights.AddWeightToElement(Argument, 30);
            weights.AddWeightToElement(Insult, 50);
            strLog += "\n\nActor Mood is Critical, modified weights...";
            strLog += "\nWarm Chat: -40, Argument: +30, Insult: +50";
        }

        if (targetMood == CHARACTER_MOOD.BAD) {
            weights.AddWeightToElement(Warm_Chat, -20);
            weights.AddWeightToElement(Argument, 15);
            strLog += "\n\nTarget Mood is Low, modified weights...";
            strLog += "\nWarm Chat: -20, Argument: +15";
        } else if (targetMood == CHARACTER_MOOD.DARK) {
            weights.AddWeightToElement(Warm_Chat, -40);
            weights.AddWeightToElement(Argument, 30);
            strLog += "\n\nTarget Mood is Critical, modified weights...";
            strLog += "\nWarm Chat: -40, Argument: +30";
        }

        if (actorOpinionLabel == OpinionComponent.Close_Friend || actorOpinionLabel == OpinionComponent.Friend) {
            weights.AddWeightToElement(Awkward_Chat, -15);
            strLog += "\n\nActor's opinion of Target is Close Friend or Friend, modified weights...";
            strLog += "\nAwkward Chat: -15";
        } else if (actorOpinionLabel == OpinionComponent.Enemy || actorOpinionLabel == OpinionComponent.Rival) {
            weights.AddWeightToElement(Awkward_Chat, 15);
            strLog += "\n\nActor's opinion of Target is Enemy or Rival, modified weights...";
            strLog += "\nAwkward Chat: +15";
        }

        if (targetOpinionLabel == OpinionComponent.Close_Friend || targetOpinionLabel == OpinionComponent.Friend) {
            weights.AddWeightToElement(Awkward_Chat, -15);
            strLog += "\n\nTarget's opinion of Actor is Close Friend or Friend, modified weights...";
            strLog += "\nAwkward Chat: -15";
        } else if (targetOpinionLabel == OpinionComponent.Enemy || targetOpinionLabel == OpinionComponent.Rival) {
            weights.AddWeightToElement(Awkward_Chat, 15);
            strLog += "\n\nTarget's opinion of Actor is Enemy or Rival, modified weights...";
            strLog += "\nAwkward Chat: +15";
        }

        if(compatibility != -1) {
            strLog += "\n\nActor and Target Compatibility is " + compatibility + ", modified weights...";
            if (compatibility == 0) {
                weights.AddWeightToElement(Awkward_Chat, 15);
                weights.AddWeightToElement(Argument, 20);
                weights.AddWeightToElement(Insult, 15);
                strLog += "\nAwkward Chat: +15, Argument: +20, Insult: +15";
            } else if (compatibility == 1) {
                weights.AddWeightToElement(Awkward_Chat, 10);
                weights.AddWeightToElement(Argument, 10);
                weights.AddWeightToElement(Insult, 10);
                strLog += "\nAwkward Chat: +10, Argument: +10, Insult: +10";
            } else if (compatibility == 2) {
                weights.AddWeightToElement(Awkward_Chat, 5);
                weights.AddWeightToElement(Argument, 5);
                weights.AddWeightToElement(Insult, 5);
                strLog += "\nAwkward Chat: +5, Argument: +5, Insult: +5";
            } else if (compatibility == 3) {
                weights.AddWeightToElement(Praise, 5);
                strLog += "\nPraise: +5";
            } else if (compatibility == 4) {
                weights.AddWeightToElement(Praise, 10);
                strLog += "\nPraise: +10";
            } else if (compatibility == 5) {
                weights.AddWeightToElement(Praise, 20);
                strLog += "\nPraise: +20";
            }
        }

        if (owner.traitContainer.GetNormalTrait<Trait>("Hothead") != null) {
            weights.AddWeightToElement(Argument, 15);
            strLog += "\n\nActor is Hotheaded, modified weights...";
            strLog += "\nArgument: +15";
        }
        if (target.traitContainer.GetNormalTrait<Trait>("Hothead") != null) {
            weights.AddWeightToElement(Argument, 15);
            strLog += "\n\nTarget is Hotheaded, modified weights...";
            strLog += "\nArgument: +15";
        }

        if (owner.traitContainer.GetNormalTrait<Trait>("Diplomatic") != null) {
            weights.AddWeightToElement(Insult, -30);
            weights.AddWeightToElement(Praise, 30);
            strLog += "\n\nActor is Diplomatic, modified weights...";
            strLog += "\nInsult: -30, Praise: +30";
        }

        strLog += "\n\n" + weights.GetWeightsSummary("Final Weights");

        string result = weights.PickRandomElementGivenWeights();
        strLog += "\nResult: " + result;

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
            owner.opinionComponent.AdjustOpinion(target, result, opinionValue);
            target.opinionComponent.AdjustOpinion(owner, result, opinionValue);
        } else {
            //If adjustment of opinion is not on both sides, this must mean that the result is either Insult or Praise, so adjust opinion of target to actor
            target.opinionComponent.AdjustOpinion(owner, result, opinionValue);
        }

        GameDate dueDate = GameManager.Instance.Today();
        Log log = new Log(dueDate, "Interrupt", "Chat", result);
        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        owner.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        owner.SetIsChatting(true);
        target.SetIsChatting(true);

        dueDate.AddTicks(2);
        SchedulingManager.Instance.AddEntry(dueDate, () => owner.SetIsChatting(false), owner);
        SchedulingManager.Instance.AddEntry(dueDate, () => target.SetIsChatting(false), target);

        owner.PrintLogIfActive(strLog);
    }
    #endregion

    #region Break Up
    //Char1 decreased his/her opinion of char2
    private void OnOpinionDecreased(Character char1, Character char2) {
        if(char1 == owner) {
            char1.interruptComponent.TriggerInterrupt(INTERRUPT.Break_Up, char2);
        }
    }
    public void NormalBreakUp(Character target) {
        if (UnityEngine.Random.Range(0, 5) == 0) {
            RELATIONSHIP_TYPE relationship = owner.relationshipContainer.GetRelationshipFromParametersWith(target, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.PARAMOUR);
            if (relationship != RELATIONSHIP_TYPE.NONE) {
                if (owner.opinionComponent.GetTotalOpinion(target) < -25) {
                    TriggerBreakUp(target, relationship);
                }
            }
        }
    }
    private void TriggerBreakUp(Character target, RELATIONSHIP_TYPE relationship) {
        RelationshipManager.Instance.RemoveRelationshipBetween(owner, target, relationship);
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
        //upon break up, if one of them still has a Positive opinion of the other, he will gain Heartbroken trait
        if (owner.opinionComponent.GetTotalOpinion(target) > 0) {
            owner.traitContainer.AddTrait(owner, "Heartbroken", target);
        }
        if (target.opinionComponent.GetTotalOpinion(owner) > 0) {
            target.traitContainer.AddTrait(target, "Heartbroken", owner);
        }
        RelationshipManager.Instance.CreateNewRelationshipBetween(owner, target, RELATIONSHIP_TYPE.EX_LOVER);

        Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Break Up", "break_up");
        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        owner.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
    }
    #endregion

    #region Flirt
    public bool NormalFlirtCharacter(Character target) {
        if (target.isDead
            || !target.canWitness
            || !owner.canWitness
            || target.role.roleType == CHARACTER_ROLE.BEAST
            || owner.role.roleType == CHARACTER_ROLE.BEAST
            || target.faction.isPlayerFaction
            || owner.faction.isPlayerFaction
            || target.characterClass.className == "Zombie"
            || owner.characterClass.className == "Zombie"
            || (owner.currentActionNode != null && owner.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)
            || (target.currentActionNode != null && target.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)
            //|| owner.isChatting
            //|| target.isChatting
            ) {
            return false;
        }
        if (!owner.IsHostileWith(target)) {
            string result = TriggerFlirtCharacter(target);
            Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Flirt", result);
            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            owner.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
            return true;
        }
        return false;
    }
    private string TriggerFlirtCharacter(Character target) {
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 50) {
            if (owner.traitContainer.GetNormalTrait<Trait>("Ugly") != null) {
                owner.opinionComponent.AdjustOpinion(target, "Base", -4);
                target.opinionComponent.AdjustOpinion(owner, "Base", -2);
                return "ugly";
            }
        }
        if(chance < 90) {
            if(!RelationshipManager.Instance.IsSexuallyCompatibleOneSided(target, owner)) {
                owner.opinionComponent.AdjustOpinion(target, "Base", -4);
                target.opinionComponent.AdjustOpinion(owner, "Base", -2);
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
            && owner.relationshipValidator.CanHaveRelationship(owner, target, RELATIONSHIP_TYPE.PARAMOUR)
            && target.relationshipValidator.CanHaveRelationship(target, owner, RELATIONSHIP_TYPE.PARAMOUR)) {
            RelationshipManager.Instance.CreateNewRelationshipBetween(owner, target, RELATIONSHIP_TYPE.PARAMOUR);
            return "flirted_back";
        }
        return "flirted_back";
    }
    #endregion
}
