using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnfinishedCurse : Interaction {

    private const string endResult1Name = "Curse 1 Completed";
    private const string endResult2Name = "Curse 2 Completed";
    private const string endResult3Name = "Curse 3 Completed";
    private const string endResult4Name = "Curse Failed To Complete";
    private const string endResult5Name = "Obtain Mana";

    public UnfinishedCurse(IInteractable interactable) : base(interactable, INTERACTION_TYPE.UNFINISHED_CURSE) {
        _name = "Unfinished Curse";
    }

    #region Overrides
    public override void CreateStates() {
        if (_interactable is BaseLandmark) {

            InteractionState startState = new InteractionState("State 1", this);
            string startStateDesc = "Our imp has reported what appears to be an ancient unfinished curse placed within one of the cemetery mausoleums. We may be able to complete the curse but we aren't aware of what it's actual effect would be, if any.";
            startState.SetDescription(startStateDesc);
            CreateActionOptions(startState);
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddHours(100);
            startState.SetTimeSchedule(startState.actionOptions[2], dueDate); //default is do nothing

            //action option states
            InteractionState endResult1State = new InteractionState(endResult1Name, this); //raid
            InteractionState endResult2State = new InteractionState(endResult2Name, this); //successfully cancelled raid
            InteractionState endResult3State = new InteractionState(endResult3Name, this); //failed to cancel raid
            InteractionState endResult4State = new InteractionState(endResult4Name, this); //critically failed to cancel raid
            InteractionState endResult5State = new InteractionState(endResult5Name, this); //empowered raid

            endResult1State.SetEndEffect(() => Curse1CompletedEffect(endResult1State));
            endResult2State.SetEndEffect(() => Curse2CompletedEffect(endResult2State));
            endResult3State.SetEndEffect(() => Curse3CompletedEffect(endResult3State));
            endResult4State.SetEndEffect(() => CurseFailedToCompleteEffect(endResult4State));
            endResult5State.SetEndEffect(() => ObtainManaEffect(endResult5State));

            _states.Add(startState.name, startState);
            _states.Add(endResult1State.name, endResult1State);
            _states.Add(endResult2State.name, endResult2State);
            _states.Add(endResult3State.name, endResult3State);
            _states.Add(endResult4State.name, endResult4State);
            _states.Add(endResult5State.name, endResult5State);

            SetCurrentState(startState);
        }
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "State 1") {
            ActionOption completeCurse = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 5, currency = CURRENCY.MANA },
                name = "Attempt to complete the curse.",
                duration = 10,
                needsMinion = true,
                effect = () => CompleteCurseEffect(state),
            };
            ActionOption harnessMagic = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Harness its magic into Mana.",
                duration = 10,
                needsMinion = true,
                effect = () => HarnessMagicEffect(state),
            };
            ActionOption leaveAlone = new ActionOption {
                interactionState = state,
                name = "Leave it alone.",
                duration = 0,
                needsMinion = false,
                effect = () => LeaveItAloneEffect(state),
            };

            state.AddActionOption(completeCurse);
            state.AddActionOption(harnessMagic);
            state.AddActionOption(leaveAlone);
        }
    }
    #endregion

    private void CompleteCurseEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(endResult1Name, 30);
        effectWeights.AddElement(endResult2Name, 20);
        effectWeights.AddElement(endResult3Name, 10);
        effectWeights.AddElement(endResult4Name, 10);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == endResult1Name) {
            Curse1Completed(state, chosenEffect);
        } else if (chosenEffect == endResult2Name) {
            Curse2Completed(state, chosenEffect);
        } else if (chosenEffect == endResult3Name) {
            Curse3Completed(state, chosenEffect);
        } else if (chosenEffect == endResult4Name) {
            CurseFailedToComplete(state, chosenEffect);
        }
    }
    private void HarnessMagicEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(endResult5Name, 10);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == endResult5Name) {
            Curse1Completed(state, chosenEffect);
        }
    }
    private void LeaveItAloneEffect(InteractionState state) {
        state.EndResult(); //Immediately ends interaction
    }

    private void Curse1Completed(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " tried a couple of chants to complete the curse. After the latest one, a violent thunderstrike sounded from the distance.");
        SetCurrentState(_states[effectName]);
    }
    private void Curse1CompletedEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(1); //**Reward**: Demon gains Exp 1
    }
    private void Curse2Completed(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " tried a couple of chants to complete the curse. After the latest one, a violent thunderstrike sounded from the distance.");
        SetCurrentState(_states[effectName]);
    }
    private void Curse2CompletedEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(1); //**Reward**: Demon gains Exp 1
    }
    private void Curse3Completed(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " tried a couple of chants to complete the curse. After the latest one, a violent thunderstrike sounded from the distance.");
        SetCurrentState(_states[effectName]);
    }
    private void Curse3CompletedEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(1); //**Reward**: Demon gains Exp 1
    }
    private void CurseFailedToComplete(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " attempted to complete the ancient curse but failed to figure out the proper chant to make it work.");
        SetCurrentState(_states[effectName]);
    }
    private void CurseFailedToCompleteEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(1); //**Reward**: Demon gains Exp 1
    }

    private void ObtainMana(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " discovered a source of magical energy. We have converted it into a small amount of Mana.");
        SetCurrentState(_states[effectName]);
    }
    private void ObtainManaEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(1); //**Reward**: Demon gains Exp 1
    }
}
