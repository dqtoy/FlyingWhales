using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigateInteraction : Interaction {

    public InvestigateInteraction(IInteractable interactable) : base(interactable) {

    }

    #region Overrides
    public override void CreateStates() {
        InteractionState uninvestigatedState = new InteractionState("Uninvestigated");
        InteractionState investigatedState = new InteractionState("Investigated");

        if(_interactable is BaseLandmark) {
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
        }
        _states.Add(uninvestigatedState.name, uninvestigatedState);
        _states.Add(investigatedState.name, investigatedState);
        SetCurrentState(uninvestigatedState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if(state.name == "Uninvestigated") {
            ActionOption actionOption = new ActionOption {
                interaction = this,
                cost = new ActionOptionCost { amount = 1, currency = CURRENCY.IMP },
                description = "Send an Imp",
                duration = 5,
                needsMinion = false,
                effect = () => ChangeToInvestigatedState()
            };
            state.AddActionOption(actionOption);
        }else if (state.name == "Investigated") {
            ActionOption actionOption = new ActionOption {
                interaction = this,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.IMP },
                description = "Recall an Imp",
                duration = 1,
                needsMinion = false,
                effect = () => ChangeToUninvestigatedState()
            };
            state.AddActionOption(actionOption);
        }
    }
    #endregion

    private void ChangeToInvestigatedState() {
        SetCurrentState(_states["Investigated"]);
        _interactable.SetIsBeingInspected(true);
        if (!_interactable.hasBeenInspected) {
            _interactable.SetHasBeenInspected(true);
        }
    }
    private void ChangeToUninvestigatedState() {
        SetCurrentState(_states["Uninvestigated"]);
        _interactable.SetIsBeingInspected(false);
        _interactable.EndedInspection();
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.IMP, 1);
    }
}
