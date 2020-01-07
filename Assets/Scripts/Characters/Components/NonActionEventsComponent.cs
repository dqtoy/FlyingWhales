using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class NonActionEventsComponent {
    public Character owner { get; private set; }

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
        int chance = UnityEngine.Random.Range(0, 100);
        string result = string.Empty;
        if(chance < 75) {
            target.opinionComponent.AdjustOpinion(owner, "Good Chat", 2);
            owner.opinionComponent.AdjustOpinion(target, "Good Chat", 2);
            result = "good";
        } else if (chance < 25) {
            target.opinionComponent.AdjustOpinion(owner, "Awkward Chat", -2);
            owner.opinionComponent.AdjustOpinion(target, "Awkward Chat", 2);
            result = "awkward";
        } else {
            target.opinionComponent.AdjustOpinion(owner, "Argument", -5);
            owner.opinionComponent.AdjustOpinion(target, "Argument", -5);
            result = "argument";
        }
        GameDate dueDate = GameManager.Instance.Today();
        Log log = new Log(dueDate, "Interrupt", "Chat", "chat_" + result);
        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        owner.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        owner.SetIsChatting(true);
        target.SetIsChatting(true);

        dueDate.AddTicks(2);
        SchedulingManager.Instance.AddEntry(dueDate, () => owner.SetIsChatting(false), owner);
        SchedulingManager.Instance.AddEntry(dueDate, () => target.SetIsChatting(false), target);
    }
    #endregion

    #region Break Up
    //Char1 decreased his/her opinion of char2
    private void OnOpinionDecreased(Character char1, Character char2) {
        if(char1 == owner) {
            NormalBreakUp(char2);
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
            if (owner.homeRegion.area != null) {
                owner.homeRegion.area.AssignCharacterToDwellingInArea(owner);
            }
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
            TriggerFlirtCharacter(target);
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
        if(opinionLabel == "Acquaintance") {
            value = 1;
        }else if (opinionLabel == "Friend" || opinionLabel == "Close Friend") {
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
