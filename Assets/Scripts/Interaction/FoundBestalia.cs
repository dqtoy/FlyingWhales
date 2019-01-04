using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundBestalia : Interaction {

    private const string Start = "Start";
    private const string Turn_Success = "Turn Success";
    private const string Turn_Fail = "Turn Fail";
    private const string Turn_Critical_Fail = "Turn Critical Fail";
    private const string Alliance_Success = "Alliance Success";
    private const string Alliance_Fail = "Alliance Fail";
    private const string Alliance_Critical_Fail = "Alliance Critical Fail";
    private const string Dissuade_Success = "Dissuade Success";
    private const string Dissuade_Fail = "Dissuade Fail";
    private const string Bestalia_Founded = "Bestalia Founded";

    public FoundBestalia(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.FOUND_BESTALIA, 0) {
        _name = "Found Bestalia";
        _jobFilter = new JOB[] { JOB.DISSUADER, JOB.DIPLOMAT, JOB.INSTIGATOR };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState turnSuccessState = new InteractionState(Turn_Success, this);
        InteractionState turnFailState = new InteractionState(Turn_Fail, this);
        InteractionState turnCritFailState = new InteractionState(Turn_Critical_Fail, this);
        InteractionState allianceSuccessState = new InteractionState(Alliance_Success, this);
        InteractionState allianceFailState = new InteractionState(Alliance_Fail, this);
        InteractionState allianceCritFailState = new InteractionState(Alliance_Critical_Fail, this);
        InteractionState dissuadeSuccessState = new InteractionState(Dissuade_Success, this);
        InteractionState dissuadeFailState = new InteractionState(Dissuade_Fail, this);
        InteractionState bestaliaFoundedState = new InteractionState(Bestalia_Founded, this);

        CreateActionOptions(startState);

        turnSuccessState.SetEffect(() => TurnSuccessEffect(turnSuccessState));
        turnFailState.SetEffect(() => TurnFailEffect(turnFailState));
        turnCritFailState.SetEffect(() => TurnCritFailEffect(turnCritFailState));
        allianceSuccessState.SetEffect(() => AllianceSuccessEffect(allianceSuccessState));
        allianceFailState.SetEffect(() => AllianceFailEffect(allianceFailState));
        allianceCritFailState.SetEffect(() => AllianceCritFailEffect(allianceCritFailState));
        dissuadeSuccessState.SetEffect(() => DissuadeSuccessEffect(dissuadeSuccessState));
        dissuadeFailState.SetEffect(() => DissuadeFailEffect(dissuadeFailState));
        bestaliaFoundedState.SetEffect(() => BestaliaFoundEffect(bestaliaFoundedState));

        _states.Add(startState.name, startState);
        _states.Add(turnSuccessState.name, turnSuccessState);
        _states.Add(turnFailState.name, turnFailState);
        _states.Add(turnCritFailState.name, turnCritFailState);
        _states.Add(allianceSuccessState.name, allianceSuccessState);
        _states.Add(allianceFailState.name, allianceFailState);
        _states.Add(allianceCritFailState.name, allianceCritFailState);
        _states.Add(dissuadeSuccessState.name, dissuadeSuccessState);
        _states.Add(dissuadeFailState.name, dissuadeFailState);
        _states.Add(bestaliaFoundedState.name, bestaliaFoundedState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption turnOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Turn him against his former faction: " + _characterInvolved.faction.name + ".",
                disabledTooltipText = "Must be an Instigator.",
                jobNeeded = JOB.INSTIGATOR,
                effect = () => TurnOption(),
            };
            ActionOption allyOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Establish an alliance with him.",
                disabledTooltipText = "Must be a Diplomat.",
                jobNeeded = JOB.DIPLOMAT,
                effect = () => AllyOption(),
            };
            ActionOption dissuadeOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Dissuade him from the idea.",
                disabledTooltipText = "Must be a Dissuader.",
                jobNeeded = JOB.DISSUADER,
                effect = () => DissuadeOption(),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(turnOption);
            state.AddActionOption(allyOption);
            state.AddActionOption(dissuadeOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private void TurnOption() {
        Job job = investigatorMinion.character.job;
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Turn_Success, job.GetSuccessRate());
        effectWeights.AddElement(Turn_Fail, job.GetFailRate());
        effectWeights.AddElement(Turn_Critical_Fail, job.GetCritFailRate());
        string result = effectWeights.PickRandomElementGivenWeights();

        SetCurrentState(_states[result]);
    }
    private void AllyOption() {
        Job job = investigatorMinion.character.job;
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Alliance_Success, job.GetSuccessRate());
        effectWeights.AddElement(Alliance_Fail, job.GetFailRate());
        effectWeights.AddElement(Alliance_Critical_Fail, job.GetCritFailRate());
        string result = effectWeights.PickRandomElementGivenWeights();

        SetCurrentState(_states[result]);
    }
    private void DissuadeOption() {
        Job job = investigatorMinion.character.job;
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Dissuade_Success, job.GetSuccessRate());
        effectWeights.AddElement(Dissuade_Fail, job.GetFailRate());
        string result = effectWeights.PickRandomElementGivenWeights();

        SetCurrentState(_states[result]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Bestalia_Founded]);
    }
    #endregion

    #region State Effects
    private void TurnSuccessEffect(InteractionState state) {
        investigatorMinion.LevelUp();

        //Remove character from her current Faction and turn her into the Faction Leader of a new Bestalia faction. Current area becomes owned by Bestalia faction, set its race to the character's race.
        _characterInvolved.SetForcedInteraction(null);
        Faction oldFaction = _characterInvolved.faction;
        _characterInvolved.faction.RemoveCharacter(_characterInvolved);
        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName("Bestalia");
        newFaction.SetLeader(_characterInvolved);
        newFaction.AddNewCharacter(_characterInvolved);
        FactionManager.Instance.neutralFaction.UnownArea(interactable.tileLocation.areaOfTile);
        LandmarkManager.Instance.OwnArea(newFaction, _characterInvolved.race, interactable.tileLocation.areaOfTile);

        //Set Bestalia faction to Enemy of character's original faction
        newFaction.GetRelationshipWith(oldFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.ENEMY);
        newFaction.SetFactionActiveState(true);

        //All beasts in the location will change faction and home to the current location. Spawn 2 new characters in the location or until the resident capacity has been reached. Race is the same as the character's. 
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            if (interactable.tileLocation.areaOfTile.IsResidentsFull()) {
                break;
            }
            Character character = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (character.role.roleType == CHARACTER_ROLE.BEAST && !character.currentParty.icon.isTravelling) {
                if (!character.IsInOwnParty()) {
                    character.currentParty.RemoveCharacter(character);
                }
                character.MigrateTo(character.specificLocation as BaseLandmark);
                character.ChangeFactionTo(newFaction);
                character.SetForcedInteraction(null);
            }
        }
        interactable.SpawnRandomCharacters(2);

        state.descriptionLog.AddToFillers(newFaction, newFaction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(oldFaction, oldFaction.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(newFaction, newFaction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(oldFaction, oldFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void TurnFailEffect(InteractionState state) {
        //Remove character from her current Faction and turn her into the Faction Leader of a new Bestalia faction. Current area becomes owned by Bestalia faction, set its race to the character's race.
        _characterInvolved.SetForcedInteraction(null);
        Faction oldFaction = _characterInvolved.faction;
        _characterInvolved.faction.RemoveCharacter(_characterInvolved);
        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName("Bestalia");
        newFaction.SetLeader(_characterInvolved);
        newFaction.AddNewCharacter(_characterInvolved);
        FactionManager.Instance.neutralFaction.UnownArea(interactable.tileLocation.areaOfTile);
        LandmarkManager.Instance.OwnArea(newFaction, _characterInvolved.race, interactable.tileLocation.areaOfTile);

        //All beasts in the location will change faction and home to the current location. Spawn 2 new characters in the location or until the resident capacity has been reached. Race is the same as the character's. 
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            if (interactable.tileLocation.areaOfTile.IsResidentsFull()) {
                break;
            }
            Character character = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (character.role.roleType == CHARACTER_ROLE.BEAST && !character.currentParty.icon.isTravelling) {
                if (!character.IsInOwnParty()) {
                    character.currentParty.RemoveCharacter(character);
                }
                character.MigrateTo(character.specificLocation as BaseLandmark);
                character.ChangeFactionTo(newFaction);
                character.SetForcedInteraction(null);
            }
        }
        interactable.SpawnRandomCharacters(2);

        state.descriptionLog.AddToFillers(newFaction, newFaction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(oldFaction, oldFaction.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(newFaction, newFaction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(oldFaction, oldFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void TurnCritFailEffect(InteractionState state) {
        //Remove character from her current Faction and turn her into the Faction Leader of a new Bestalia faction. Current area becomes owned by Bestalia faction, set its race to the character's race.
        _characterInvolved.SetForcedInteraction(null);
        Faction oldFaction = _characterInvolved.faction;
        _characterInvolved.faction.RemoveCharacter(_characterInvolved);
        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName("Bestalia");
        newFaction.SetLeader(_characterInvolved);
        newFaction.AddNewCharacter(_characterInvolved);
        FactionManager.Instance.neutralFaction.UnownArea(interactable.tileLocation.areaOfTile);
        LandmarkManager.Instance.OwnArea(newFaction, _characterInvolved.race, interactable.tileLocation.areaOfTile);

        //Set Bestalia faction to Friend of character's original faction
        newFaction.GetRelationshipWith(oldFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.FRIEND);
        newFaction.SetFactionActiveState(true);

        //All beasts in the location will change faction and home to the current location. Spawn 2 new characters in the location or until the resident capacity has been reached. Race is the same as the character's. 
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            if (interactable.tileLocation.areaOfTile.IsResidentsFull()) {
                break;
            }
            Character character = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (character.role.roleType == CHARACTER_ROLE.BEAST && !character.currentParty.icon.isTravelling) {
                if (!character.IsInOwnParty()) {
                    character.currentParty.RemoveCharacter(character);
                }
                character.MigrateTo(character.specificLocation as BaseLandmark);
                character.ChangeFactionTo(newFaction);
                character.SetForcedInteraction(null);
            }
        }
        interactable.SpawnRandomCharacters(2);

        state.descriptionLog.AddToFillers(newFaction, newFaction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(oldFaction, oldFaction.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(newFaction, newFaction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(oldFaction, oldFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void AllianceSuccessEffect(InteractionState state) {
        investigatorMinion.LevelUp();

        //Remove character from her current Faction and turn her into the Faction Leader of a new Bestalia faction. Current area becomes owned by Bestalia faction, set its race to the character's race.
        _characterInvolved.SetForcedInteraction(null);
        _characterInvolved.faction.RemoveCharacter(_characterInvolved);
        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName("Bestalia");
        newFaction.SetLeader(_characterInvolved);
        newFaction.AddNewCharacter(_characterInvolved);
        FactionManager.Instance.neutralFaction.UnownArea(interactable.tileLocation.areaOfTile);
        LandmarkManager.Instance.OwnArea(newFaction, _characterInvolved.race, interactable.tileLocation.areaOfTile);

        //Set Bestalia faction to Ally of player faction.
        newFaction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.ALLY);
        newFaction.SetFactionActiveState(true);

        //All beasts in the location will change faction and home to the current location. Spawn 2 new characters in the location or until the resident capacity has been reached. Race is the same as the character's. 
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            if (interactable.tileLocation.areaOfTile.IsResidentsFull()) {
                break;
            }
            Character character = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (character.role.roleType == CHARACTER_ROLE.BEAST && !character.currentParty.icon.isTravelling) {
                if (!character.IsInOwnParty()) {
                    character.currentParty.RemoveCharacter(character);
                }
                character.MigrateTo(character.specificLocation as BaseLandmark);
                character.ChangeFactionTo(newFaction);
                character.SetForcedInteraction(null);
            }
        }
        interactable.SpawnRandomCharacters(2);
    }
    private void AllianceFailEffect(InteractionState state) {
        //Remove character from her current Faction and turn her into the Faction Leader of a new Bestalia faction. Current area becomes owned by Bestalia faction, set its race to the character's race.
        _characterInvolved.SetForcedInteraction(null);
        _characterInvolved.faction.RemoveCharacter(_characterInvolved);
        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName("Bestalia");
        newFaction.SetLeader(_characterInvolved);
        newFaction.AddNewCharacter(_characterInvolved);
        FactionManager.Instance.neutralFaction.UnownArea(interactable.tileLocation.areaOfTile);
        LandmarkManager.Instance.OwnArea(newFaction, _characterInvolved.race, interactable.tileLocation.areaOfTile);

        //All beasts in the location will change faction and home to the current location. Spawn 2 new characters in the location or until the resident capacity has been reached. Race is the same as the character's. 
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            if (interactable.tileLocation.areaOfTile.IsResidentsFull()) {
                break;
            }
            Character character = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (character.role.roleType == CHARACTER_ROLE.BEAST && !character.currentParty.icon.isTravelling) {
                if (!character.IsInOwnParty()) {
                    character.currentParty.RemoveCharacter(character);
                }
                character.MigrateTo(character.specificLocation as BaseLandmark);
                character.ChangeFactionTo(newFaction);
                character.SetForcedInteraction(null);
            }
        }
        interactable.SpawnRandomCharacters(2);
    }
    private void AllianceCritFailEffect(InteractionState state) {
        //Remove character from her current Faction and turn her into the Faction Leader of a new Bestalia faction. Current area becomes owned by Bestalia faction, set its race to the character's race.
        _characterInvolved.SetForcedInteraction(null);
        _characterInvolved.faction.RemoveCharacter(_characterInvolved);
        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName("Bestalia");
        newFaction.SetLeader(_characterInvolved);
        newFaction.AddNewCharacter(_characterInvolved);
        FactionManager.Instance.neutralFaction.UnownArea(interactable.tileLocation.areaOfTile);
        LandmarkManager.Instance.OwnArea(newFaction, _characterInvolved.race, interactable.tileLocation.areaOfTile);

        //Set Bestalia faction to Disliked of player faction.
        newFaction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.DISLIKED);
        newFaction.SetFactionActiveState(true);

        //All beasts in the location will change faction and home to the current location. Spawn 2 new characters in the location or until the resident capacity has been reached. Race is the same as the character's. 
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            if (interactable.tileLocation.areaOfTile.IsResidentsFull()) {
                break;
            }
            Character character = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (character.role.roleType == CHARACTER_ROLE.BEAST && !character.currentParty.icon.isTravelling) {
                if (!character.IsInOwnParty()) {
                    character.currentParty.RemoveCharacter(character);
                }
                character.MigrateTo(character.specificLocation as BaseLandmark);
                character.ChangeFactionTo(newFaction);
                character.SetForcedInteraction(null);
            }
        }
        interactable.SpawnRandomCharacters(2);
    }
    private void DissuadeSuccessEffect(InteractionState state) {
        investigatorMinion.LevelUp();
    }
    private void DissuadeFailEffect(InteractionState state) {
        //Remove character from her current Faction and turn her into the Faction Leader of a new Bestalia faction. Current area becomes owned by Bestalia faction, set its race to the character's race.
        _characterInvolved.SetForcedInteraction(null);
        _characterInvolved.faction.RemoveCharacter(_characterInvolved);
        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName("Bestalia");
        newFaction.SetLeader(_characterInvolved);
        newFaction.AddNewCharacter(_characterInvolved);
        FactionManager.Instance.neutralFaction.UnownArea(interactable.tileLocation.areaOfTile);
        LandmarkManager.Instance.OwnArea(newFaction, _characterInvolved.race, interactable.tileLocation.areaOfTile);

        //All beasts in the location will change faction and home to the current location. Spawn 2 new characters in the location or until the resident capacity has been reached. Race is the same as the character's. 
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            if (interactable.tileLocation.areaOfTile.IsResidentsFull()) {
                break;
            }
            Character character = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (character.role.roleType == CHARACTER_ROLE.BEAST && !character.currentParty.icon.isTravelling) {
                if (!character.IsInOwnParty()) {
                    character.currentParty.RemoveCharacter(character);
                }
                character.MigrateTo(character.specificLocation as BaseLandmark);
                character.ChangeFactionTo(newFaction);
                character.SetForcedInteraction(null);
            }
        }
        interactable.SpawnRandomCharacters(2);
    }
    private void BestaliaFoundEffect(InteractionState state) {
        //Remove character from her current Faction and turn her into the Faction Leader of a new Bestalia faction. Current area becomes owned by Bestalia faction, set its race to the character's race.
        _characterInvolved.SetForcedInteraction(null);
        _characterInvolved.faction.RemoveCharacter(_characterInvolved);
        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName("Bestalia");
        newFaction.SetLeader(_characterInvolved);
        newFaction.AddNewCharacter(_characterInvolved);
        FactionManager.Instance.neutralFaction.UnownArea(interactable.tileLocation.areaOfTile);
        LandmarkManager.Instance.OwnArea(newFaction, _characterInvolved.race, interactable.tileLocation.areaOfTile);

        //All beasts in the location will change faction and home to the current location. Spawn 2 new characters in the location or until the resident capacity has been reached. Race is the same as the character's. 
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            if (interactable.tileLocation.areaOfTile.IsResidentsFull()) {
                break;
            }
            Character character = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (character.role.roleType == CHARACTER_ROLE.BEAST && !character.currentParty.icon.isTravelling) {
                if (!character.IsInOwnParty()) {
                    character.currentParty.RemoveCharacter(character);
                }
                character.MigrateTo(character.specificLocation as BaseLandmark);
                character.ChangeFactionTo(newFaction);
                character.SetForcedInteraction(null);
            }
        }
        interactable.SpawnRandomCharacters(2);
    }
    #endregion
}
