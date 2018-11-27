using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyMobilization : Interaction {

    public ArmyMobilization(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.ARMY_MOBILIZATION, 50) {
        _name = "Army Mobilization";
    }

    #region Overrides
    public override void CreateStates() {
        //CreateExploreStates();
        //CreateWhatToDoNextState(explorerMinion.name + " just watches while the army is being mobilized. Do you want him to continue surveillance of " + _interactable.specificLocation.thisName + "?");

        InteractionState startState = new InteractionState("Start", this);
        InteractionState cancelledMobilizationState = new InteractionState("Cancelled Mobilization", this);
        InteractionState failCancelledMobilizationState = new InteractionState("Failed Cancel Mobilization", this);
        InteractionState demonDisappearsState = new InteractionState("Demon Disappears", this);
        InteractionState armyMobilizedState = new InteractionState("Army Mobilized", this);

        //string startStateDesc = "The garrison is mobilizing its forces. They are planning to assign some reserved units to defensive positions in the city.";
        //startState.SetDescription(startStateDesc);

        CreateActionOptions(startState);
        //CreateActionOptions(cancelledMobilizationState);
        //CreateActionOptions(failCancelledMobilizationState);
        //CreateActionOptions(armyMobilizedState);

        cancelledMobilizationState.SetEndEffect(() => CancelledMobilizationRewardEffect(cancelledMobilizationState));
        failCancelledMobilizationState.SetEndEffect(() => FailedCancelMobilizationRewardEffect(failCancelledMobilizationState));
        demonDisappearsState.SetEndEffect(() => DemonDisappearsRewardEffect(demonDisappearsState));
        armyMobilizedState.SetEndEffect(() => ArmyMobilizedRewardEffect(armyMobilizedState));

        _states.Add(startState.name, startState);
        _states.Add(cancelledMobilizationState.name, cancelledMobilizationState);
        _states.Add(failCancelledMobilizationState.name, failCancelledMobilizationState);
        _states.Add(demonDisappearsState.name, demonDisappearsState);
        _states.Add(armyMobilizedState.name, armyMobilizedState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopThemOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Stop them.",
                duration = 0,
                //description = "We have sent %minion% to persuade the garrison general to stop army mobilization.",
                needsMinion = false,
                effect = () => StopThemOption(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                //description = "The garrison is mobilizing its forces. They are planning to assign some reserved units to defensive positions in the city.",
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
        effectWeights.AddElement("Cancelled Mobilization", 30);
        effectWeights.AddElement("Failed Cancel Mobilization", 10);
        effectWeights.AddElement("Demon Disappears", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
        //if (chosenEffect == "Cancelled Mobilization") {
        //    CancelledMobilizationRewardState(state, chosenEffect);
        //} else if (chosenEffect == "Failed Cancel Mobilization") {
        //    FailedCancelMobilizationRewardState(state, chosenEffect);
        //} else if (chosenEffect == "Demon Disappears") {
        //    DemonDisappearsRewardState(state, chosenEffect);
        //}
    }

    private void DoNothingOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Army Mobilized", 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);

        //if (chosenEffect == "Army Mobilized") {
        //    ArmyMobilizedRewardState(state, chosenEffect);
        //}
    }

    #region States
    private void CancelledMobilizationRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " disguised himself and talked to the Army General, eventually convincing him to delay their mobilization.");
        SetCurrentState(_states[stateName]);
        CancelledMobilizationRewardEffect(_states[stateName]);
    }
    private void FailedCancelMobilizationRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " disguised himself and talked to the Army General, but was unable to convince him to delay their mobilization.");
        SetCurrentState(_states[stateName]);
        FailedCancelMobilizationRewardEffect(_states[stateName]);
    }
    private void ArmyMobilizedRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription("The garrison has now mobilized its reserved forces.");
        SetCurrentState(_states[stateName]);
        ArmyMobilizedRewardEffect(_states[stateName]);
    }
    #endregion

    #region State Effects
    private void CancelledMobilizationRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
    private void FailedCancelMobilizationRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        ArmyMobilizedRewardEffect(state);
    }
    private void ArmyMobilizedRewardEffect(InteractionState state) {
        _interactable.StartMobilization();
    }
    #endregion
}
