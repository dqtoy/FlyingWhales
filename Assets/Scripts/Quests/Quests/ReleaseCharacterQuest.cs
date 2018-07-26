using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;
using UnityEngine;

public class ReleaseCharacterQuest : Quest {

    protected Character _targetCharacter;

    #region getters.setters
    public Character targetCharacter {
        get { return _targetCharacter; }
    }
    public override GROUP_TYPE groupType { get { return GROUP_TYPE.PARTY; } }
    #endregion

    public ReleaseCharacterQuest(Character targetCharacter) : base(QUEST_TYPE.RELEASE_CHARACTER) {
        _targetCharacter = targetCharacter;
        Messenger.AddListener<Character>(Signals.CHARACTER_RELEASED, OnCharacterReleased);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnTargetDied);
    }

    #region overrides
    //public override QuestAction GetQuestAction(Character character, CharacterQuestData data, ref IObject targetObject) {
    //    ReleaseCharacterQuestData questData = data as ReleaseCharacterQuestData;
    //    if (character.party.computedPower >= questData.requiredPower) { //if current power is greater than or equal to Required Power
    //        if (questData.HasHostilesInPath()) { //check if there are hostiles along the path
    //            //if yes, inspect nearest hostile along the path
    //            NewParty nearestHostile = questData.GetFirstHostileInPath();
    //            if (nearestHostile.computedPower <= character.party.computedPower) { //if within power range, Attack action
    //                data.SetLastActionDesperateState(false);
    //                targetObject = nearestHostile.icharacterObject;
    //                return nearestHostile.icharacterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
    //            } else { //if above power range, set Required Power value
    //                questData.SetRequiredPower(nearestHostile.computedPower);
    //            }
    //        } else { //if no, Release target
    //            data.SetLastActionDesperateState(false);
    //            targetObject = targetCharacter.party.characterObject;
    //            return targetCharacter.party.characterObject.currentState.GetAction(ACTION_TYPE.RELEASE);
    //        }
    //    }

    //    if (character.role == null || character.role.roleType == CHARACTER_ROLE.CIVILIAN) { //if character is a Civilian, Change Role to Hero, randomize class
    //        character.AssignRole(CHARACTER_ROLE.HERO);
    //        List<string> choices = CharacterManager.Instance.GetNonCivilianClasses();
    //        string chosenClass = choices[Random.Range(0, choices.Count)];
    //        character.AssignClass(CharacterManager.Instance.classesDictionary[chosenClass].CreateNewCopy());
    //    }

    //    //if character is a Hero, and Gain Power Type is None, check which Gain Power Type options are available
    //    if (character.role.roleType == CHARACTER_ROLE.HERO && questData.gainPowerType == ReleaseCharacterQuestData.Gain_Power_Type.None) {
    //        List<ReleaseCharacterQuestData.Gain_Power_Type> availablePowerSources = new List<ReleaseCharacterQuestData.Gain_Power_Type>();
    //        if (CharacterManager.Instance.HasCharacterWithClass("Retired Hero")) {
    //            //if there is a Retired Hero from non-hostile Factions with no negative relationship to the character, Mentor is available
    //            questData.SetElligibleMentors(GetElligibleMentors(character));
    //            if (questData.elligibleMentors != null && questData.elligibleMentors.Count > 0) {
    //                availablePowerSources.Add(ReleaseCharacterQuestData.Gain_Power_Type.Mentor);
    //            }
    //        }
    //        //if (IsShopAvailable(character)) {
    //        //    //if there is a Shop from non-hostile settlements, Equipment is available
    //        //    availablePowerSources.Add(ReleaseCharacterQuestData.Gain_Power_Type.Equipment);
    //        //}
    //        if (IsDungeonTypeAreaAvailable(character)) {
    //            //if there is at least one Dungeon type area in the region, Hunt is available
    //            availablePowerSources.Add(ReleaseCharacterQuestData.Gain_Power_Type.Hunt);
    //        }
    //        //randomize between available options and set it as Gain Power Type
    //        if (availablePowerSources.Count > 0) {
    //            questData.SetGainPowerType(availablePowerSources[Utilities.rng.Next(0, availablePowerSources.Count)]);
    //        }
    //    }

    //    if (questData.gainPowerType == ReleaseCharacterQuestData.Gain_Power_Type.None) {
    //        //if Gain Power Type is None
    //        if (!data.lastActionWasDesperate) {
    //            //perform character's Desperate Action
    //            data.SetLastActionDesperateState(true);
    //            //targetObject = character.party.icharacterObject;
    //            return character.GetRandomDesperateAction(ref targetObject);
    //        } else {
    //            //after performing the Desperate Action, check again if there is a Gain Power Type available, 
    //            //if still none, 50% chance to perform character's Desperate Action and loop this again, 50% chance to abandon Quest
    //            if (Utilities.rng.Next(0, 2) == 0) {
    //                data.SetLastActionDesperateState(true);
    //                //targetObject = character.party.icharacterObject;
    //                return character.GetRandomDesperateAction(ref targetObject);
    //            } else {
    //                data.AbandonQuest(); //abandon quest
    //            }
    //        }
    //    } else {
    //        //if Gain Power Type is not None
    //        //perform action, based on gain power type
    //        switch (questData.gainPowerType) {
    //            case ReleaseCharacterQuestData.Gain_Power_Type.Mentor:
    //                return GetMentorAction(character, questData, ref targetObject);
    //            case ReleaseCharacterQuestData.Gain_Power_Type.Equipment:
    //                break;
    //            case ReleaseCharacterQuestData.Gain_Power_Type.Hunt:
    //                return GetHuntAction(character, questData, ref targetObject);
    //            default:
    //                break;
    //        }

    //    }
    //    return base.GetQuestAction(character, data, ref targetObject);
    //}
    public override QuestAction GetQuestAction(Character character, CharacterQuestData data) {
        ReleaseCharacterQuestData questData = data as ReleaseCharacterQuestData;
        if (questData.HasHostilesInPath()) { //check if there are hostiles along the path
            NewParty nearestHostile = questData.GetFirstHostileInPath(); //if yes, inspect nearest hostile along the path
            //Attack action, set hostile group's power as Required Power
            return new QuestAction(nearestHostile.icharacterObject.currentState.GetAction(ACTION_TYPE.ATTACK), nearestHostile.icharacterObject, nearestHostile.computedPower);
        } else {
            //if no, Release target, set 0 as Required Power
            return new QuestAction(targetCharacter.party.characterObject.currentState.GetAction(ACTION_TYPE.RELEASE), targetCharacter.party.characterObject);
        }
    }
    protected override string GetQuestName() {
        return "Release " + _targetCharacter.name;
    }
    protected override string GetQuestDescription() {
        return "Release " + _targetCharacter.name + " from imprisonment.";
    }
    #endregion

    private void OnCharacterReleased(Character releasedCharacter) {
        if (releasedCharacter.id == targetCharacter.id) {
            //Set Quest as finished
            QuestManager.Instance.OnQuestDone(this);
            RemoveListerners();
        }
    }
    private void OnTargetDied(Character characterThatDied) {
        if (characterThatDied.id == targetCharacter.id) {
            //Set Quest as finished
            QuestManager.Instance.OnQuestDone(this);
            RemoveListerners();
        }
    }
    private void RemoveListerners() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_RELEASED, OnCharacterReleased);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnTargetDied);
    }

    ///*
    // Get a list of mentors that do not have negative relationships 
    // with the character or the character with the mentor.
    //     */
    //private List<Character> GetElligibleMentors(Character character) {
    //    if (character.faction == null) {
    //        Debug.LogWarning(character.name + " is trying to get elligible mentors, but it does not have a faction!");
    //        return null;
    //    }
    //    List<Character> elligibleMentors = new List<Character>();
    //    List<Character> mentors = CharacterManager.Instance.GetCharactersWithClass("Retired Hero");
    //    List<Faction> nonHostileFactions = FactionManager.Instance.GetFactionsWithByStatus(character.faction, FACTION_RELATIONSHIP_STATUS.NON_HOSTILE);
    //    for (int i = 0; i < mentors.Count; i++) {
    //        Character currMentor = mentors[i];
    //        if (currMentor.party.characterObject.currentState.GetAction(ACTION_TYPE.ENROLL) == null||
    //            currMentor.party.characterObject.currentState.GetAction(ACTION_TYPE.TRAIN) == null) {
    //            continue; //the mentor does not have an enroll or train action
    //        }
    //        if (currMentor.faction == null || !nonHostileFactions.Contains(currMentor.faction)) {
    //            //the current mentor does not have a faction or the faction of the mentor is hostile to the faction of the character seeking a mentor
    //            continue; //skip
    //        }
    //        Relationship relMentor = currMentor.GetRelationshipWith(character);
    //        Relationship relCharacter = character.GetRelationshipWith(currMentor);
    //        if (relMentor == null && relCharacter == null) {
    //            elligibleMentors.Add(currMentor); //both mentor and character do not have relationships with each other, allow.
    //        } else if (relMentor != null && !relMentor.IsNegative()) { //the mentor has a relationship with the character and is not negative
    //            if (relCharacter == null || !relCharacter.IsNegative()) {
    //                //the character does not have a relationship with the mentor OR the character's relationship with the mentor is not negative, allow.
    //                elligibleMentors.Add(currMentor);
    //            }
    //        } else if (relCharacter != null && !relCharacter.IsNegative()) { //the character has a relationship with the mentor and is not negative
    //            if (relMentor == null || !relMentor.IsNegative()) {
    //                //the mentor does not have a relationship with the character OR the mentor's relationship with the character is not negative, allow.
    //                elligibleMentors.Add(currMentor);
    //            }
    //        }
    //    }

    //    return elligibleMentors;
    //}
    //private bool IsShopAvailable(Character character) {
    //    if (character.faction == null) {
    //        Debug.LogWarning(character.name + " is trying to get elligible shops, but it does not have a faction!");
    //        return false;
    //    }
    //    List<Faction> nonHostileFactions = FactionManager.Instance.GetFactionsWithByStatus(character.faction, FACTION_RELATIONSHIP_STATUS.NON_HOSTILE);
    //    for (int i = 0; i < nonHostileFactions.Count; i++) {
    //        Faction currFaction = nonHostileFactions[i];
    //        for (int j = 0; j < currFaction.ownedAreas.Count; j++) {
    //            Area area = currFaction.ownedAreas[j];
    //            if (area.HasLandmarkOfType(LANDMARK_TYPE.SHOP)) {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}
    //private bool IsDungeonTypeAreaAvailable(Character character) {
    //    Region regionOfChar = character.specificLocation.tileLocation.region;
    //    List<Area> areas = regionOfChar.GetAreasInRegion();
    //    for (int i = 0; i < areas.Count; i++) {
    //        Area currArea = areas[i];
    //        if (currArea.GetBaseAreaType() == BASE_AREA_TYPE.DUNGEON) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //#region Gain Power Type Actions
    //private CharacterAction GetMentorAction(Character character, ReleaseCharacterQuestData data, ref IObject targetObject) {
    //    if (IsCharacterStudent(character)) { //If character is already a Student of another character.
    //        //NOTE: This already has checking for if the mentor is dead, since this checks the characters relationships, and once a character dies, relationships are removed
    //        Character mentor = character.GetCharacterWithRelationshipStatus(CHARACTER_RELATIONSHIP.MENTOR);
    //        CharacterAction trainAction = mentor.party.characterObject.currentState.GetAction(ACTION_TYPE.TRAIN);
    //        if (trainAction != null && trainAction.CanBeDoneBy(character.party, mentor.party.characterObject)) { //if Train action is available
    //            //perform Train action on the Mentor, avoid strong hostiles
    //            data.SetGainPowerType(ReleaseCharacterQuestData.Gain_Power_Type.None);//set Gain Power Type to None
    //            targetObject = mentor.party.characterObject;
    //            return trainAction;
    //        } else { //if Train action is not available
    //            return character.GetRandomIdleAction(ref targetObject); //perform character's Idle Action
    //        }
    //    } else {//If character is not yet a Student, find nearest Retired Hero character (no negative relationships only and not yet Student of that character)
    //        List<Character> mentors = data.elligibleMentors.OrderBy(x => character.specificLocation.tileLocation.GetDistanceTo(x.specificLocation.tileLocation)).ToList();
    //        Character nearestMentor = mentors[0];
    //        character.party.icon.SetMovementType(MOVEMENT_TYPE.AVOID);
    //        targetObject = nearestMentor.party.characterObject;
    //        return nearestMentor.party.characterObject.currentState.GetAction(ACTION_TYPE.ENROLL); //perform Enroll action on the Retired Hero, avoid strong hostiles
    //    }
    //}
    //private CharacterAction GetHuntAction(Character character, ReleaseCharacterQuestData data, ref IObject targetObject) {
    //    // List all hostile parties within the region whose Power is lower than character by at least 10%
    //    List<IParty> hostiles = GetHostileCharactersFor(character);
    //    //If none are available, perform Idle Action. Loop. If Idle Action performed 5 times in a row, set Gain Power Type to None
    //    if (hostiles.Count > 0) {
    //        //Randomly select one and perform Attack action
    //        IParty chosenParty = hostiles[Random.Range(0, hostiles.Count)];
    //        data.OnChooseHuntCharacter(chosenParty);
    //        targetObject = chosenParty.icharacterObject;
    //        return chosenParty.icharacterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
    //    } else {
    //        //If none are available, perform Idle Action. Loop. If Idle Action performed 5 times in a row, set Gain Power Type to None
    //        data.OnDoIdleActionFromHunt();
    //        if (data.idleActionsInARow >= 5) {
    //            data.ResetIdleActions();
    //            data.SetGainPowerType(ReleaseCharacterQuestData.Gain_Power_Type.None);
    //        }
    //        //targetObject = character.party.characterObject;
    //        return character.GetRandomIdleAction(ref targetObject);
    //    }
    //}
    //#endregion

    //#region Utilities
    //private bool IsCharacterStudent(Character character) {
    //    return character.AlreadyHasRelationshipStatus(CHARACTER_RELATIONSHIP.MENTOR); //check if character has a character that he considers as his/her mentor
    //}
    //private List<IParty> GetHostileCharactersFor(Character character) {
    //    List<IParty> hostileCharacters = new List<IParty>();
    //    Region regionOfChar = character.specificLocation.tileLocation.region;
    //    // List all hostile parties within the region whose Power is lower than character by at least 10%
    //    for (int i = 0; i < regionOfChar.landmarks.Count; i++) {
    //        BaseLandmark baseLandmark = regionOfChar.landmarks[i];
    //        for (int j = 0; j < baseLandmark.charactersAtLocation.Count; j++) {
    //            IParty currParty = baseLandmark.charactersAtLocation[j];
    //            if (currParty.id == character.party.id) {
    //                continue; //skip
    //            }
    //            float powerComparison = GetPowerComparison(character.party, currParty);
    //            if (powerComparison < 0.10f) {
    //                continue; //skip. power comparison is less than 10%
    //            }
    //            if (currParty is CharacterParty) {
    //                Character partyMainChar = (currParty as CharacterParty).mainCharacter as Character;
    //                Relationship rel = character.GetRelationshipWith(partyMainChar);
    //                if (rel != null && rel.IsNegative()) {
    //                    continue; //skip. relationship is not negative
    //                }
    //            }
    //            hostileCharacters.Add(currParty);
    //        }
    //    }
    //    return hostileCharacters;
    //}
    //private float GetPowerComparison(IParty party1, IParty party2) {
    //    //how much stronger is party1 compared to party2?
    //    float difference = party1.computedPower - party2.computedPower;
    //    return party1.computedPower / difference;
    //}
    //#endregion

    #region Equations
    public override int GetHashCode() {
        return base.GetHashCode();
    }
    public override bool Equals(object obj) {
        if (obj is ECS.Character) {
            return this.Equals(obj as ECS.Character);
        }
        return base.Equals(obj);
    }
    public bool Equals(ECS.Character character) {
        if (character.id == targetCharacter.id) {
            return true;
        }
        return false;
    }
    #endregion

}
