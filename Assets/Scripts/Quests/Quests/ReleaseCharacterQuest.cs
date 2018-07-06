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

        //if (character.role == null || character.role.roleType == CHARACTER_ROLE.CIVILIAN) { //if character is a Civilian, Change Role to Hero, randomize class
        //    character.AssignRole(CHARACTER_ROLE.HERO);
        //    List<string> choices = CharacterManager.Instance.GetNonCivilianClasses();
        //    string chosenClass = choices[Random.Range(0, choices.Count)];
        //    character.AssignClass(CharacterManager.Instance.classesDictionary[chosenClass].CreateNewCopy());
        //}

        ////if character is a Hero, and Gain Power Type is None, check which Gain Power Type options are available
        //if (character.role.roleType == CHARACTER_ROLE.HERO && questData.gainPowerType == ReleaseCharacterQuestData.Gain_Power_Type.None) {
        //    List<ReleaseCharacterQuestData.Gain_Power_Type> availablePowerSources = new List<ReleaseCharacterQuestData.Gain_Power_Type>();
        //    //- if there is a Retired Hero from non-hostile Factions with no negative relationship to the character, Mentor is available
        //    if (CharacterManager.Instance.HasCharacterWithJob(CHARACTER_JOB.RETIRED_HERO)) { //TODO: Add faction relationship checking

        //    }
        //    //- if there is a Shop from non-hostile settlements, Equipment is available

        //    // - if there is at least one Dungeon type area in the region, Hunt is available
        //    //- randomize between available options and set it as Gain Power Type
        //}
        return base.GetQuestAction(character,data);
    }
    #endregion

    private List<Character> GetElligibleMentors() {
        List<Character> elligibleMentors = new List<Character>();

        return elligibleMentors;
    }
}
