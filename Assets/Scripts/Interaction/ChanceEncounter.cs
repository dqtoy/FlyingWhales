using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChanceEncounter : Interaction {

    private const string Start = "Start";
    private const string Instigated_Encounter_Positive = "Instigated Encounter Positive";
    private const string Instigated_Encounter_Neutral = "Instigated Encounter Neutral";
    private const string Instigated_Encounter_Negative = "Instigated Encounter Negative";
    private const string Assisted_Encounter_Positive = "Assisted Encounter Positive";
    private const string Assisted_Encounter_Neutral = "Assisted Encounter Neutral";
    private const string Assisted_Encounter_Negative = "Assisted Encounter Negative";
    private const string Normal_Encounter_Positive = "Normal Encounter Positive";
    private const string Normal_Encounter_Neutral = "Normal Encounter Neutral";
    private const string Normal_Encounter_Negative = "Normal Encounter Negative";

    private const string Positive = "Positive";
    private const string Neutral = "Neutral";
    private const string Negative = "Negative";

    private Character targetCharacter;
    private WeightedDictionary<string> encounterWeights = new WeightedDictionary<string>();

    public ChanceEncounter(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.CHANCE_ENCOUNTER, 0) {
        _name = "Chance Encounter";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        if (targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        encounterWeights.AddElement(Positive, 100);
        encounterWeights.AddElement(Neutral, 100);
        encounterWeights.AddElement(Negative, 100);

        if(!_characterInvolved.isFactionless && !targetCharacter.isFactionless) {
            if (_characterInvolved.faction == targetCharacter.faction) {
                encounterWeights.AddWeightToElement(Positive, 100);
                encounterWeights.AddWeightToElement(Neutral, 50);
            } else {
                FactionRelationship relationship = _characterInvolved.faction.GetRelationshipWith(targetCharacter.faction);
                if(relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ALLY || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                    encounterWeights.AddWeightToElement(Positive, 50);
                    encounterWeights.AddWeightToElement(Neutral, 100);
                } else if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL) {
                    encounterWeights.AddWeightToElement(Neutral, 50);
                } else if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY) {
                    encounterWeights.AddWeightToElement(Negative, 100);
                }
            }
            if(_characterInvolved.race == targetCharacter.race) {
                encounterWeights.AddWeightToElement(Positive, 100);
                encounterWeights.AddWeightToElement(Neutral, 50);
            } else {
                if(Utilities.AreTwoCharactersFromOpposingRaces(_characterInvolved, targetCharacter)) {
                    encounterWeights.AddWeightToElement(Negative, 100);
                }
            }
            if (_characterInvolved.characterClass.rangeType == targetCharacter.characterClass.rangeType &&_characterInvolved.characterClass.attackType == targetCharacter.characterClass.attackType) {
                encounterWeights.AddWeightToElement(Positive, 100);
                encounterWeights.AddWeightToElement(Neutral, 50);
            }
        }
        

        InteractionState startState = new InteractionState(Start, this);
        InteractionState instigatedEncounterPositive = new InteractionState(Instigated_Encounter_Positive, this);
        InteractionState instigatedEncounterNeutral = new InteractionState(Instigated_Encounter_Neutral, this);
        InteractionState instigatedEncounterNegative = new InteractionState(Instigated_Encounter_Negative, this);
        InteractionState assistedEncounterPositive = new InteractionState(Assisted_Encounter_Positive, this);
        InteractionState assistedEncounterNeutral = new InteractionState(Assisted_Encounter_Neutral, this);
        InteractionState assistedEncounterNegative = new InteractionState(Assisted_Encounter_Negative, this);
        InteractionState normalEncounterPositive = new InteractionState(Normal_Encounter_Positive, this);
        InteractionState normalEncounterNeutral = new InteractionState(Normal_Encounter_Neutral, this);
        InteractionState normalEncounterNegative = new InteractionState(Normal_Encounter_Negative, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        instigatedEncounterPositive.SetEffect(() => InstigatedEncounterPositiveEffect(instigatedEncounterPositive));
        instigatedEncounterNeutral.SetEffect(() => InstigatedEncounterNeutralEffect(instigatedEncounterNeutral));
        instigatedEncounterNegative.SetEffect(() => InstigatedEncounterNegativeEffect(instigatedEncounterNegative));
        assistedEncounterPositive.SetEffect(() => AssistedEncounterPositiveEffect(assistedEncounterPositive));
        assistedEncounterNeutral.SetEffect(() => AssistedEncounterNeutralEffect(assistedEncounterNeutral));
        assistedEncounterNegative.SetEffect(() => AssistedEncounterNegativeEffect(assistedEncounterNegative));
        normalEncounterPositive.SetEffect(() => NormalEncounterPositiveEffect(normalEncounterPositive));
        normalEncounterNeutral.SetEffect(() => NormalEncounterNeutralEffect(normalEncounterNeutral));
        normalEncounterNegative.SetEffect(() => NormalEncounterNegativeEffect(normalEncounterNegative));

        _states.Add(startState.name, startState);
        _states.Add(instigatedEncounterPositive.name, instigatedEncounterPositive);
        _states.Add(instigatedEncounterNeutral.name, instigatedEncounterNeutral);
        _states.Add(instigatedEncounterNegative.name, instigatedEncounterNegative);
        _states.Add(assistedEncounterPositive.name, assistedEncounterPositive);
        _states.Add(assistedEncounterNeutral.name, assistedEncounterNeutral);
        _states.Add(assistedEncounterNegative.name, assistedEncounterNegative);
        _states.Add(normalEncounterPositive.name, normalEncounterPositive);
        _states.Add(normalEncounterNeutral.name, normalEncounterNeutral);
        _states.Add(normalEncounterNegative.name, normalEncounterNegative);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption enemy = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Turn them against each other.",
                duration = 0,
                effect = () => EnemyOption(),
                jobNeeded = JOB.INSTIGATOR,
                disabledTooltipText = "Minion must be an Instigator",
            };
            ActionOption friend = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Let them become friends.",
                duration = 0,
                effect = () => FriendOption(),
                jobNeeded = JOB.DIPLOMAT,
                disabledTooltipText = "Minion must be a Diplomat",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(),
            };
            state.AddActionOption(enemy);
            state.AddActionOption(friend);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        SetTargetCharacter(GetTargetCharacter(character));
        if (targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Action Options
    private void EnemyOption() {
        encounterWeights.AddWeightToElement(Negative, 150);
        string result = encounterWeights.PickRandomElementGivenWeights();
        string nextState = string.Empty;
        if(result == Positive) {
            nextState = Instigated_Encounter_Positive;
        } else if (result == Neutral) {
            nextState = Instigated_Encounter_Neutral;
        } else if (result == Negative) {
            nextState = Instigated_Encounter_Negative;
        }
        SetCurrentState(_states[nextState]);
    }
    private void FriendOption() {
        encounterWeights.AddWeightToElement(Positive, 150);
        string result = encounterWeights.PickRandomElementGivenWeights();
        string nextState = string.Empty;
        if (result == Positive) {
            nextState = Assisted_Encounter_Positive;
        } else if (result == Neutral) {
            nextState = Assisted_Encounter_Neutral;
        } else if (result == Negative) {
            nextState = Assisted_Encounter_Negative;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOption() {
        string result = encounterWeights.PickRandomElementGivenWeights();
        string nextState = string.Empty;
        if (result == Positive) {
            nextState = Normal_Encounter_Positive;
        } else if (result == Neutral) {
            nextState = Normal_Encounter_Neutral;
        } else if (result == Negative) {
            nextState = Normal_Encounter_Negative;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region State Effects
    private void InstigatedEncounterPositiveEffect(InteractionState state) {
        CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(_characterInvolved, targetCharacter, 1);

        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void InstigatedEncounterNeutralEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void InstigatedEncounterNegativeEffect(InteractionState state) {
        investigatorMinion.LevelUp();
        CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(_characterInvolved, targetCharacter, -1);

        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void AssistedEncounterPositiveEffect(InteractionState state) {
        investigatorMinion.LevelUp();
        CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(_characterInvolved, targetCharacter, 1);

        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void AssistedEncounterNeutralEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void AssistedEncounterNegativeEffect(InteractionState state) {
        CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(_characterInvolved, targetCharacter, -1);

        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalEncounterPositiveEffect(InteractionState state) {
        CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(_characterInvolved, targetCharacter, 1);

        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalEncounterNeutralEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalEncounterNegativeEffect(InteractionState state) {
        CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(_characterInvolved, targetCharacter, -1);

        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion

    public void SetTargetCharacter(Character targetCharacter) {
        this.targetCharacter = targetCharacter;
    }
    public Character GetTargetCharacter(Character characterInvolved) {
        List<Character> characterList = new List<Character>();
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (currCharacter.forcedInteraction == null && currCharacter.id != characterInvolved.id
                && currCharacter.race != RACE.BEAST && currCharacter.race != RACE.SKELETON) {
                characterList.Add(currCharacter);
            }
        }
        if(characterList.Count > 0) {
            return characterList[UnityEngine.Random.Range(0, characterList.Count)];
        }
        return null;
        //throw new System.Exception("Could not find any character to recruit!");
    }
}
