using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class UnexploredCave : Interaction {
    public UnexploredCave(IInteractable interactable) : base(interactable, INTERACTION_TYPE.UNEXPLORED_CAVE, 30) {
        _name = "Unexplored Cave";
    }
    #region Overrides
    public override void CreateStates() {
        CreateExploreStates();
        CreateWhatToDoNextState("%minion% ignored the point of interest in the location. Do you want him to continue surveillance of " + _interactable.specificLocation.thisName + "?");

        InteractionState startState = new InteractionState("Start", this);
        InteractionState supplyState = new InteractionState("Supply", this);
        InteractionState manaState = new InteractionState("Mana", this);
        InteractionState demonDisappearsState = new InteractionState("Demon Disappears", this);
        InteractionState demonWeaponUpgradeState = new InteractionState("Demon Weapon Upgrade", this);
        InteractionState demonArmorUpgradeState = new InteractionState("Demon Arnor Upgrade", this);
        InteractionState unleashedMonsterState = new InteractionState("Unleashed Monster", this);
        InteractionState nothingState = new InteractionState("Nothing", this);

        //string startStateDesc = "%minion% has discovered a previously unexplored cave. We can send out a minion to explore further.";
        //startState.SetDescription(startStateDesc);

        CreateActionOptions(startState);
        CreateActionOptions(supplyState);
        CreateActionOptions(manaState);
        CreateActionOptions(demonWeaponUpgradeState);
        CreateActionOptions(demonArmorUpgradeState);
        CreateActionOptions(unleashedMonsterState);
        CreateActionOptions(nothingState);

        //supplyState.SetEndEffect(() => SupplyRewardEffect(supplyState));
        //manaState.SetEndEffect(() => ManaRewardEffect(manaState));
        demonDisappearsState.SetEndEffect(() => DemonDisappearsRewardEffect(demonDisappearsState));
        //demonWeaponUpgradeState.SetEndEffect(() => DemonWeaponUpgradeEffect(demonWeaponUpgradeState));
        //demonArmorUpgradeState.SetEndEffect(() => DemonArmorUpgradeEffect(demonArmorUpgradeState));
        //unleashedMonsterState.SetEndEffect(() => UnleashedMonsterEffect(unleashedMonsterState));
        //nothingState.SetEndEffect(() => NothingEffect(nothingState));

        _states.Add(startState.name, startState);
        _states.Add(supplyState.name, supplyState);
        _states.Add(manaState.name, manaState);
        _states.Add(demonDisappearsState.name, demonDisappearsState);
        _states.Add(demonWeaponUpgradeState.name, demonWeaponUpgradeState);
        _states.Add(demonArmorUpgradeState.name, demonArmorUpgradeState);
        _states.Add(unleashedMonsterState.name, unleashedMonsterState);
        _states.Add(nothingState.name, nothingState);

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
                effect = () => SendOutDemonEffect(state),
            };

            ActionOption leaveAloneOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Leave it alone.",
                duration = 0,
                needsMinion = false,
                effect = () => WhatToDoNextState(),
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

    private void SendOutDemonEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Supply", 15);
        effectWeights.AddElement("Mana", 10);
        effectWeights.AddElement("Demon Disappears", 5);
        effectWeights.AddElement("Demon Weapon Upgrade", 10);
        effectWeights.AddElement("Demon Armor Upgrade", 10);
        //effectWeights.AddElement("Unleashed Monster", 5);
        effectWeights.AddElement("Nothing", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if(chosenEffect == "Supply") {
            SupplyRewardState(state, chosenEffect);
        }else if (chosenEffect == "Mana") {
            ManaRewardState(state, chosenEffect);
        }else if (chosenEffect == "Demon Disappears") {
            DemonDisappearsRewardState(state, chosenEffect);
        }else if (chosenEffect == "Demon Weapon Upgrade") {
            DemonWeaponUpgradeRewardState(state, chosenEffect);
        }else if (chosenEffect == "Demon Armor Upgrade") {
            DemonArmorUpgradeRewardState(state, chosenEffect);
        }else if (chosenEffect == "Unleashed Monster") {
            UnleashedMonsterRewardState(state, chosenEffect);
        }else if (chosenEffect == "Nothing") {
            NothingRewardState(state, chosenEffect);
        }
    }
    private void DemonWeaponUpgradeRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(_interactable.explorerMinion.name + " has returned with an improved Weapon.");
        SetCurrentState(_states[effectName]);
        DemonWeaponUpgradeEffect(_states[effectName]);
    }
    private void DemonArmorUpgradeRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(_interactable.explorerMinion.name + " has returned with an improved Armor.");
        SetCurrentState(_states[effectName]);
        DemonArmorUpgradeEffect(_states[effectName]);
    }
    private void UnleashedMonsterRewardState(InteractionState state, string effectName) {
        if(_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            if(landmark.charactersWithHomeOnLandmark.Count > 0) {
                //_states[effectName].SetDescription(_interactable.explorerMinion.name + " has awakened a " + landmark.charactersWithHomeOnLandmark[0].name + ". It now defends the cave from intruders.");
                SetCurrentState(_states[effectName]);
                UnleashedMonsterEffect(_states[effectName]);
            }
        }
    }
    private void DemonWeaponUpgradeEffect(InteractionState state) {
        if(_interactable.explorerMinion.icharacter is Character) {
            Character character = _interactable.explorerMinion.icharacter as Character;
            character.UpgradeWeapon();
        }
    }
    private void DemonArmorUpgradeEffect(InteractionState state) {
        if (_interactable.explorerMinion.icharacter is Character) {
            Character character = _interactable.explorerMinion.icharacter as Character;
            character.UpgradeArmor();
        }
    }
    private void UnleashedMonsterEffect(InteractionState state) {
        //TODO: awaken monster and put it in defenders list
    }
}
