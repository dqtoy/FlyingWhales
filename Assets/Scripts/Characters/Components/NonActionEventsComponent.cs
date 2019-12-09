using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            target.opinionComponent.AdjustOpinion(owner, "Base", 3);
            result = "good";
        } else if (chance < 25) {
            target.opinionComponent.AdjustOpinion(owner, "Base", -2);
            result = "awkward";
        } else {
            target.opinionComponent.AdjustOpinion(owner, "Base", -5);
            owner.opinionComponent.AdjustOpinion(target, "Base", -5);
            result = "argument";
        }
        GameDate dueDate = GameManager.Instance.Today();
        Log log = new Log(dueDate, "Character", "NonIntel", "chat_" + result);
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
            RELATIONSHIP_TRAIT relationship = owner.relationshipContainer.GetRelationshipFromParametersWith(target, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR);
            if (relationship != RELATIONSHIP_TRAIT.NONE) {
                if (owner.opinionComponent.GetTotalOpinion(target) < -25) {
                    TriggerBreakUp(target, relationship);
                }
            }
        }

    }
    private void TriggerBreakUp(Character target, RELATIONSHIP_TRAIT relationship) {
        RelationshipManager.Instance.RemoveRelationshipBetween(owner, target, relationship);
        if (relationship == RELATIONSHIP_TRAIT.LOVER) {
            //**Effect 1**: Actor - Remove Lover relationship with Character 2
            //if the relationship that was removed is lover, change home to a random unoccupied dwelling,
            //otherwise, no home. Reference: https://trello.com/c/JUSt9bEa/1938-broken-up-characters-should-live-in-separate-house
            owner.MigrateHomeStructureTo(null);
            if (owner.homeArea != null) {
                owner.homeArea.AssignCharacterToDwellingInArea(owner);
            }
        }
        //upon break up, if one of them still has a Positive opinion of the other, he will gain Heartbroken trait
        if (owner.opinionComponent.GetTotalOpinion(target) > 0) {
            owner.traitContainer.AddTrait(owner, "Heartbroken", target);
        }
        if (target.opinionComponent.GetTotalOpinion(owner) > 0) {
            target.traitContainer.AddTrait(target, "Heartbroken", owner);
        }
        RelationshipManager.Instance.CreateNewRelationshipBetween(owner, target, RELATIONSHIP_TRAIT.EX_LOVER);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "break_up");
        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        owner.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
    }
    #endregion
}
