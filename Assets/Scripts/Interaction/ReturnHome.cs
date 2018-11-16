using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnHome : Interaction {

    public ReturnHome(IInteractable interactable) : base(interactable, INTERACTION_TYPE.RETURN_HOME, 70) {
        _name = "Return Home";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState cancelledState = new InteractionState("Cancelled", this);
        InteractionState continuesState = new InteractionState("Continues", this);
        InteractionState doNothingState = new InteractionState("Do Nothing", this);

        CreateActionOptions(startState);

        cancelledState.SetEndEffect(() => CancelledRewardEffect(cancelledState));
        continuesState.SetEndEffect(() => ContinuesRewardEffect(continuesState));
        doNothingState.SetEndEffect(() => DoNothingRewardEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(cancelledState.name, cancelledState);
        _states.Add(continuesState.name, continuesState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption preventOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Prevent him/her from leaving.",
                //description = "We have sent %minion% to watch the soldiers and follow them on their next secret meeting.",
                duration = 0,
                needsMinion = false,
                effect = () => PreventOption(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingOption(state),
            };

            state.AddActionOption(preventOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    private void PreventOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Cancelled", 20);
        effectWeights.AddElement("Continues", 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption(InteractionState state) {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement("Do Nothing", 15);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states["Do Nothing"]);
    }

    #region State Effects
    private void CancelledRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        state.AddLogFiller(new LogFiller(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.STRING_1));
    }
    private void ContinuesRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        state.AddLogFiller(new LogFiller(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.STRING_1));

        characterInvolved.currentParty.GoHome();
    }
    private void DoNothingRewardEffect(InteractionState state) {
        ContinuesRewardEffect(state);
    }
    #endregion
}
