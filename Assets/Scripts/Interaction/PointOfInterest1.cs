using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOfInterest1 : Interaction {
    public PointOfInterest1(IInteractable interactable) : base(interactable, INTERACTION_TYPE.POI_1) {
        _name = "Point of Interest 1";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState supplyState = new InteractionState("Supply", this);
        InteractionState manaState = new InteractionState("Mana", this);
        InteractionState demonDisappearsState = new InteractionState("Demon Disappears", this);
        InteractionState demonBonusExpState = new InteractionState("Demon Bonus Exp", this);

        if (_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            string startStateDesc = "Our imp has discovered a new point of interest in " + landmark.landmarkName + ". We can send out a minion to investigate.";
            startState.SetDescription(startStateDesc);
            CreateActionOptions(startState);

            supplyState.SetEndEffect(() => SupplyRewardEffect(supplyState));
            manaState.SetEndEffect(() => ManaRewardEffect(manaState));
            demonDisappearsState.SetEndEffect(() => DemonDisappearsRewardEffect(demonDisappearsState));
            demonBonusExpState.SetEndEffect(() => DemonBonusExpRewardEffect(demonBonusExpState));

        }
        _states.Add(startState.name, startState);
        _states.Add(supplyState.name, supplyState);
        _states.Add(manaState.name, manaState);
        _states.Add(demonDisappearsState.name, demonDisappearsState);
        _states.Add(demonBonusExpState.name, demonBonusExpState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if(state.name == "Start") {
            ActionOption sendOutDemonOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Send out a Demon.",
                description = "We have sent %minion% to explore the interesting location.",
                duration = 15,
                needsMinion = true,
                effect = () => SendOutDemonEffect(state),
            };
            ActionOption leaveAloneOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Leave it alone.",
                duration = 1,
                needsMinion = false,
                effect = () => LeaveAloneEffect(state),
            };

            state.AddActionOption(sendOutDemonOption);
            state.AddActionOption(leaveAloneOption);
        }
    }
    #endregion

    private void SendOutDemonEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Supply", 15);
        effectWeights.AddElement("Mana", 10);
        effectWeights.AddElement("Demon Disappears", 5);
        effectWeights.AddElement("Demon Bonus Exp", 10);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if(chosenEffect == "Supply") {
            SupplyRewardState(state, chosenEffect);
        }else if (chosenEffect == "Mana") {
            ManaRewardState(state, chosenEffect);
        }else if (chosenEffect == "Demon Disappears") {
            DemonDisappearsRewardState(state, chosenEffect);
        }else if (chosenEffect == "Demon Bonus Exp") {
            DemonBonusExpRewardState(state, chosenEffect);
        }
    }
    private void DemonBonusExpRewardState(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " has returned with nothing but there seems to be a newfound strength within it.");
        SetCurrentState(_states[effectName]);
    }
    private void DemonBonusExpRewardEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(2);
    }
}
