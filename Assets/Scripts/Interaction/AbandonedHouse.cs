﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbandonedHouse : Interaction {
    public AbandonedHouse(IInteractable interactable) : base(interactable, INTERACTION_TYPE.ABANDONED_HOUSE, 50) {
        _name = "Abandoned House";
    }

    #region Overrides
    public override void CreateStates() {
        //CreateExploreStates();
        //CreateWhatToDoNextState("%minion% ignored the point of interest in the location. Do you want him to continue surveillance of " + _interactable.specificLocation.thisName + "?");

        InteractionState startState = new InteractionState("Start", this);
        InteractionState supplyState = new InteractionState("Supply", this);
        InteractionState manaState = new InteractionState("Mana", this);
        InteractionState demonDisappearsState = new InteractionState("Demon Disappears", this);
        InteractionState demonBonusExpState = new InteractionState("Demon Bonus Exp", this);
        InteractionState leftAloneState = new InteractionState("Left Alone", this);

        //string startStateDesc = "%minion% has discovered an abandoned mansion. It appears to be one of the oldest structure in the city. It seems to have been unoccupied for decades and we don't know exactly why it's left this way for so long. We can send him inside to investigate.";
        //startState.SetDescription(startStateDesc);

        CreateActionOptions(startState);
        //CreateActionOptions(supplyState);
        //CreateActionOptions(manaState);
        //CreateActionOptions(demonBonusExpState);

        supplyState.SetEndEffect(() => SupplyRewardEffect(supplyState));
        manaState.SetEndEffect(() => ManaRewardEffect(manaState));
        demonDisappearsState.SetEndEffect(() => DemonDisappearsRewardEffect(demonDisappearsState));
        demonBonusExpState.SetEndEffect(() => DemonBonusExpRewardEffect(demonBonusExpState));
        leftAloneState.SetEndEffect(() => LeftAloneRewardEffect(leftAloneState));

        _states.Add(startState.name, startState);
        _states.Add(supplyState.name, supplyState);
        _states.Add(manaState.name, manaState);
        _states.Add(demonDisappearsState.name, demonDisappearsState);
        _states.Add(demonBonusExpState.name, demonBonusExpState);
        _states.Add(leftAloneState.name, leftAloneState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if(state.name == "Start") {
            ActionOption sendOutDemonOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Send out a Demon.",
                //description = "We have sent %minion% to explore the interesting location.",
                duration = 0,
                needsMinion = false,
                effect = () => SendOutDemonOption(state),
            };
            ActionOption leaveAloneOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Leave it alone.",
                duration = 0,
                needsMinion = false,
                effect = () => LeftAloneOption(state),
            };

            state.AddActionOption(sendOutDemonOption);
            state.AddActionOption(leaveAloneOption);
            state.SetDefaultOption(leaveAloneOption);
        } else {
            ActionOption continueSurveillanceOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Continue surveillance of the area.",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreContinuesOption(state),
            };
            ActionOption returnToMeOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
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

    private void SendOutDemonOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Supply", 15);
        effectWeights.AddElement("Mana", 10);
        effectWeights.AddElement("Demon Disappears", 5);
        effectWeights.AddElement("Demon Bonus Exp", 10);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);

        //if (chosenEffect == "Supply") {
        //    SupplyRewardState(state, chosenEffect);
        //}else if (chosenEffect == "Mana") {
        //    ManaRewardState(state, chosenEffect);
        //}else if (chosenEffect == "Demon Disappears") {
        //    DemonDisappearsRewardState(state, chosenEffect);
        //}else if (chosenEffect == "Demon Bonus Exp") {
        //    DemonBonusExpRewardState(state, chosenEffect);
        //}
    }
    private void LeftAloneOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Left Alone", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);

        //if (chosenEffect == "Left Alone") {
        //    LeftAloneRewardState(state, chosenEffect);
        //}
    }
    private void DemonBonusExpRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(_interactable.explorerMinion.name + " has returned with nothing but there seems to be a newfound strength within it.");
        SetCurrentState(_states[effectName]);
    }
    private void DemonBonusExpRewardEffect(InteractionState state) {
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_2));
    }
    private void LeftAloneRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription("We left the abandoned mansion alone.");
        SetCurrentState(_states[effectName]);
    }
    private void LeftAloneRewardEffect(InteractionState state) {
        //_interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_2));
    }
}