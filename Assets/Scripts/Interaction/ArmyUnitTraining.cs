using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ArmyUnitTraining : Interaction {

    private string _chosenClassName;

    public ArmyUnitTraining(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.ARMY_UNIT_TRAINING, 50) {
        _name = "Army Unit Training";
    }

    #region Overrides
    public override void CreateStates() {
        //CreateExploreStates();

        InteractionState startState = new InteractionState("Start", this);
        InteractionState cancelledTrainingState = new InteractionState("Cancelled Training", this);
        InteractionState failCancelledTrainingState = new InteractionState("Failed Cancel Training", this);
        InteractionState demonDisappearsState = new InteractionState("Demon Disappears", this);
        InteractionState armyProducedState = new InteractionState("Army Produced", this);

        //string startStateDesc = "The garrison is producing another army unit.";
        //startState.SetDescription(startStateDesc);

        CreateActionOptions(startState);
        //CreateActionOptions(cancelledTrainingState);
        //CreateActionOptions(failCancelledTrainingState);
        //CreateActionOptions(armyProducedState);

        cancelledTrainingState.SetEffect(() => CancelledTrainingRewardEffect(cancelledTrainingState));
        failCancelledTrainingState.SetEffect(() => FailedCancelTrainingRewardEffect(failCancelledTrainingState));
        demonDisappearsState.SetEffect(() => DemonDisappearsRewardEffect(demonDisappearsState));
        armyProducedState.SetEffect(() => ArmyProducedRewardEffect(armyProducedState));

        _states.Add(startState.name, startState);
        _states.Add(cancelledTrainingState.name, cancelledTrainingState);
        _states.Add(failCancelledTrainingState.name, failCancelledTrainingState);
        _states.Add(demonDisappearsState.name, demonDisappearsState);
        _states.Add(armyProducedState.name, armyProducedState);

        SetCurrentState(startState);
        SetArmyClassNameToBeCreated();
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopThemOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Stop them.",
                duration = 0,
                //description = "We have sent %minion% to persuade the garrison general to stop their current plan of raising a new army unit.",
                needsMinion = false,
                effect = () => StopThemOption(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                //description = "The garrison is producing another army unit.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingOption(state),
                //onStartDurationAction = () => SetDefaultActionDurationAsRemainingTicks("Do nothing.", state)
            };

            state.AddActionOption(stopThemOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
            //GameDate scheduleDate = GameManager.Instance.Today();
            //scheduleDate.AddHours(50);
            //state.SetTimeSchedule(doNothingOption, scheduleDate);
        } else {
            ActionOption continueSurveillanceOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Continue surveillance of the area.",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreContinuesOption(state),
            };
            ActionOption returnToMeOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Return to me.",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreEndsOption(state),
            };

            state.AddActionOption(continueSurveillanceOption);
            state.AddActionOption(returnToMeOption);
            state.SetDefaultOption(returnToMeOption);
        }
    }
    #endregion

    private void StopThemOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Cancelled Training", 30);
        effectWeights.AddElement("Failed Cancel Training", 10);
        effectWeights.AddElement("Demon Disappears", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
        //if (chosenEffect == "Cancelled Training") {
        //    CancelledTrainingRewardState(state, chosenEffect);
        //} else if (chosenEffect == "Failed Cancel Training") {
        //    FailedCancelTrainingRewardState(state, chosenEffect);
        //} else if (chosenEffect == "Demon Disappears") {
        //    DemonDisappearsRewardState(state, chosenEffect);
        //}
    }

    private void DoNothingOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Army Produced", 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);

        //if (chosenEffect == "Army Produced") {
        //    ArmyProducedRewardState(state, chosenEffect);
        //}
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
        //_states[stateName].SetDescription(explorerMinion.name + " distracted the soldiers with liquor so they end up forgetting that they were supposed to form a new defensive army unit.");
        SetCurrentState(_states[stateName]);
        //CancelledTrainingRewardEffect(_states[stateName]);
    }
    private void FailedCancelTrainingRewardState(InteractionState state, string stateName) {
        SetArmyClassNameToBeCreated();
        //_states[stateName].SetDescription(explorerMinion.name + " failed to distract the soldiers. A new " + Utilities.NormalizeString(interactable.faction.race.ToString()) + " " + _chosenClassName + " unit has been formed at the garrison.");
        SetCurrentState(_states[stateName]);
        //FailedCancelTrainingRewardEffect(_states[stateName]);
    }
    private void ArmyProducedRewardState(InteractionState state, string stateName) {
        SetArmyClassNameToBeCreated();
        //_states[stateName].SetDescription("A new " + Utilities.NormalizeString(interactable.faction.race.ToString()) + " " + _chosenClassName + " unit has been formed at the garrison.");
        SetCurrentState(_states[stateName]);
        //ArmyProducedRewardEffect(_states[stateName]);
    }
    #endregion

    #region State Effects
    private void CancelledTrainingRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        state.AddLogFiller(new LogFiller(_interactable.faction, _interactable.faction.name, LOG_IDENTIFIER.FACTION_1));
        //if (state.minionLog != null) {
        //    state.minionLog.AddToFillers(_interactable.faction, _interactable.faction.name, LOG_IDENTIFIER.FACTION_1);
        //}
    }
    private void FailedCancelTrainingRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        ArmyProducedRewardEffect(state);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(_interactable.faction.race), LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(null, _chosenClassName, LOG_IDENTIFIER.STRING_2);
        }
        state.AddLogFiller(new LogFiller(_interactable.faction, _interactable.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.GetNormalizedSingularRace(_interactable.faction.race), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, _chosenClassName, LOG_IDENTIFIER.STRING_2));
        //if (state.minionLog != null) {
        //    state.minionLog.AddToFillers(_interactable.faction, _interactable.faction.name, LOG_IDENTIFIER.FACTION_1);
        //    state.minionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(_interactable.faction.race), LOG_IDENTIFIER.STRING_1);
        //    state.minionLog.AddToFillers(null, _chosenClassName, LOG_IDENTIFIER.STRING_2);
        //}
    }
    private void ArmyProducedRewardEffect(InteractionState state) {
        CharacterManager.Instance.CreateNewCharacter(_chosenClassName, interactable.faction.race, GENDER.MALE, interactable.faction, interactable);
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(_interactable.faction.race), LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(null, _chosenClassName, LOG_IDENTIFIER.STRING_2);
        }
        state.AddLogFiller(new LogFiller(_interactable.faction, _interactable.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.GetNormalizedSingularRace(_interactable.faction.race), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, _chosenClassName, LOG_IDENTIFIER.STRING_2));
        //if (state.minionLog != null) {
        //    state.minionLog.AddToFillers(_interactable.faction, _interactable.faction.name, LOG_IDENTIFIER.FACTION_1);
        //    state.minionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(_interactable.faction.race), LOG_IDENTIFIER.STRING_1);
        //    state.minionLog.AddToFillers(null, _chosenClassName, LOG_IDENTIFIER.STRING_2);
        //}

    }
    #endregion
}
