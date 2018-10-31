using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NothingHappened : Interaction {

    public NothingHappened(IInteractable interactable) : base(interactable, INTERACTION_TYPE.SUSPICIOUS_SOLDIER_MEETING, 70) {
        _name = "Nothing Happened";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState exploreContinuesState = new InteractionState("Explore Continues", this);
        InteractionState exploreEndsState = new InteractionState("Explore Ends", this);

        string startStateDesc = _interactable.explorerMinion.name + " did not find anything worth reporting.";
        startState.SetDescription(startStateDesc);
        CreateActionOptions(startState);

        exploreContinuesState.SetEndEffect(() => ExploreContinuesRewardEffect(exploreContinuesState));
        exploreEndsState.SetEndEffect(() => ExploreEndsRewardEffect(exploreEndsState));

        _states.Add(startState.name, startState);
        _states.Add(exploreContinuesState.name, exploreContinuesState);
        _states.Add(exploreEndsState.name, exploreEndsState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption keepLookingOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Keep looking.",
                duration = 0,
                needsMinion = false,
                effect = () => KeepLookingOption(state),
            };
            ActionOption okOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "OK.",
                duration = 0,
                needsMinion = false,
                effect = () => OkOption(state),
            };

            state.AddActionOption(keepLookingOption);
            state.AddActionOption(okOption);

            GameDate scheduleDate = GameManager.Instance.Today();
            scheduleDate.AddHours(70);
            state.SetDefaultOption(okOption);
        }
    }
    #endregion

    private void KeepLookingOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Explore Continues", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Explore Continues") {
            ExploreContinuesRewardState(state, chosenEffect);
        }
    }
    private void OkOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Explore Ends", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Explore Ends") {
            ExploreEndsRewardState(state, chosenEffect);
        }
    }
    #region States
    private void ExploreContinuesRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription("We've instructed " + _interactable.explorerMinion.name + " to continue its surveillance of the area.");
        SetCurrentState(_states[stateName]);
    }
    private void ExploreEndsRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription("We've instructed " + _interactable.explorerMinion.name + " to return.");
        SetCurrentState(_states[stateName]);
    }
    #endregion

    #region State Effects
    private void ExploreContinuesRewardEffect(InteractionState state) {
        if(_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            landmark.landmarkInvestigation.ExploreLandmark();
        }
    }
    private void ExploreEndsRewardEffect(InteractionState state) {
        if (_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            landmark.landmarkInvestigation.MinionGoBackFromAssignment(landmark.landmarkInvestigation.UnexploreLandmark);
        }
    }
    #endregion
}
