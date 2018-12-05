using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnCharacter : Interaction {

    private string _classNameToBeSpawned;

    public SpawnCharacter(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.SPAWN_CHARACTER, 70) {
        _name = "Spawn Character";
        _jobFilter = new JOB[] { JOB.DISSUADER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState successCancellationState = new InteractionState("Success Cancellation", this);
        InteractionState failCancellationState = new InteractionState("Fail Cancellation", this);
        InteractionState successCurseState = new InteractionState("Success Curse", this);
        InteractionState failCurseState = new InteractionState("Fail Curse", this);
        InteractionState normalSpawnState = new InteractionState("Normal Spawn", this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        successCancellationState.SetEffect(() => SuccessCancelEffect(successCancellationState));
        failCancellationState.SetEffect(() => FailCancelEffect(failCancellationState));
        successCurseState.SetEffect(() => SuccessCurseEffect(successCurseState));
        failCurseState.SetEffect(() => FailCurseEffect(failCurseState));
        normalSpawnState.SetEffect(() => NormalSpawnEffect(normalSpawnState));

        _states.Add(startState.name, startState);
        _states.Add(successCancellationState.name, successCancellationState);
        _states.Add(failCancellationState.name, failCancellationState);
        _states.Add(successCurseState.name, successCurseState);
        _states.Add(failCurseState.name, failCurseState);
        _states.Add(normalSpawnState.name, normalSpawnState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Stop it from happening.",
                duration = 0,
                jobNeeded = JOB.DISSUADER,
                effect = () => StopOption(),
                doesNotMeetRequirementsStr = "Minion must be Dissuader."
            };
            ActionOption curseOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.MANA },
                name = "Curse the new character.",
                duration = 0,
                jobNeeded = JOB.DISSUADER,
                effect = () => CurseOption(),
                doesNotMeetRequirementsStr = "Minion must be Dissuader."
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(stopOption);
            state.AddActionOption(curseOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private void StopOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Success Cancellation", explorerMinion.character.job.GetSuccessRate());
        effectWeights.AddElement("Fail Cancellation", explorerMinion.character.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void CurseOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Success Curse", explorerMinion.character.job.GetSuccessRate());
        effectWeights.AddElement("Fail Curse", explorerMinion.character.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement("Normal Spawn", 25);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states["Normal Spawn"]);
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        WeightedDictionary<AreaCharacterClass> classWeights = interactable.tileLocation.areaOfTile.GetClassWeights();
        _classNameToBeSpawned = classWeights.PickRandomElementGivenWeights().className;

        state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(interactable.tileLocation.areaOfTile.raceType.ToString()), LOG_IDENTIFIER.STRING_1);
        state.descriptionLog.AddToFillers(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2);
    }
    private void SuccessCancelEffect(InteractionState state) {
        explorerMinion.LevelUp();
        MinionSuccess();

        state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(interactable.tileLocation.areaOfTile.raceType.ToString()), LOG_IDENTIFIER.STRING_1);
        state.descriptionLog.AddToFillers(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(interactable.tileLocation.areaOfTile.raceType.ToString()), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2));
    }
    private void FailCancelEffect(InteractionState state) {
        Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(_classNameToBeSpawned, interactable.tileLocation.areaOfTile.raceType, Utilities.GetRandomGender(), interactable.tileLocation.areaOfTile.owner, interactable);

        state.descriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(interactable.tileLocation.areaOfTile.raceType.ToString()), LOG_IDENTIFIER.STRING_1);
        state.descriptionLog.AddToFillers(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2);
        state.descriptionLog.AddToFillers(createdCharacter, createdCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(interactable.tileLocation.areaOfTile.raceType.ToString()), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2));
        state.AddLogFiller(new LogFiller(createdCharacter, createdCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void SuccessCurseEffect(InteractionState state) {
        explorerMinion.LevelUp();

        Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(_classNameToBeSpawned, interactable.tileLocation.areaOfTile.raceType, Utilities.GetRandomGender(), interactable.tileLocation.areaOfTile.owner, interactable);
        Trait curse = AttributeManager.Instance.allTraits["Placeholder Curse 1"];
        createdCharacter.AddTrait(curse);

        state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(interactable.tileLocation.areaOfTile.raceType.ToString()), LOG_IDENTIFIER.STRING_1);
        state.descriptionLog.AddToFillers(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2);
        state.descriptionLog.AddToFillers(createdCharacter, createdCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(null, curse.name, LOG_IDENTIFIER.OTHER);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(interactable.tileLocation.areaOfTile.raceType.ToString()), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2));
        state.AddLogFiller(new LogFiller(createdCharacter, createdCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(null, curse.name, LOG_IDENTIFIER.OTHER));
    }
    private void FailCurseEffect(InteractionState state) {
        Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(_classNameToBeSpawned, interactable.tileLocation.areaOfTile.raceType, Utilities.GetRandomGender(), interactable.tileLocation.areaOfTile.owner, interactable);

        state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(interactable.tileLocation.areaOfTile.raceType.ToString()), LOG_IDENTIFIER.STRING_1);
        state.descriptionLog.AddToFillers(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(interactable.tileLocation.areaOfTile.raceType.ToString()), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2));
        state.AddLogFiller(new LogFiller(createdCharacter, createdCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalSpawnEffect(InteractionState state) {
        Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(_classNameToBeSpawned, interactable.tileLocation.areaOfTile.raceType, Utilities.GetRandomGender(), interactable.tileLocation.areaOfTile.owner, interactable);

        state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(interactable.tileLocation.areaOfTile.raceType.ToString()), LOG_IDENTIFIER.STRING_1);
        state.descriptionLog.AddToFillers(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2);
        state.descriptionLog.AddToFillers(createdCharacter, createdCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(interactable.tileLocation.areaOfTile.raceType.ToString()), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2));
        state.AddLogFiller(new LogFiller(createdCharacter, createdCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
