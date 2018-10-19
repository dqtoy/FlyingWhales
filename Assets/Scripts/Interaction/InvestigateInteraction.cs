using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigateInteraction : Interaction {

    public InvestigateInteraction(IInteractable interactable) : base(interactable, INTERACTION_TYPE.INVESTIGATE) {

    }

    #region Overrides
    public override void CreateStates() {
        InteractionState uninvestigatedState = new InteractionState("Uninvestigated", this);
        InteractionState investigatedState = new InteractionState("Investigated", this);
        InteractionState attackLandmarkState = new InteractionState("Attack Landmark", this);
        InteractionState raidLandmarkState = new InteractionState("Raid Landmark", this);

        if (_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            string landmarkType = Utilities.NormalizeStringUpperCaseFirstLetters(landmark.specificLandmarkType.ToString());
            string uninvestigatedDesc = "This is a/an " + landmarkType.ToLower() + ". We must send an imp to gather further information about this place.";
            string investigatedDesc = "This is a/an " + landmarkType.ToLower() + ". We have an imp observing the place. You may recall the imp at any moment.";

            if (landmark.owner != null && landmark.owner.leader != null) {
                string landmarkOwner = Utilities.GetNormalizedRaceAdjective(landmark.owner.leader.race);
                uninvestigatedDesc = "This is a/an " + landmarkOwner.ToLower() + " " + landmarkType.ToLower() + ". We must send an imp to gather further information about this place.";
                investigatedDesc = "This is a/an " + landmarkOwner.ToLower() + " " + landmarkType.ToLower() + ". We have an imp observing the place. You may recall the imp at any moment.";
            }

            uninvestigatedState.SetDescription(uninvestigatedDesc);
            investigatedState.SetDescription(investigatedDesc);

            CreateActionOptions(uninvestigatedState);
            CreateActionOptions(investigatedState);
            CreateActionOptions(attackLandmarkState);
            CreateActionOptions(raidLandmarkState);

        }
        _states.Add(uninvestigatedState.name, uninvestigatedState);
        _states.Add(investigatedState.name, investigatedState);
        _states.Add(attackLandmarkState.name, attackLandmarkState);
        _states.Add(raidLandmarkState.name, raidLandmarkState);

        SetCurrentState(uninvestigatedState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if(state.name == "Uninvestigated") {
            ActionOption investigateOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 1, currency = CURRENCY.IMP },
                description = "Send an Imp.",
                duration = 1,
                needsMinion = false,
                effect = () => InvestigatedState()
            };
            ActionOption attackOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                description = "Attack it.",
                duration = 10,
                needsMinion = true,
                effect = () => AttackItState(state)
            };
            ActionOption raidOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                description = "Raid it.",
                duration = 10,
                needsMinion = true,
                effect = () => RaidItState(state),
                canBeDoneAction = CanBeRaided,
            };
            state.AddActionOption(investigateOption);
            state.AddActionOption(attackOption);
            state.AddActionOption(raidOption);
        } else if (state.name == "Investigated") {
            ActionOption uninvestigateOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.IMP },
                description = "Recall an Imp.",
                duration = 1,
                needsMinion = false,
                effect = () => UninvestigatedState()
            };
            ActionOption attackOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                description = "Attack it.",
                duration = 10,
                needsMinion = true,
                effect = () => AttackItState(state)
            };
            ActionOption raidOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                description = "Raid it.",
                duration = 10,
                needsMinion = true,
                effect = () => RaidItState(state),
                canBeDoneAction = CanBeRaided,

            };
            state.AddActionOption(uninvestigateOption);
            state.AddActionOption(attackOption);
            state.AddActionOption(raidOption);
        } else if (state.name == "Attack Landmark") {
            ActionOption okayOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                description = "Okay.",
                duration = 1,
                needsMinion = false,
                effect = () => OkayState(state)
            };
            state.AddActionOption(okayOption);
            state.SetDefaultOption(okayOption);
        } else if (state.name == "Raid Landmark") {
            ActionOption okayOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                description = "Okay.",
                duration = 1,
                needsMinion = false,
                effect = () => OkayState(state),
            };
            state.AddActionOption(okayOption);
            state.SetDefaultOption(okayOption);
        }
    }
    #endregion

    private void InvestigatedState() {
        SetCurrentState(_states["Investigated"]);
        _interactable.SetIsBeingInspected(true);
        if (!_interactable.hasBeenInspected) {
            _interactable.SetHasBeenInspected(true);
        }
    }
    private void UninvestigatedState() {
        SetCurrentState(_states["Uninvestigated"]);
        _interactable.SetIsBeingInspected(false);
        _interactable.EndedInspection();
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.IMP, 1);
    }
    private void AttackItState(InteractionState state) {
        AttackLandmarkState(state, "Attack Landmark");
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement("Attack Landmark", 15);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        //if (chosenEffect == "Attack Landmark") {
        //    AttackLandmarkState(state, chosenEffect);
        //}
    }
    private void RaidItState(InteractionState state) {
        RaidLandmarkState(state, "Raid Landmark");
    }
    private void OkayState(InteractionState state) {
        if (_interactable.isBeingInspected) {
            SetCurrentState(_states["Investigated"]);
        } else {
            SetCurrentState(_states["Uninvestigated"]);
        }
        state.AssignedMinionGoesBack();
    }
    private void AttackLandmarkState(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " has been sent to attack " + _interactable.specificLocation.tileLocation.landmarkOnTile.landmarkName);
        SetCurrentState(_states[effectName]);
        AttackLandmarkEffect(_states[effectName]);
    }
    private void AttackLandmarkEffect(InteractionState state) {
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        state.assignedMinion.icharacter.currentParty.iactionData.AssignAction(characterAction, _interactable.specificLocation.tileLocation.landmarkOnTile.landmarkObj);
        state.assignedMinion.icharacter.currentParty.currentAction.SetOnEndAction(() => state.ActivateDefault());
    }
    private void RaidLandmarkState(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " has been sent to " + _interactable.specificLocation.tileLocation.landmarkOnTile.landmarkName + " to raid it for supplies");
        SetCurrentState(_states[effectName]);
        RaidLandmarkEffect(_states[effectName]);
    }
    private void RaidLandmarkEffect(InteractionState state) {
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.RAID_LANDMARK);
        state.assignedMinion.icharacter.currentParty.iactionData.AssignAction(characterAction, _interactable.specificLocation.tileLocation.landmarkOnTile.landmarkObj);
        state.assignedMinion.icharacter.currentParty.currentAction.SetOnEndAction(() => state.ActivateDefault());
    }
    private bool CanBeRaided() {
        if (_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            return !landmark.isRaided;
        }
        return false;
    }
}
