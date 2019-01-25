using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillerOnTheLoose : Interaction {

    public KillerOnTheLoose(Area interactable) : base(interactable, INTERACTION_TYPE.KILLER_ON_THE_LOOSE, 70) {
        _name = "Killer On The Loose";
    }
    #region Overrides
    public override void CreateStates() {
        //CreateExploreStates();
        //CreateWhatToDoNextState("%minion% did not care about whether there is a serial killer on the loose. Do you want him to continue surveillance of " + _interactable.thisName + "?");

        InteractionState startState = new InteractionState("Start", this);
        InteractionState convertDemonState = new InteractionState("Convert Demon", this);
        InteractionState nothingHappensState = new InteractionState("Nothing Happens", this);
        InteractionState gainSupplyState = new InteractionState("Gain Supply", this);
        InteractionState doNothingState = new InteractionState("Do Nothing", this);

        //string startStateDesc = "%minion% reported rumors of a serial killer on the loose in " + _interactable.thisName + ". If there is truth to the rumor, a hardened criminal like that is a great candidate for a Demonic Conversion.";
        //startState.SetDescription(startStateDesc);

        CreateActionOptions(startState);
        //CreateActionOptions(convertDemonState);
        //CreateActionOptions(nothingHappensState);
        //CreateActionOptions(gainSupplyState);

        convertDemonState.SetEffect(() => ConvertToDemonRewardEffect(convertDemonState));
        gainSupplyState.SetEffect(() => GainSupplyRewardEffect(gainSupplyState));
        nothingHappensState.SetEffect(() => NothingHappensRewardEffect(nothingHappensState));
        doNothingState.SetEffect(() => DoNothingRewardEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(convertDemonState.name, convertDemonState);
        _states.Add(gainSupplyState.name, gainSupplyState);
        _states.Add(nothingHappensState.name, nothingHappensState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption searchKillerOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Search for the killer.",
                //description = "We have sent %minion% to search for the killer.",
                duration = 0,
                effect = () => SearchKillerOption(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(state),
            };

            state.AddActionOption(searchKillerOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
            //GameDate scheduleDate = GameManager.Instance.Today();
            //scheduleDate.AddHours(300);
            //state.SetTimeSchedule(doNothingOption, scheduleDate);
        }
    }
    #endregion

    private void SearchKillerOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Convert Demon", 20);
        effectWeights.AddElement("Nothing Happens", 25);
        effectWeights.AddElement("Gain Supply", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);

        //if (chosenEffect == "Convert Demon") {
        //    ConvertToDemonRewardState(state, chosenEffect);
        //} else if (chosenEffect == "Gain Supply") {
        //    GainSupplyRewardState(state, chosenEffect);
        //} else if (chosenEffect == "Nothing Happens") {
        //    NothingHappensRewardState(state, chosenEffect);
        //}
    }
    private void DoNothingOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Do Nothing", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);

        //if (chosenEffect == "Left Alone") {
        //    LeftAloneRewardState(state, chosenEffect);
        //}
    }

    #region States
    private void ConvertToDemonRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " was able to find the killer and restrain him. After a lengthy Transform Ritual, we have summoned a new Envy Demon into his corrupted body.");
        SetCurrentState(_states[stateName]);
        ConvertToDemonRewardEffect(_states[stateName]);
    }
    private void GainSupplyRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " managed to track the killer to his hideout but just as he was about to restrain him, guards arrived forcing him to retreat. They killed the criminal and obtained some supplies from his hideout.");
        SetCurrentState(_states[stateName]);
        GainSupplyRewardEffect(_states[stateName]);
    }
    private void NothingHappensRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription("After hours of searching, " + explorerMinion.name + " was still unable to find any trace of the killer. It has returned empty handed.");
        SetCurrentState(_states[stateName]);
        NothingHappensRewardEffect(_states[stateName]);
    }
    #endregion

    #region State Effects
    private void ConvertToDemonRewardEffect(InteractionState state) {
        Minion createdMinion = PlayerManager.Instance.player.CreateNewMinion("Envy", RACE.DEMON);
        PlayerManager.Instance.player.AddMinion(createdMinion);
        state.AddLogFiller(new LogFiller(createdMinion, createdMinion.name, LOG_IDENTIFIER.STRING_1));
    }
    private void GainSupplyRewardEffect(InteractionState state) {
        investigatorCharacter.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
        _interactable.AdjustSuppliesInBank(50);
        state.AddLogFiller(new LogFiller(_interactable.owner, _interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void NothingHappensRewardEffect(InteractionState state) {
        investigatorCharacter.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_2));
    }
    #endregion
}
