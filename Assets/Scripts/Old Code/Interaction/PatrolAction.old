using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolAction : Interaction {

    private const string Revealed_Patroller_Killed_Character = "Revealed Patroller Killed Character";
    private const string Revealed_Patroller_Injured_Character = "Revealed Patroller Injured Character";
    private const string Revealed_Character_Killed_Patroller = "Revealed Character Killed Patroller";
    private const string Revealed_Character_Injured_Patroller = "Revealed Character Injured Patroller";
    private const string Persuaded_Patrol_Stopped = "Persuaded Patrol Stopped";
    private const string Persuaded_Patroller_Killed_Character = "Persuaded Patroller Killed Character";
    private const string Persuaded_Patroller_Injured_Character = "Persuaded Patroller Injured Character";
    private const string Persuaded_Character_Killed_Patroller = "Persuaded Character Killed Patroller";
    private const string Persuaded_Character_Injured_Patroller = "Persuaded Character Injured Patroller";
    private const string Persuaded_Patrol_Failed = "Persuaded Patrol Failed";
    private const string Normal_Patroller_Killed_Character = "Normal Patroller Killed Character";
    private const string Normal_Patroller_Injured_Character = "Normal Patroller Injured Character";
    private const string Normal_Character_Killed_Patroller = "Normal Character Killed Patroller";
    private const string Normal_Character_Injured_Patroller = "Normal Character Injured Patroller";
    private const string Normal_Patrol_Failed = "Normal Patrol Failed";

    public PatrolAction(Area interactable) 
        : base(interactable, INTERACTION_TYPE.PATROL_ACTION, 0) {
        _name = "Patrol Action";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DEBILITATOR };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState revealedPatrollerKilledCharacter = new InteractionState(Revealed_Patroller_Killed_Character, this);
        InteractionState revealedPatrollerInjuredCharacter = new InteractionState(Revealed_Patroller_Injured_Character, this);
        InteractionState revealedCharacterKilledPatroller = new InteractionState(Revealed_Character_Killed_Patroller, this);
        InteractionState revealedCharacterInjuredPatroller = new InteractionState(Revealed_Character_Injured_Patroller, this);
        InteractionState pursuadedPatrolStopped = new InteractionState(Persuaded_Patrol_Stopped, this);
        InteractionState pursuadedPatrollerKilledCharacter = new InteractionState(Persuaded_Patroller_Killed_Character, this);
        InteractionState pursuadedPatrollerInjuredCharacter = new InteractionState(Persuaded_Patroller_Injured_Character, this);
        InteractionState pursuadedCharacterKilledPatroller = new InteractionState(Persuaded_Character_Killed_Patroller, this);
        InteractionState pursuadedCharacterInjuredPatroller = new InteractionState(Persuaded_Character_Injured_Patroller, this);
        InteractionState pursuadedPatrolFailed = new InteractionState(Persuaded_Patrol_Failed, this);
        InteractionState normalPatrollerKilledCharacter = new InteractionState(Normal_Patroller_Killed_Character, this);
        InteractionState normalPatrollerInjuredCharacter = new InteractionState(Normal_Patroller_Injured_Character, this);
        InteractionState normalCharacterKilledPatroller = new InteractionState(Normal_Character_Killed_Patroller, this);
        InteractionState normalCharacterInjuredPatroller = new InteractionState(Normal_Character_Injured_Patroller, this);
        InteractionState normalPatrolFailed = new InteractionState(Normal_Patrol_Failed, this);

        //Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        //startStateDescriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        //startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        revealedPatrollerKilledCharacter.SetEffect(() => RevealedPatrollerKilledCharacter(revealedPatrollerKilledCharacter));
        revealedPatrollerInjuredCharacter.SetEffect(() => RevealedPatrollerInjuredCharacter(revealedPatrollerInjuredCharacter));
        revealedCharacterKilledPatroller.SetEffect(() => RevealedCharacterKilledPatroller(revealedCharacterKilledPatroller));
        revealedCharacterInjuredPatroller.SetEffect(() => RevealedCharacterInjuredPatroller(revealedCharacterInjuredPatroller));
        pursuadedPatrolStopped.SetEffect(() => PursuadedPatrolStopped(pursuadedPatrolStopped));
        pursuadedPatrollerKilledCharacter.SetEffect(() => PursuadedPatrollerKilledCharacter(pursuadedPatrollerKilledCharacter));
        pursuadedPatrollerInjuredCharacter.SetEffect(() => PursuadedPatrollerInjuredCharacter(pursuadedPatrollerInjuredCharacter));
        pursuadedCharacterKilledPatroller.SetEffect(() => PursuadedCharacterKilledPatroller(pursuadedCharacterKilledPatroller));
        pursuadedCharacterInjuredPatroller.SetEffect(() => PursuadedCharacterInjuredPatroller(pursuadedCharacterInjuredPatroller));
        pursuadedPatrolFailed.SetEffect(() => PursuadedPatrolFailed(pursuadedPatrolFailed));
        normalPatrollerKilledCharacter.SetEffect(() => NormalPatrollerKilledCharacter(normalPatrollerKilledCharacter));
        normalPatrollerInjuredCharacter.SetEffect(() => NormalPatrollerInjuredCharacter(normalPatrollerInjuredCharacter));
        normalCharacterKilledPatroller.SetEffect(() => NormalCharacterKilledPatroller(normalCharacterKilledPatroller));
        normalCharacterInjuredPatroller.SetEffect(() => NormalCharacterInjuredPatroller(normalCharacterInjuredPatroller));
        normalPatrolFailed.SetEffect(() => NormalPatrolFailed(normalPatrolFailed));
        

        _states.Add(startState.name, startState);
        _states.Add(revealedPatrollerKilledCharacter.name, revealedPatrollerKilledCharacter);
        _states.Add(revealedPatrollerInjuredCharacter.name, revealedPatrollerInjuredCharacter);
        _states.Add(revealedCharacterKilledPatroller.name, revealedCharacterKilledPatroller);
        _states.Add(revealedCharacterInjuredPatroller.name, revealedCharacterInjuredPatroller);
        _states.Add(pursuadedPatrolStopped.name, pursuadedPatrolStopped);
        _states.Add(pursuadedPatrollerKilledCharacter.name, pursuadedPatrollerKilledCharacter);
        _states.Add(pursuadedPatrollerInjuredCharacter.name, pursuadedPatrollerInjuredCharacter);
        _states.Add(pursuadedCharacterKilledPatroller.name, pursuadedCharacterKilledPatroller);
        _states.Add(pursuadedCharacterInjuredPatroller.name, pursuadedCharacterInjuredPatroller);
        _states.Add(pursuadedPatrolFailed.name, pursuadedPatrolFailed);
        _states.Add(normalPatrollerKilledCharacter.name, normalPatrollerKilledCharacter);
        _states.Add(normalPatrollerInjuredCharacter.name, normalPatrollerInjuredCharacter);
        _states.Add(normalCharacterKilledPatroller.name, normalCharacterKilledPatroller);
        _states.Add(normalCharacterInjuredPatroller.name, normalCharacterInjuredPatroller);
        _states.Add(normalPatrolFailed.name, normalPatrolFailed);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption reveal = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Reveal an enemy's whereabouts.",
                effect = () => RevealOptionEffect(state),
                neededObjects = new List<System.Type>() { typeof(CharacterToken) },
                neededObjectsChecker = new List<ActionOptionNeededObjectChecker>() {
                    new ActionOptionLocationRequirement {
                        requiredLocation = interactable,
                    },
                    new ActionOptionFactionRelationshipRequirement {
                        requiredStatus = new List<FACTION_RELATIONSHIP_STATUS>(){ FACTION_RELATIONSHIP_STATUS.DISLIKED, FACTION_RELATIONSHIP_STATUS.ENEMY },
                        sourceCharacter = _characterInvolved,
                    }
                },
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Must have instigator minion."
            };
            ActionOption pursuade = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Pursuade to stop " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.POSSESSIVE, false) + " patrol.",
                effect = () => PursuadeOptionEffect(state),
                jobNeeded = JOB.DEBILITATOR,
                doesNotMeetRequirementsStr = "Must have dissuader minion."
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(reveal);
            state.AddActionOption(pursuade);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override void SetTargetCharacter(Character targetCharacter, Character actor) {
        _targetCharacter = targetCharacter;
        if (_targetCharacter != null) {
            AddLogFillerToAllStates(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        }
    }
    #endregion

    #region Option Effects
    private void RevealOptionEffect(InteractionState state) {
        int patrollerChance = 0;
        int tokenChance = 0;
        SetTargetCharacter(state.assignedCharacter.character, _characterInvolved);
        CombatManager.Instance.GetCombatWeightsOfTwoLists(_characterInvolved.currentParty.characters,
            _targetCharacter.currentParty.characters, out patrollerChance, out tokenChance);
        WeightedDictionary<string> combatResults = new WeightedDictionary<string>();
        combatResults.AddElement("Patroller Won", patrollerChance);
        combatResults.AddElement("Patroller Lost", tokenChance);

        WeightedDictionary<string> nextStateWeights = new WeightedDictionary<string>();
        switch (combatResults.PickRandomElementGivenWeights()) {
            case "Patroller Won":
                nextStateWeights.AddElement(Revealed_Patroller_Killed_Character, 20);
                nextStateWeights.AddElement(Revealed_Patroller_Injured_Character, 40);
                break;
            case "Patroller Lost":
                nextStateWeights.AddElement(Revealed_Character_Killed_Patroller, 20);
                nextStateWeights.AddElement(Revealed_Character_Injured_Patroller, 40);
                break;
            default:
                break;
        }
        //investigatorCharacter.LevelUp();
        SetCurrentState(_states[nextStateWeights.PickRandomElementGivenWeights()]);
    }
    private void PursuadeOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> dissuaderSuccessRate = investigatorCharacter.job.GetJobRateWeights();
        dissuaderSuccessRate.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        if (dissuaderSuccessRate.PickRandomElementGivenWeights() == RESULT.SUCCESS) {
            nextState = Persuaded_Patrol_Stopped;
        } else {
            WeightedDictionary<RESULT> patrollerSuccessRate = _characterInvolved.job.GetJobRateWeights();
            patrollerSuccessRate.RemoveElement(RESULT.CRITICAL_FAIL);
            if (patrollerSuccessRate.PickRandomElementGivenWeights() == RESULT.SUCCESS) {
                //**Mechanics**: Combat Patroller vs Selected Character
                SetTargetCharacter(GetTargetCharacter(), _characterInvolved);
                if (_targetCharacter != null) {
                    int patrollerChance = 0;
                    int targetCharacterChance = 0;
                    CombatManager.Instance.GetCombatWeightsOfTwoLists(_characterInvolved.currentParty.characters,
                        _targetCharacter.currentParty.characters, out patrollerChance, out targetCharacterChance);
                    WeightedDictionary<string> combatResults = new WeightedDictionary<string>();
                    combatResults.AddElement("Patroller Won", patrollerChance);
                    combatResults.AddElement("Patroller Lost", targetCharacterChance);

                    WeightedDictionary<string> nextStateWeights = new WeightedDictionary<string>();
                    switch (combatResults.PickRandomElementGivenWeights()) {
                        case "Patroller Won":
                            nextStateWeights.AddElement(Persuaded_Patroller_Killed_Character, 20);
                            nextStateWeights.AddElement(Persuaded_Patroller_Injured_Character, 40);
                            break;
                        case "Patroller Lost":
                            nextStateWeights.AddElement(Persuaded_Character_Killed_Patroller, 20);
                            nextStateWeights.AddElement(Persuaded_Character_Injured_Patroller, 40);
                            break;
                        default:
                            break;
                    }
                    nextState = nextStateWeights.PickRandomElementGivenWeights();
                } else {
                    nextState = Persuaded_Patrol_Failed;
                }
            } else {
                nextState = Persuaded_Patrol_Failed;
            }
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> patrollerSuccessRate = _characterInvolved.job.GetJobRateWeights();
        patrollerSuccessRate.RemoveElement(RESULT.CRITICAL_FAIL);
        string nextState = string.Empty;
        if (patrollerSuccessRate.PickRandomElementGivenWeights() == RESULT.SUCCESS) {
            //**Mechanics**: Combat Patroller vs Selected Character
            SetTargetCharacter(GetTargetCharacter(), _characterInvolved);
            if (_targetCharacter != null) {
                int patrollerChance = 0;
                int targetCharacterChance = 0;
                CombatManager.Instance.GetCombatWeightsOfTwoLists(_characterInvolved.currentParty.characters,
                    _targetCharacter.currentParty.characters, out patrollerChance, out targetCharacterChance);
                WeightedDictionary<string> combatResults = new WeightedDictionary<string>();
                combatResults.AddElement("Patroller Won", patrollerChance);
                combatResults.AddElement("Patroller Lost", targetCharacterChance);
                if (patrollerChance <= 0 && targetCharacterChance <= 0) {
                    throw new System.Exception("Both patroller and target has no chance to win combat! Patroller is " + _characterInvolved.name + ". Target is " + _targetCharacter.name + ".");
                }
                WeightedDictionary<string> nextStateWeights = new WeightedDictionary<string>();
                switch (combatResults.PickRandomElementGivenWeights()) {
                    case "Patroller Won":
                        nextStateWeights.AddElement(Normal_Patroller_Killed_Character, 20);
                        nextStateWeights.AddElement(Normal_Patroller_Injured_Character, 40);
                        break;
                    case "Patroller Lost":
                        nextStateWeights.AddElement(Normal_Character_Killed_Patroller, 20);
                        nextStateWeights.AddElement(Normal_Character_Injured_Patroller, 40);
                        break;
                    default:
                        break;
                }
                nextState = nextStateWeights.PickRandomElementGivenWeights();
            } else {
                nextState = Normal_Patrol_Failed;
            }
        } else {
            nextState = Normal_Patrol_Failed;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effects
    private void RevealedPatrollerKilledCharacter(InteractionState state) {
        //**Mechanics**: Character is killed. Reduce relationship between the two factions.
        AdjustFactionsRelationship(_characterInvolved.faction, _targetCharacter.faction, -1, state);
        _targetCharacter.Death();
        //**Level Up**: Patroller +1
        //_characterInvolved.LevelUp();

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void RevealedPatrollerInjuredCharacter(InteractionState state) {
        //**Mechanics**: Character gains Injured.
        _targetCharacter.AddTrait("Injured");

        //**Level Up**: Patroller +1
        //_characterInvolved.LevelUp();

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void RevealedCharacterKilledPatroller(InteractionState state) {
        //**Mechanics**: Patroller is killed. Reduce relationship between the two factions.
        AdjustFactionsRelationship(_characterInvolved.faction, _targetCharacter.faction, -1, state);
        _characterInvolved.Death();

        //**Level Up**: Character +1
        //_targetCharacter.LevelUp();

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void RevealedCharacterInjuredPatroller(InteractionState state) {
        //**Mechanics**: Patroller gains Injured.
        _characterInvolved.AddTrait("Injured");

        //**Level Up**: Character +1
        //_targetCharacter.LevelUp();

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void PursuadedPatrolStopped(InteractionState state) {
        
    }
    private void PursuadedPatrollerKilledCharacter(InteractionState state) {
        //**Mechanics**: Character is killed. Reduce relationship between the two factions.
        AdjustFactionsRelationship(_characterInvolved.faction, _targetCharacter.faction, -1, state);
        _targetCharacter.Death();

        //**Level Up**: Character +1
        //_characterInvolved.LevelUp();

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void PursuadedPatrollerInjuredCharacter(InteractionState state) {
        //**Mechanics**: Character gains Injured.
        _targetCharacter.AddTrait("Injured");

        //**Level Up**: Patroller +1
        //_characterInvolved.LevelUp();

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void PursuadedCharacterKilledPatroller(InteractionState state) {
        //**Mechanics**: Patroller is killed. Reduce relationship between the two factions.
        AdjustFactionsRelationship(_characterInvolved.faction, _targetCharacter.faction, -1, state);
        _characterInvolved.Death();

        //**Level Up**: Character +1
        //_targetCharacter.LevelUp();
    }
    private void PursuadedCharacterInjuredPatroller(InteractionState state) {
        //**Mechanics**: Patroller gains Injured.
        _characterInvolved.AddTrait("Injured");

        //**Level Up**: Character +1
        //_targetCharacter.LevelUp();

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void PursuadedPatrolFailed(InteractionState state) {

    }
    private void NormalPatrollerKilledCharacter(InteractionState state) {
        //**Mechanics**: Character is killed. Reduce relationship between the two factions.
        AdjustFactionsRelationship(_characterInvolved.faction, _targetCharacter.faction, -1, state);
        _targetCharacter.Death();

        //**Level Up**: Patroller +1
        //_characterInvolved.LevelUp();

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalPatrollerInjuredCharacter(InteractionState state) {
        //**Mechanics**: Character gains Injured.
        _targetCharacter.AddTrait("Injured");

        //**Level Up**: Patroller +1
        //_characterInvolved.LevelUp();

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalCharacterKilledPatroller(InteractionState state) {
        //**Mechanics**: Patroller is killed. Reduce relationship between the two factions.
        AdjustFactionsRelationship(_characterInvolved.faction, _targetCharacter.faction, -1, state);
        _characterInvolved.Death();

        //**Level Up**: Character +1
        //_targetCharacter.LevelUp();

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalCharacterInjuredPatroller(InteractionState state) {
        //**Mechanics**: Patroller gains Injured.
        _characterInvolved.AddTrait("Injured");

        //**Level Up**: Character +1
        //_targetCharacter.LevelUp();

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalPatrolFailed(InteractionState state) {

    }
    #endregion

    #region Utilities
    //private void AdjustFactionsRelationship(Faction faction1, Faction faction2, int adjustment, InteractionState state) {
    //    faction1.AdjustRelationshipFor(faction2, adjustment);
    //    state.AddLogFiller(new LogFiller(faction1, faction1.name, LOG_IDENTIFIER.FACTION_1));
    //    state.AddLogFiller(new LogFiller(faction2, faction2.name, LOG_IDENTIFIER.FACTION_2));
    //    state.AddLogFiller(new LogFiller(null,
    //        Utilities.NormalizeString(faction1.GetRelationshipWith(faction2).relationshipStatus.ToString()), LOG_IDENTIFIER.STRING_1));
    //}
    private Character GetTargetCharacter() {
        List<Character> choices = new List<Character>();
        for (int i = 0; i < interactable.charactersAtLocation.Count; i++) {
            Character character = interactable.charactersAtLocation[i];
            if (character.id == _characterInvolved.id || character.currentParty.icon.isTravelling) {
                continue; //skip
            }
            if (character.isFactionless) {
                choices.Add(character);
            } else if (_characterInvolved.faction.id != character.faction.id) {
                FactionRelationship rel = _characterInvolved.faction.GetRelationshipWith(character.faction);
                if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED ||
                    rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY) {
                    choices.Add(character);
                }
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
        //throw new System.Exception("Could not find target character for Patrol Action at " + interactable.name);
    }
    #endregion

}
