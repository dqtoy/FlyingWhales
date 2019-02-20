using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnNeutralCharacter : Interaction {

    private string _classNameToBeSpawned;

    public SpawnNeutralCharacter(Area interactable) : base(interactable, INTERACTION_TYPE.SPAWN_NEUTRAL_CHARACTER, 70) {
        _name = "Spawn Neutral Character";
        _jobFilter = new JOB[] { JOB.DEBILITATOR, JOB.RECRUITER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState successCancellationState = new InteractionState("Success Cancellation", this);
        InteractionState failCancellationState = new InteractionState("Fail Cancellation", this);
        InteractionState successRecruitState = new InteractionState("Success Recruit", this);
        InteractionState failRecruitState = new InteractionState("Fail Recruit", this);
        InteractionState normalSpawnState = new InteractionState("Normal Spawn", this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        successCancellationState.SetEffect(() => SuccessCancelEffect(successCancellationState));
        failCancellationState.SetEffect(() => FailCancelEffect(failCancellationState));
        successRecruitState.SetEffect(() => SuccessRecruitEffect(successRecruitState));
        failRecruitState.SetEffect(() => FailRecruitEffect(failRecruitState));
        normalSpawnState.SetEffect(() => NormalSpawnEffect(normalSpawnState));

        _states.Add(startState.name, startState);
        _states.Add(successCancellationState.name, successCancellationState);
        _states.Add(failCancellationState.name, failCancellationState);
        _states.Add(successRecruitState.name, successRecruitState);
        _states.Add(failRecruitState.name, failRecruitState);
        _states.Add(normalSpawnState.name, normalSpawnState);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Stop it from happening.",
                duration = 0,
                jobNeeded = JOB.DEBILITATOR,
                effect = () => StopOption(),
                doesNotMeetRequirementsStr = "Minion must be Dissuader."
            };
            ActionOption recruitOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.MANA },
                name = "Beguile it to our side.",
                duration = 0,
                jobNeeded = JOB.RECRUITER,
                effect = () => RecruitOption(),
                doesNotMeetRequirementsStr = "Minion must be Recruiter."
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(stopOption);
            state.AddActionOption(recruitOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private void StopOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Success Cancellation", investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement("Fail Cancellation", investigatorCharacter.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void RecruitOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Success Recruit", investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement("Fail Recruit", investigatorCharacter.job.GetFailRate());
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
        WeightedDictionary<AreaCharacterClass> classWeights = interactable.GetClassWeights();
        _classNameToBeSpawned = classWeights.PickRandomElementGivenWeights().className;

        state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(interactable.raceType.ToString()), LOG_IDENTIFIER.STRING_1);
        state.descriptionLog.AddToFillers(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2);
    }
    private void SuccessCancelEffect(InteractionState state) {
        //investigatorCharacter.LevelUp();

        state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(interactable.raceType.ToString()), LOG_IDENTIFIER.STRING_1);
        state.descriptionLog.AddToFillers(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2);

        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(interactable.raceType.ToString()), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2));
    }
    private void FailCancelEffect(InteractionState state) {
        //Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(_classNameToBeSpawned, interactable.raceType, Utilities.GetRandomGender(), FactionManager.Instance.neutralFaction, interactable);
        //createdCharacter.SetLevel(createdCharacter.raceSetting.neutralSpawnLevel);

        //state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(interactable.raceType.ToString()), LOG_IDENTIFIER.STRING_1);
        //state.descriptionLog.AddToFillers(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2);

        //state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(interactable.raceType.ToString()), LOG_IDENTIFIER.STRING_1));
        //state.AddLogFiller(new LogFiller(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2));
        //state.AddLogFiller(new LogFiller(createdCharacter, createdCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void SuccessRecruitEffect(InteractionState state) {
        ////investigatorCharacter.LevelUp();

        //Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(_classNameToBeSpawned, interactable.raceType, Utilities.GetRandomGender(), FactionManager.Instance.neutralFaction, interactable);
        //createdCharacter.SetLevel(createdCharacter.raceSetting.neutralSpawnLevel);
        //createdCharacter.RecruitAsMinion();

        //state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(interactable.raceType.ToString()), LOG_IDENTIFIER.STRING_1);
        //state.descriptionLog.AddToFillers(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2);
        //state.descriptionLog.AddToFillers(createdCharacter, createdCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        //state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(interactable.raceType.ToString()), LOG_IDENTIFIER.STRING_1));
        //state.AddLogFiller(new LogFiller(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2));
        //state.AddLogFiller(new LogFiller(createdCharacter, createdCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void FailRecruitEffect(InteractionState state) {
        //Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(_classNameToBeSpawned, interactable.raceType, Utilities.GetRandomGender(), FactionManager.Instance.neutralFaction, interactable);
        //createdCharacter.SetLevel(createdCharacter.raceSetting.neutralSpawnLevel);

        //state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(interactable.raceType.ToString()), LOG_IDENTIFIER.STRING_1);
        //state.descriptionLog.AddToFillers(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2);

        //state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(interactable.raceType.ToString()), LOG_IDENTIFIER.STRING_1));
        //state.AddLogFiller(new LogFiller(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2));
        //state.AddLogFiller(new LogFiller(createdCharacter, createdCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalSpawnEffect(InteractionState state) {
        //Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(_classNameToBeSpawned, interactable.raceType, Utilities.GetRandomGender(), FactionManager.Instance.neutralFaction, interactable);
        //createdCharacter.SetLevel(createdCharacter.raceSetting.neutralSpawnLevel);

        //state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(interactable.raceType.ToString()), LOG_IDENTIFIER.STRING_1);
        //state.descriptionLog.AddToFillers(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2);
        //state.descriptionLog.AddToFillers(createdCharacter, createdCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        //state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(interactable.raceType.ToString()), LOG_IDENTIFIER.STRING_1));
        //state.AddLogFiller(new LogFiller(null, _classNameToBeSpawned, LOG_IDENTIFIER.STRING_2));
        //state.AddLogFiller(new LogFiller(createdCharacter, createdCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
