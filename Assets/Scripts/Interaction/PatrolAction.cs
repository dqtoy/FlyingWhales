using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolAction : Interaction {

    private const string Revealed_Patroller_Killed_Character = "Revealed Patroller Killed Character";
    private const string Revealed_Patroller_Injured_Character = "Revealed Patroller Injured Character";
    private const string Revealed_Character_Killed_Patroller = "Revealed Character Killed Patroller";
    private const string Revealed_Character_Injured_Patroller = "Revealed Character Injured Patroller";
    private const string Pursuaded_Patrol_Stopped = "Pursuaded Patrol Stopped";
    private const string Pursuaded_Patroller_Killed_Character = "Pursuaded Patroller Killed Character";
    private const string Pursuaded_Patroller_Injured_Character = "Pursuaded Patroller Injured Character";
    private const string Pursuaded_Character_Killed_Patroller = "Pursuaded Character Killed Patroller";
    private const string Pursuaded_Character_Injured_Patroller = "Pursuaded Character Injured Patroller";
    private const string Pursuaded_Patrol_Failed = "Pursuaded Patrol Failed";
    private const string Normal_Patroller_Killed_Character = "Normal Patroller Killed Character";
    private const string Normal_Patroller_Injured_Character = "Normal Patroller Injured Character";
    private const string Normal_Character_Killed_Patroller = "Normal Character Killed Patroller";
    private const string Normal_Character_Injured_Patroller = "Normal Character Injured Patroller";
    private const string Normal_Patrol_Failed = "Normal Patrol Failed";

    private Character targetCharacter;

    public PatrolAction(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.PATROL_ACTION, 0) {
        _name = "Patrol Action";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DISSUADER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState revealedPatrollerKilledCharacter = new InteractionState(Revealed_Patroller_Killed_Character, this);
        InteractionState revealedPatrollerInjuredCharacter = new InteractionState(Revealed_Patroller_Injured_Character, this);
        InteractionState revealedCharacterKilledPatroller = new InteractionState(Revealed_Character_Killed_Patroller, this);
        InteractionState revealedCharacterInjuredPatroller = new InteractionState(Revealed_Character_Injured_Patroller, this);
        InteractionState pursuadedPatrolStopped = new InteractionState(Pursuaded_Patrol_Stopped, this);
        InteractionState pursuadedPatrollerKilledCharacter = new InteractionState(Pursuaded_Patroller_Killed_Character, this);
        InteractionState pursuadedPatrollerInjuredCharacter = new InteractionState(Pursuaded_Patroller_Injured_Character, this);
        InteractionState pursuadedCharacterKilledPatroller = new InteractionState(Pursuaded_Character_Killed_Patroller, this);
        InteractionState pursuadedCharacterInjuredPatroller = new InteractionState(Pursuaded_Character_Injured_Patroller, this);
        InteractionState pursuadedPatrolFailed = new InteractionState(Pursuaded_Patrol_Failed, this);
        InteractionState normalPatrollerKilledCharacter = new InteractionState(Normal_Patroller_Killed_Character, this);
        InteractionState normalPatrollerInjuredCharacter = new InteractionState(Normal_Patroller_Injured_Character, this);
        InteractionState normalCharacterKilledPatroller = new InteractionState(Normal_Character_Killed_Patroller, this);
        InteractionState normalCharacterInjuredPatroller = new InteractionState(Normal_Character_Injured_Patroller, this);
        InteractionState normalPatrolFailed = new InteractionState(Normal_Patrol_Failed, this);

        //Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        //startStateDescriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        //startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        revealedPatrollerKilledCharacter.SetEffect(() => RevealedPatrollerKilledCharacter(revealedPatrollerKilledCharacter));
        revealedPatrollerInjuredCharacter.SetEffect(() => RevealedPatrollerInjuredCharacter(revealedPatrollerInjuredCharacter));
        revealedCharacterKilledPatroller.SetEffect(() => RevealedCharacterKilledPatroller(revealedPatrollerInjuredCharacter));
        revealedCharacterInjuredPatroller.SetEffect(() => RevealedCharacterInjuredPatroller(revealedCharacterInjuredPatroller));
        pursuadedPatrolStopped.SetEffect(() => PursuadedPatrolStopped(pursuadedPatrolStopped));
        pursuadedPatrollerKilledCharacter.SetEffect(() => PursuadedPatrollerKilledCharacter(pursuadedPatrolStopped));
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

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption reveal = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Reveal an enemy's whereabouts.",
                effect = () => RevealOptionEffect(state),
                neededObjects = new List<System.Type>() { typeof(CharacterToken) },
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Minion must be an instigator",
            };
            ActionOption pursuade = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Pursuade to stop " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.POSSESSIVE, false) + " patrol.",
                effect = () => PursuadeOptionEffect(state),
                jobNeeded = JOB.DISSUADER,
                doesNotMeetRequirementsStr = "Minion must be a dissuader",
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
    #endregion

    #region Option Effects
    private void RevealOptionEffect(InteractionState state) {
        int patrollerChance = 0;
        int tokenChance = 0;
        SetTargetCharacter(state.assignedCharacter.character);
        CombatManager.Instance.GetCombatWeightsOfTwoLists(_characterInvolved.currentParty.characters,
            targetCharacter.currentParty.characters, out patrollerChance, out tokenChance);
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
        investigatorMinion.LevelUp();
        SetCurrentState(_states[nextStateWeights.PickRandomElementGivenWeights()]);
    }
    private void PursuadeOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> dissuaderSuccessRate = investigatorMinion.character.job.GetJobRateWeights();
        dissuaderSuccessRate.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        if (dissuaderSuccessRate.PickRandomElementGivenWeights() == RESULT.SUCCESS) {
            nextState = Pursuaded_Patrol_Stopped;
        } else {
            WeightedDictionary<RESULT> patrollerSuccessRate = _characterInvolved.job.GetJobRateWeights();
            patrollerSuccessRate.RemoveElement(RESULT.CRITICAL_FAIL);
            if (patrollerSuccessRate.PickRandomElementGivenWeights() == RESULT.SUCCESS) {
                //**Mechanics**: Combat Patroller vs Selected Character
                SetTargetCharacter(GetTargetCharacter());
                if (targetCharacter != null) {
                    int patrollerChance = 0;
                    int targetCharacterChance = 0;
                    CombatManager.Instance.GetCombatWeightsOfTwoLists(_characterInvolved.currentParty.characters,
                        targetCharacter.currentParty.characters, out patrollerChance, out targetCharacterChance);
                    WeightedDictionary<string> combatResults = new WeightedDictionary<string>();
                    combatResults.AddElement("Patroller Won", patrollerChance);
                    combatResults.AddElement("Patroller Lost", targetCharacterChance);

                    WeightedDictionary<string> nextStateWeights = new WeightedDictionary<string>();
                    switch (combatResults.PickRandomElementGivenWeights()) {
                        case "Patroller Won":
                            nextStateWeights.AddElement(Pursuaded_Patroller_Killed_Character, 20);
                            nextStateWeights.AddElement(Pursuaded_Patroller_Injured_Character, 40);
                            break;
                        case "Patroller Lost":
                            nextStateWeights.AddElement(Pursuaded_Character_Killed_Patroller, 20);
                            nextStateWeights.AddElement(Pursuaded_Character_Injured_Patroller, 40);
                            break;
                        default:
                            break;
                    }
                    nextState = nextStateWeights.PickRandomElementGivenWeights();
                } else {
                    nextState = Pursuaded_Patrol_Failed;
                }
            } else {
                nextState = Pursuaded_Patrol_Failed;
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
            SetTargetCharacter(GetTargetCharacter());
            if (targetCharacter != null) {
                int patrollerChance = 0;
                int targetCharacterChance = 0;
                CombatManager.Instance.GetCombatWeightsOfTwoLists(_characterInvolved.currentParty.characters,
                    targetCharacter.currentParty.characters, out patrollerChance, out targetCharacterChance);
                WeightedDictionary<string> combatResults = new WeightedDictionary<string>();
                combatResults.AddElement("Patroller Won", patrollerChance);
                combatResults.AddElement("Patroller Lost", targetCharacterChance);

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
        AdjustFactionsRelationship(_characterInvolved.faction, targetCharacter.faction, -1, state);
        targetCharacter.Death();
        
        //**Level Up**: Patroller +1
        _characterInvolved.LevelUp();
    }
    private void RevealedPatrollerInjuredCharacter(InteractionState state) {
        //**Mechanics**: Character gains Injured.
        targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);

        //**Level Up**: Patroller +1
        _characterInvolved.LevelUp();
    }
    private void RevealedCharacterKilledPatroller(InteractionState state) {
        //**Mechanics**: Patroller is killed. Reduce relationship between the two factions.
        AdjustFactionsRelationship(_characterInvolved.faction, targetCharacter.faction, -1, state);
        _characterInvolved.Death();

        //**Level Up**: Character +1
        targetCharacter.LevelUp();
    }
    private void RevealedCharacterInjuredPatroller(InteractionState state) {
        //**Mechanics**: Patroller gains Injured.
        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Injured"]);

        //**Level Up**: Character +1
        targetCharacter.LevelUp();
    }
    private void PursuadedPatrolStopped(InteractionState state) {
        
    }
    private void PursuadedPatrollerKilledCharacter(InteractionState state) {
        //**Mechanics**: Character is killed. Reduce relationship between the two factions.
        AdjustFactionsRelationship(_characterInvolved.faction, targetCharacter.faction, -1, state);
        targetCharacter.Death();

        //**Level Up**: Character +1
        _characterInvolved.LevelUp();
    }
    private void PursuadedPatrollerInjuredCharacter(InteractionState state) {
        //**Mechanics**: Character gains Injured.
        targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);

        //**Level Up**: Patroller +1
        _characterInvolved.LevelUp();
    }
    private void PursuadedCharacterKilledPatroller(InteractionState state) {
        //**Mechanics**: Patroller is killed. Reduce relationship between the two factions.
        AdjustFactionsRelationship(_characterInvolved.faction, targetCharacter.faction, -1, state);
        _characterInvolved.Death();

        //**Level Up**: Character +1
        targetCharacter.LevelUp();
    }
    private void PursuadedCharacterInjuredPatroller(InteractionState state) {
        //**Mechanics**: Patroller gains Injured.
        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Injured"]);

        //**Level Up**: Character +1
        targetCharacter.LevelUp();
    }
    private void PursuadedPatrolFailed(InteractionState state) {

    }
    private void NormalPatrollerKilledCharacter(InteractionState state) {
        //**Mechanics**: Character is killed. Reduce relationship between the two factions.
        AdjustFactionsRelationship(_characterInvolved.faction, targetCharacter.faction, -1, state);
        targetCharacter.Death();

        //**Level Up**: Patroller +1
        _characterInvolved.LevelUp();
    }
    private void NormalPatrollerInjuredCharacter(InteractionState state) {
        //**Mechanics**: Character gains Injured.
        targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);

        //**Level Up**: Patroller +1
        _characterInvolved.LevelUp();
    }
    private void NormalCharacterKilledPatroller(InteractionState state) {
        //**Mechanics**: Patroller is killed. Reduce relationship between the two factions.
        AdjustFactionsRelationship(_characterInvolved.faction, targetCharacter.faction, -1, state);
        _characterInvolved.Death();

        //**Level Up**: Character +1
        targetCharacter.LevelUp();
    }
    private void NormalCharacterInjuredPatroller(InteractionState state) {
        //**Mechanics**: Patroller gains Injured.
        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Injured"]);

        //**Level Up**: Character +1
        targetCharacter.LevelUp();
    }
    private void NormalPatrolFailed(InteractionState state) {

    }
    #endregion

    #region Utilities
    private void AdjustFactionsRelationship(Faction faction1, Faction faction2, int adjustment, InteractionState state) {
        faction1.AdjustRelationshipFor(faction2, adjustment);
        state.AddLogFiller(new LogFiller(faction1, faction1.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(faction2, faction2.name, LOG_IDENTIFIER.FACTION_2));
        state.AddLogFiller(new LogFiller(null,
            Utilities.NormalizeString(faction1.GetRelationshipWith(faction2).relationshipStatus.ToString()), LOG_IDENTIFIER.STRING_1));
    }
    private Character GetTargetCharacter() {
        List<Character> choices = new List<Character>();
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            Character character = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (!character.isFactionless && _characterInvolved.id != character.id && _characterInvolved.faction.id != character.faction.id) {
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
    }

    private void SetTargetCharacter(Character character) {
        targetCharacter = character;
        if (targetCharacter != null) {
            AddLogFillerToAllStates(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        }
    }
    #endregion

}
