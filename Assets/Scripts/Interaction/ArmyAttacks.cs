using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyAttacks : Interaction {

    private BaseLandmark landmark;

    private const string endResult1Name = "Stop Successful";
    private const string endResult2Name = "Stop Failure";
    private const string endResult3Name = "Stop Critical Failure";
    private const string endResult4Name = "Redirection Successful";
    private const string endResult5Name = "Redirection Failure";

    public ArmyAttacks(IInteractable interactable) : base(interactable, INTERACTION_TYPE.ARMY_ATTACKS) {
        _name = "Army Attacks";
    }

    #region Overrides
    public override void CreateStates() {
        if (_interactable is BaseLandmark) {
            landmark = _interactable as BaseLandmark;

            InteractionState startState = new InteractionState("State 1", this);
            string startStateDesc = "The Garrison is preparing to attack " + landmark.name + ".";
            startState.SetDescription(startStateDesc);
            CreateActionOptions(startState);

            //action option states
            InteractionState stopSuccessfulState = new InteractionState(endResult1Name, this);
            InteractionState stopFailureState = new InteractionState(endResult2Name, this);
            InteractionState stopCriticalFailureState = new InteractionState(endResult3Name, this);
            InteractionState redirectionSuccessfulState = new InteractionState(endResult4Name, this);
            InteractionState redirectionFailureState = new InteractionState(endResult5Name, this);

            stopSuccessfulState.SetEndEffect(() => StopSuccessfulEffect(stopSuccessfulState));
            stopFailureState.SetEndEffect(() => StopFailureEffect(stopFailureState));
            stopCriticalFailureState.SetEndEffect(() => StopCriticalFailureEffect(stopCriticalFailureState));
            redirectionSuccessfulState.SetEndEffect(() => RedirectionSuccessfulEffect(redirectionSuccessfulState));
            redirectionFailureState.SetEndEffect(() => RedirectionFailureEffect(redirectionFailureState));

            _states.Add(startState.name, startState);
            _states.Add(stopSuccessfulState.name, stopSuccessfulState);
            _states.Add(stopFailureState.name, stopFailureState);
            _states.Add(stopCriticalFailureState.name, stopCriticalFailureState);
            _states.Add(redirectionSuccessfulState.name, redirectionSuccessfulState);
            _states.Add(redirectionFailureState.name, redirectionFailureState);

            SetCurrentState(startState);
        }
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "State 1") {
            ActionOption stopThem = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Stop Them.",
                duration = 10,
                needsMinion = true,
                neededObjects = new List<System.Type>() { typeof(Minion) },
                effect = () => StopThemEffect(state),
            };
            ActionOption redirectAttack = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 30, currency = CURRENCY.SUPPLY },
                name = "Redirect their attack.",
                duration = 10,
                needsMinion = true,
                neededObjects = new List<System.Type>() { typeof(Minion), typeof(LocationIntel) },
                effect = () => RedirectAttackEffect(state),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                description = "The bandits are increasing their defensive army.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingEffect(state),
                onStartDurationAction = () => SetDefaultActionDurationAsRemainingTicks("Do nothing.", state),
            };
            state.AddActionOption(stopThem);
            state.AddActionOption(redirectAttack);
            state.AddActionOption(doNothing);

            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddHours(50);
            state.SetTimeSchedule(doNothing, dueDate); //default is do nothing
        }
    }
    #endregion

    private void StopThemEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(endResult1Name, 30);
        effectWeights.AddElement(endResult2Name, 10);
        effectWeights.AddElement(endResult3Name, 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == endResult1Name) {
            StopSuccessful(state, chosenEffect);
        } else if (chosenEffect == endResult2Name) {
            StopFailure(state, chosenEffect);
        } else if (chosenEffect == endResult3Name) {
            StopCriticalFailure(state, chosenEffect);
        }
    }
    private void RedirectAttackEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(endResult4Name, 30);
        effectWeights.AddElement(endResult5Name, 10);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == endResult4Name) {
            RedirectionSuccessful(state, chosenEffect);
        } else if (chosenEffect == endResult5Name) {
            RedirectionFailure(state, chosenEffect);
        }
    }
    private void DoNothingEffect(InteractionState state) {
        state.EndResult();
    }

    private void StopSuccessful(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " disguised himself and talked to the Army General, eventually convincing him to cancel their attack.");
        SetCurrentState(_states[effectName]);
    }
    private void StopSuccessfulEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        state.chosenOption.assignedMinion.AdjustExp(1);
    }
    private void StopFailure(InteractionState state, string effectName) {
        //TODO: **Mechanics**: Army Unit with most occupied slots will Attack the selected enemy location.
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " disguised himself and talked to the Army General, but was unable to convince him to cancel their attack.");
        SetCurrentState(_states[effectName]);
    }
    private void StopFailureEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        state.chosenOption.assignedMinion.AdjustExp(1);
    }
    private void StopCriticalFailure(InteractionState state, string effectName) {
        //TODO: **Mechanics**: Army Unit with most occupied slots will Attack a player location.
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " disguised himself and talked to the Army General, but was unable to convince him to cancel their attack. Annoyed with the demon, the General redirected the attack to us!");
        SetCurrentState(_states[effectName]);
    }
    private void StopCriticalFailureEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        state.chosenOption.assignedMinion.AdjustExp(1);
    }

    private void RedirectionSuccessful(InteractionState state, string effectName) {
        //TODO: **Mechanics**: Army Unit with most occupied slots will Attack assigned Location Intel.
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " disguised himself and talked to the Army General, eventually convincing him to redirect their attack to " + landmark.name + ".");
        SetCurrentState(_states[effectName]);
    }
    private void RedirectionSuccessfulEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        state.chosenOption.assignedMinion.AdjustExp(1);
    }
    private void RedirectionFailure(InteractionState state, string effectName) {
        //TODO: **Mechanics**: Army Unit with most occupied slots will Attack a player location.
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " disguised himself and talked to the Army General, but failed to convince him to redirect their attack to " + landmark.name + ".");
        SetCurrentState(_states[effectName]);
    }
    private void RedirectionFailureEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        state.chosenOption.assignedMinion.AdjustExp(1);
    }
}
