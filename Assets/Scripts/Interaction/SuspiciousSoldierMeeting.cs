using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspiciousSoldierMeeting : Interaction {

    public SuspiciousSoldierMeeting(IInteractable interactable) : base(interactable, INTERACTION_TYPE.SUSPICIOUS_SOLDIER_MEETING) {
        _name = "Suspicious Soldier Meeting";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState reduceDefendersState = new InteractionState("Reduce Defenders", this);
        InteractionState warDeclaredState = new InteractionState("War Declared", this);
        InteractionState demonDisappearsState = new InteractionState("Demon Disappears", this);
        InteractionState generalDiesState = new InteractionState("General Dies", this);
        InteractionState nothingHappensState = new InteractionState("Nothing Happens", this);

        string startStateDesc = "Our Imp has discovered a group of soldiers leaving the Garrison and meeting in secret.";
        startState.SetDescription(startStateDesc);
        CreateActionOptions(startState);

        reduceDefendersState.SetEndEffect(() => ReduceDefendersRewardEffect(reduceDefendersState));
        warDeclaredState.SetEndEffect(() => WarDeclaredRewardEffect(warDeclaredState));
        demonDisappearsState.SetEndEffect(() => DemonDisappearsRewardEffect(demonDisappearsState));
        generalDiesState.SetEndEffect(() => GeneralDiesRewardEffect(generalDiesState));
        nothingHappensState.SetEndEffect(() => NothingHappensRewardEffect(nothingHappensState));

        _states.Add(startState.name, startState);
        _states.Add(reduceDefendersState.name, reduceDefendersState);
        _states.Add(warDeclaredState.name, warDeclaredState);
        _states.Add(demonDisappearsState.name, demonDisappearsState);
        _states.Add(generalDiesState.name, generalDiesState);
        _states.Add(nothingHappensState.name, nothingHappensState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption sendOutDemonOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 30, currency = CURRENCY.SUPPLY },
                name = "Send out a Demon.",
                description = "We have sent %minion% to watch the soldiers and follow them on their next secret meeting.",
                duration = 10,
                needsMinion = true,
                effect = () => SendOutDemonOption(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                needsMinion = false,
                effect = () => LeaveAloneEffect(state),
            };

            state.AddActionOption(sendOutDemonOption);
            state.AddActionOption(doNothingOption);

            GameDate scheduleDate = GameManager.Instance.Today();
            scheduleDate.AddHours(60);
            state.SetTimeSchedule(doNothingOption, scheduleDate);
        }
    }
    #endregion

    private void SendOutDemonOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Reduce Defenders", 30);
        effectWeights.AddElement("Demon Disappears", 5);
        effectWeights.AddElement("War Declared", 5);
        effectWeights.AddElement("General Dies", 5);
        effectWeights.AddElement("Nothing Happens", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Reduce Defenders") {
            ReduceDefendersRewardState(state, chosenEffect);
        } else if (chosenEffect == "Demon Disappears") {
            DemonDisappearsRewardState(state, chosenEffect);
        } else if (chosenEffect == "War Declared") {
            WarDeclaredRewardState(state, chosenEffect);
        } else if (chosenEffect == "General Dies") {
            GeneralDiesRewardState(state, chosenEffect);
        } else if (chosenEffect == "Nothing Happens") {
            NothingHappensRewardState(state, chosenEffect);
        }
    }

    #region States
    private void ReduceDefendersRewardState(InteractionState state, string stateName) {

    }
    private void WarDeclaredRewardState(InteractionState state, string stateName) {

    }
    private void GeneralDiesRewardState(InteractionState state, string stateName) {

    }
    private void NothingHappensRewardState(InteractionState state, string stateName) {

    }
    #endregion

    #region State Effects
    private void ReduceDefendersRewardEffect(InteractionState state) {
    }
    private void WarDeclaredRewardEffect(InteractionState state) {
    }
    private void GeneralDiesRewardEffect(InteractionState state) {
    }
    private void NothingHappensRewardEffect(InteractionState state) {
    }
    #endregion
}
