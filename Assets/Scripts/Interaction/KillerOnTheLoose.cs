using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillerOnTheLoose : Interaction {

    public KillerOnTheLoose(IInteractable interactable) : base(interactable, INTERACTION_TYPE.SUSPICIOUS_SOLDIER_MEETING, 70) {
        _name = "Killer On The Loose";
    }
    #region Overrides
    public override void CreateStates() {
        CreateExploreStates();
        CreateWhatToDoNextState("%minion% did not care about whether there is a serial killer on the loose. Do you want him to continue surveillance of " + _interactable.specificLocation.thisName + "?");

        InteractionState startState = new InteractionState("Start", this);
        InteractionState convertDemonState = new InteractionState("Convert Demon", this);
        InteractionState nothingHappensState = new InteractionState("Nothing Happens", this);
        InteractionState gainSupplyState = new InteractionState("Gain Supply", this);

        string startStateDesc = "%minion% reported rumors of a serial killer on the loose in " + _interactable.specificLocation.thisName + ". If there is truth to the rumor, a hardened criminal like that is a great candidate for a Demonic Conversion.";
        startState.SetDescription(startStateDesc);

        CreateActionOptions(startState);
        CreateActionOptions(convertDemonState);
        CreateActionOptions(nothingHappensState);
        CreateActionOptions(gainSupplyState);

        //convertDemonState.SetEndEffect(() => ConvertToDemonRewardEffect(convertDemonState));
        //gainSupplyState.SetEndEffect(() => GainSupplyRewardEffect(gainSupplyState));
        //nothingHappensState.SetEndEffect(() => NothingHappensRewardEffect(nothingHappensState));

        _states.Add(startState.name, startState);
        _states.Add(convertDemonState.name, convertDemonState);
        _states.Add(gainSupplyState.name, gainSupplyState);
        _states.Add(nothingHappensState.name, nothingHappensState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption searchKillerOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Search for the killer.",
                //description = "We have sent %minion% to search for the killer.",
                duration = 0,
                needsMinion = false,
                effect = () => SearchKillerOption(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                needsMinion = false,
                effect = () => WhatToDoNextState(),
            };

            state.AddActionOption(searchKillerOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
            //GameDate scheduleDate = GameManager.Instance.Today();
            //scheduleDate.AddHours(300);
            //state.SetTimeSchedule(doNothingOption, scheduleDate);
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

    private void SearchKillerOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Convert Demon", 20);
        effectWeights.AddElement("Nothing Happens", 25);
        effectWeights.AddElement("Gain Supply", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Convert Demon") {
            ConvertToDemonRewardState(state, chosenEffect);
        } else if (chosenEffect == "Gain Supply") {
            GainSupplyRewardState(state, chosenEffect);
        } else if (chosenEffect == "Nothing Happens") {
            NothingHappensRewardState(state, chosenEffect);
        }
    }

    #region States
    private void ConvertToDemonRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription(_interactable.explorerMinion.name + " was able to find the killer and restrain him. After a lengthy Transform Ritual, we have summoned a new Envy Demon into his corrupted body.");
        SetCurrentState(_states[stateName]);
        ConvertToDemonRewardEffect(_states[stateName]);
    }
    private void GainSupplyRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription(_interactable.explorerMinion.name + " managed to track the killer to his hideout but just as he was about to restrain him, guards arrived forcing him to retreat. They killed the criminal and obtained some supplies from his hideout.");
        SetCurrentState(_states[stateName]);
        GainSupplyRewardEffect(_states[stateName]);
    }
    private void NothingHappensRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription("After hours of searching, " + _interactable.explorerMinion.name + " was still unable to find any trace of the killer. It has returned empty handed.");
        SetCurrentState(_states[stateName]);
        NothingHappensRewardEffect(_states[stateName]);
    }
    #endregion

    #region State Effects
    private void ConvertToDemonRewardEffect(InteractionState state) {
        PlayerManager.Instance.player.AddMinion(PlayerManager.Instance.player.CreateNewMinion("Farmer", RACE.DEMON, DEMON_TYPE.ENVY, "Inspect", false));
    }
    private void GainSupplyRewardEffect(InteractionState state) {
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        _interactable.specificLocation.tileLocation.areaOfTile.AdjustSuppliesInBank(50);
    }
    private void NothingHappensRewardEffect(InteractionState state) {
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
    #endregion
}
