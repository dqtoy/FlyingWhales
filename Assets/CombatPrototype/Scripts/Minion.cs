using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Minion {
    public const int MAX_INTERVENTION_ABILITY_SLOT = 5;

    public Character character { get; private set; }
    public int exp { get; private set; }
    public int indexDefaultSort { get; private set; }
    public int unlockedInterventionSlots { get; private set; }
    //public bool isEnabled { get; private set; }
    public PlayerJobAction[] interventionAbilities { get; private set; }
    public CombatAbility combatAbility { get; private set; }

    public Minion(Character character, bool keepData) {
        this.character = character;
        this.exp = 0;
        this.interventionAbilities = new PlayerJobAction[MAX_INTERVENTION_ABILITY_SLOT];
        SetUnlockedInterventionSlots(0);
        character.SetMinion(this);
        //character.characterToken.SetObtainedState(true);
        character.ownParty.icon.SetVisualState(true);

        if (!keepData) {
            character.SetName(RandomNameGenerator.Instance.GenerateMinionName());
        }
    }
    //public void SetEnabledState(bool state) {
    //    if (character.IsInOwnParty()) {
    //        //also set enabled state of other party members
    //        for (int i = 0; i < character.ownParty.characters.Count; i++) {
    //            Character otherChar = character.ownParty.characters[i];
    //            if (otherChar.id != character.id && otherChar.minion != null) {
    //                otherChar.minion.SetEnabledState(state);
    //                if (state) {
    //                    //Since the otherChar will be removed from the party when he is not the owner and state is true, reduce loop count so no argument exception error will be called
    //                    i--;
    //                }
    //            }
    //        }
    //    } else {
    //        //If character is not own party and is enabled, automatically put him in his own party so he can be used again
    //        if (state) {
    //            character.currentParty.RemoveCharacter(character);
    //        }
    //    }
    //    _isEnabled = state;
    //    minionItem.SetEnabledState(state);
    //}
    public void SetPlayerCharacterItem(PlayerCharacterItem item) {
        character.SetPlayerCharacterItem(item);
    }
    public void AdjustExp(int amount) {
        exp += amount;
        if(exp >= 100) {
            LevelUp();
            exp = 0;
        }else if (exp < 0) {
            exp = 0;
        }
        //_characterItem.UpdateMinionItem();
    }
    public void SetLevel(int level) {
        character.SetLevel(level);
    }
    public void LevelUp() {
        character.LevelUp();
    }
    public void LevelUp(int amount) {
        character.LevelUp(amount);
    }
    public void SetIndexDefaultSort(int index) {
        indexDefaultSort = index;
    }

    #region Intervention Abilities
    public void SetUnlockedInterventionSlots(int amount) {
        unlockedInterventionSlots = amount;
        unlockedInterventionSlots = Mathf.Clamp(unlockedInterventionSlots, 0, MAX_INTERVENTION_ABILITY_SLOT);
    }
    public void AdjustUnlockedInterventionSlots(int amount) {
        unlockedInterventionSlots += amount;
        unlockedInterventionSlots = Mathf.Clamp(unlockedInterventionSlots, 0, MAX_INTERVENTION_ABILITY_SLOT);
    }
    public void AddInterventionAbility(PlayerJobAction ability) {
        int currentInterventionAbilityCount = GetCurrentInterventionAbilityCount();
        if(currentInterventionAbilityCount < unlockedInterventionSlots) {
            for (int i = 0; i < interventionAbilities.Length; i++) {
                if (interventionAbilities[i] == null) {
                    interventionAbilities[i] = ability;
                    break;
                }
            }
        } else {
            //Broadcast intervention ability is full, must open UI whether player wants to replace ability or discard it
        }
    }
    public int GetCurrentInterventionAbilityCount() {
        int count = 0;
        for (int i = 0; i < interventionAbilities.Length; i++) {
            if (interventionAbilities[i] != null) {
                count++;
            }
        }
        return count;
    }
    #endregion

    #region Combat Ability
    public void SetCombatAbility(CombatAbility combatAbility) {
        this.combatAbility = combatAbility;
    }
    #endregion
}
