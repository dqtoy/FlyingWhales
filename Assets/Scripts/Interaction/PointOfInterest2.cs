using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOfInterest2 : Interaction {
    public PointOfInterest2(IInteractable interactable) : base(interactable) {
    }
    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState supplyRewardState = new InteractionState("Supply Reward", this);
        InteractionState manaRewardState = new InteractionState("Mana Reward", this);
        InteractionState demonDisappearsRewardState = new InteractionState("Demon Disappears Reward", this);
        InteractionState demonBonusExpRewardState = new InteractionState("Demon Bonus Exp Reward", this);
        InteractionState demonWeaponUpgradeRewardState = new InteractionState("Demon Bonus Exp Reward", this);
        InteractionState demonArmorUpgradeRewardState = new InteractionState("Demon Bonus Exp Reward", this);

        if (_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            string startStateDesc = "Our imp has discovered a previously unexplored cave. We can send out a minion to explore further.";
            startState.SetDescription(startStateDesc);
            CreateActionOptions(startState);

            supplyRewardState.SetEndEffect(() => SupplyRewardEffect(supplyRewardState));
            manaRewardState.SetEndEffect(() => ManaRewardEffect(manaRewardState));
            demonDisappearsRewardState.SetEndEffect(() => DemonDisappearsRewardEffect(demonDisappearsRewardState));
            demonBonusExpRewardState.SetEndEffect(() => DemonBonusExpRewardEffect(demonBonusExpRewardState));

        }
        _states.Add(startState.name, startState);
        _states.Add(supplyRewardState.name, supplyRewardState);
        _states.Add(manaRewardState.name, manaRewardState);
        _states.Add(demonDisappearsRewardState.name, demonDisappearsRewardState);
        _states.Add(demonBonusExpRewardState.name, demonBonusExpRewardState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if(state.name == "Start") {
            ActionOption sendOutDemonOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                description = "Send out a Demon.",
                duration = 15,
                needsMinion = true,
                effect = () => SendOutDemonEffect(state),
            };

            ActionOption leaveAlonOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                description = "Leave it alone.",
                duration = 1,
                needsMinion = false,
                effect = () => LeaveAloneEffect(state),
            };
        }
    }
    #endregion

    private void SendOutDemonEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Supply Reward", 15);
        effectWeights.AddElement("Mana Reward", 10);
        effectWeights.AddElement("Demon Disappears Reward", 5);
        effectWeights.AddElement("Demon Bonus Exp Reward", 10);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if(chosenEffect == "Supply Reward") {
            SupplyReward(state, chosenEffect);
        }else if (chosenEffect == "Mana Reward") {
            ManaReward(state, chosenEffect);
        }else if (chosenEffect == "Demon Disappears Reward") {
            DemonDisappearsReward(state, chosenEffect);
        }else if (chosenEffect == "Demon Bonus Exp Reward") {
            DemonBonusExpReward(state, chosenEffect);
        }
    }
    private void LeaveAloneEffect(InteractionState state) {
        state.EndResult();
    }
    private void SupplyReward(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.assignedMinion.icharacter.name + " discovered a small cache of Supplies.");
        SetCurrentState(_states[effectName]);
    }
    private void ManaReward(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.assignedMinion.icharacter.name + " discovered a source of magical energy. We have converted it into a small amount of Mana.");
        SetCurrentState(_states[effectName]);
    }
    private void DemonDisappearsReward(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.assignedMinion.icharacter.name + " has not returned. We can only assume the worst.");
        SetCurrentState(_states[effectName]);
    }
    private void DemonBonusExpReward(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.assignedMinion.icharacter.name + " has returned with nothing but there seems to be a newfound strength within it.");
        SetCurrentState(_states[effectName]);
    }
    private void SupplyRewardEffect(InteractionState state) {
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, 40);
        state.assignedMinion.AdjustExp(1);
    }
    private void ManaRewardEffect(InteractionState state) {
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.MANA, 40);
        state.assignedMinion.AdjustExp(1);
    }
    private void DemonDisappearsRewardEffect(InteractionState state) {
        PlayerManager.Instance.player.RemoveMinion(state.assignedMinion);
    }
    private void DemonBonusExpRewardEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(2);
    }
}
