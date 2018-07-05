using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class ReleaseCharacterQuest : Quest {
    public ReleaseCharacterQuest() : base(QUEST_TYPE.RELEASE_CHARACTER) {
    }

    #region overrides
    public override CharacterAction GetQuestAction(Character character, CharacterQuestData data) {
        ReleaseCharacterQuestData questData = data as ReleaseCharacterQuestData;
        if (character.party.computedPower >= questData.requiredPower) { //if current power is greater than or equal to Required Power
            if (questData.HasHostilesInPath()) { //check if there are hostiles along the path
                //if yes, inspect nearest hostile along the path
                IParty nearestHostile = questData.GetFirstHostileInPath();
                if (nearestHostile.computedPower <= character.party.computedPower) { //if within power range, Attack action
                    return nearestHostile.icharacterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
                } else { //if above power range, set Required Power value
                    questData.SetRequiredPower(nearestHostile.computedPower);
                }
            } else { //if no, Release target
                return questData.targetCharacter.party.characterObject.currentState.GetAction(ACTION_TYPE.RELEASE);
            }
        }
        return base.GetQuestAction(character,data);
    }
    #endregion
}
