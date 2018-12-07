using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseMobilization : Interaction {
    private const string Start = "Start";
    private const string Stop_Mobilization_Successful = "Stop Mobilization Successful";
    private const string Stop_Mobilization_Fail = "Stop Mobilization Fail";
    private const string Defender_Group_Created = "Defender Group Created";

    public DefenseMobilization(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.DEFENSE_MOBILIZATION, 0) {
        _name = "Defense Mobilization";
        _jobFilter = new JOB[] { JOB.DISSUADER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState stopMobilizationSuccessState = new InteractionState(Stop_Mobilization_Successful, this);
        InteractionState stopMobilizationFailState = new InteractionState(Stop_Mobilization_Fail, this);
        InteractionState defenderGroupCreatedState = new InteractionState(Defender_Group_Created, this);

        CreateActionOptions(startState);

        stopMobilizationSuccessState.SetEffect(() => StopMobilizationSuccessEffect(stopMobilizationSuccessState));
        stopMobilizationFailState.SetEffect(() => StopMobilizationFailEffect(stopMobilizationFailState));
        defenderGroupCreatedState.SetEffect(() => DefenderGroupCreatedEffect(defenderGroupCreatedState));

        _states.Add(startState.name, startState);
        _states.Add(stopMobilizationSuccessState.name, stopMobilizationSuccessState);
        _states.Add(stopMobilizationFailState.name, stopMobilizationFailState);
        _states.Add(defenderGroupCreatedState.name, defenderGroupCreatedState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Stop them.",
                duration = 0,
                jobNeeded = JOB.DISSUADER,
                effect = () => StopOption(),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(stopOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private void StopOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Stop_Mobilization_Successful, explorerMinion.character.job.GetSuccessRate());
        effectWeights.AddElement(Stop_Mobilization_Fail, explorerMinion.character.job.GetFailRate());

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement(Defender_Group_Upgraded, 25);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Defender_Group_Created]);
    }
    #endregion

    #region State Effects
    private void StopMobilizationSuccessEffect(InteractionState state) {
        explorerMinion.LevelUp();
    }
    private void StopMobilizationFailEffect(InteractionState state) {
        DefenderGroup newDefenderGroup = CreateNewDefenderGroupFromIdleCharactersInArea();
        for (int i = 0; i < newDefenderGroup.party.characters.Count; i++) {
            state.descriptionLog.AddToFillers(newDefenderGroup.party.characters[i], newDefenderGroup.party.characters[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1);
            state.AddLogFiller(new LogFiller(newDefenderGroup.party.characters[i], newDefenderGroup.party.characters[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1));
        }
    }
    private void DefenderGroupCreatedEffect(InteractionState state) {
        DefenderGroup newDefenderGroup = CreateNewDefenderGroupFromIdleCharactersInArea();
        for (int i = 0; i < newDefenderGroup.party.characters.Count; i++) {
            state.descriptionLog.AddToFillers(newDefenderGroup.party.characters[i], newDefenderGroup.party.characters[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1);
            state.AddLogFiller(new LogFiller(newDefenderGroup.party.characters[i], newDefenderGroup.party.characters[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1));
        }
    }
    #endregion

    private DefenderGroup CreateNewDefenderGroupFromIdleCharactersInArea() {
        DefenderGroup newDefenders = new DefenderGroup();
        interactable.tileLocation.areaOfTile.AddDefenderGroup(newDefenders);
        for (int i = 0; i < interactable.tileLocation.areaOfTile.areaResidents.Count; i++) {
            Character resident = interactable.tileLocation.areaOfTile.areaResidents[i];
            if (!resident.currentParty.icon.isTravelling && resident.specificLocation.tileLocation.areaOfTile.id == interactable.tileLocation.areaOfTile.id) {
                newDefenders.AddCharacterToGroup(resident);
                if (newDefenders.party.characters.Count >= 4) {
                    break;
                }
            }
        }
        return newDefenders;
    }
}
