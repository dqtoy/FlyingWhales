using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ArmyUnitTraining : Interaction {

    private string _chosenClassName;

    public ArmyUnitTraining(IInteractable interactable) : base(interactable, INTERACTION_TYPE.MYSTERY_HUM) {
        _name = "Army Unit Training";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState cancelledTrainingState = new InteractionState("Cancelled Training", this);
        InteractionState failCancelledTrainingState = new InteractionState("Failed Cancel Training", this);
        InteractionState demonDisappearsState = new InteractionState("Demon Disappears", this);
        InteractionState armyProducedState = new InteractionState("Army Produced", this);

        string startStateDesc = "The garrison is producing another army unit.";
        startState.SetDescription(startStateDesc);
        CreateActionOptions(startState);

        cancelledTrainingState.SetEndEffect(() => CancelledTrainingRewardEffect(cancelledTrainingState));
        failCancelledTrainingState.SetEndEffect(() => FailedCancelTrainingRewardEffect(failCancelledTrainingState));
        demonDisappearsState.SetEndEffect(() => DemonDisappearsRewardEffect(demonDisappearsState));
        armyProducedState.SetEndEffect(() => ArmyProducedRewardEffect(armyProducedState));

        _states.Add(startState.name, startState);
        _states.Add(cancelledTrainingState.name, cancelledTrainingState);
        _states.Add(failCancelledTrainingState.name, failCancelledTrainingState);
        _states.Add(demonDisappearsState.name, demonDisappearsState);
        _states.Add(armyProducedState.name, armyProducedState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopThemOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Stop them.",
                duration = 5,
                needsMinion = true,
                effect = () => StopThemOption(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                description = "The garrison is producing another army unit.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingOption(state),
                onStartDurationAction = () => SetDefaultActionDurationAsRemainingTicks("Do nothing.", state)
            };

            state.AddActionOption(stopThemOption);
            state.AddActionOption(doNothingOption);

            GameDate scheduleDate = GameManager.Instance.Today();
            scheduleDate.AddHours(50);
            state.SetTimeSchedule(doNothingOption, scheduleDate);
        }
    }
    #endregion

    private void StopThemOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Cancelled Training", 30);
        effectWeights.AddElement("Failed Cancel Training", 10);
        effectWeights.AddElement("Demon Disappears", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Cancelled Training") {
            CancelledTrainingRewardState(state, chosenEffect);
        } else if (chosenEffect == "Failed Cancel Training") {
            FailedCancelTrainingRewardState(state, chosenEffect);
        } else if (chosenEffect == "Demon Disappears") {
            DemonDisappearsRewardState(state, chosenEffect);
        }
    }

    private void DoNothingOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Army Produced", 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Army Produced") {
            ArmyProducedRewardState(state, chosenEffect);
        }
    }

    private void SetArmyClassNameToBeCreated() {
        WeightedDictionary<string> classesWeights = new WeightedDictionary<string>();
        classesWeights.AddElement("Knight", 50);
        classesWeights.AddElement("Fire Bard", 10);
        classesWeights.AddElement("Water Bard", 10);
        classesWeights.AddElement("Earth Bard", 10);
        classesWeights.AddElement("Wind Bard", 10);
        classesWeights.AddElement("Archer", 20);
        classesWeights.AddElement("Healer", 15);

        _chosenClassName = classesWeights.PickRandomElementGivenWeights();
    }

    #region States
    private void CancelledTrainingRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " distracted the soldiers with liquor so they end up forgetting that they were supposed to form a new defensive army unit.");
        SetCurrentState(_states[stateName]);
    }
    private void FailedCancelTrainingRewardState(InteractionState state, string stateName) {
        SetArmyClassNameToBeCreated();
        _states[stateName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " failed to distract the soldiers. A new " + Utilities.NormalizeString(interactable.faction.race.ToString()) + " " + _chosenClassName + " unit has been formed at the garrison.");
        SetCurrentState(_states[stateName]);
    }
    private void ArmyProducedRewardState(InteractionState state, string stateName) {
        SetArmyClassNameToBeCreated();
        _states[stateName].SetDescription("A new " + Utilities.NormalizeString(interactable.faction.race.ToString()) + " " + _chosenClassName + " unit has been formed at the garrison.");
        SetCurrentState(_states[stateName]);
    }
    #endregion

    #region State Effects
    private void CancelledTrainingRewardEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(1);
    }
    private void FailedCancelTrainingRewardEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(1);
        ArmyProducedRewardEffect(state);
    }
    private void ArmyProducedRewardEffect(InteractionState state) {
        CharacterManager.Instance.CreateCharacterArmyUnit(_chosenClassName, interactable.faction.race, interactable.faction, interactable as BaseLandmark);
    }
    #endregion
}
