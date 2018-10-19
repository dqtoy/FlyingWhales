using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanditRaid : Interaction {

    public BanditRaid(IInteractable interactable) : base(interactable, INTERACTION_TYPE.BANDIT_RAID) {
    }

    #region Overrides
    public override void CreateStates() {
        if (_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;

            BaseLandmark chosenLandmarkToRaid;
            List<HexTile> surrounding = landmark.tileLocation.GetTilesInRange(8);
            List<BaseLandmark> choices = new List<BaseLandmark>();
            for (int i = 0; i < surrounding.Count; i++) {
                HexTile currTile = surrounding[i];
                if (currTile.landmarkOnTile != null) {
                    if (currTile.landmarkOnTile.owner == null || currTile.landmarkOnTile.owner.id != landmark.owner.id) {
                        //select a location within 8 tile distance around the camp owned by a different faction
                        choices.Add(currTile.landmarkOnTile);
                    }
                }
            }
            if (choices.Count > 0) {
                chosenLandmarkToRaid = choices[Random.Range(0, choices.Count)];
            }


            InteractionState startState = new InteractionState("State 1", this);
            string startStateDesc = "Our imp has discovered a new point of interest in " + landmark.landmarkName + ". We can send out a minion to investigate.";
            startState.SetDescription(startStateDesc);
            CreateActionOptions(startState);

        }
        //_states.Add(startState.name, startState);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        //if (state.name == "Start") {
        //    ActionOption sendOutDemonOption = new ActionOption {
        //        interactionState = state,
        //        cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
        //        description = "Send out a Demon.",
        //        duration = 15,
        //        needsMinion = true,
        //        effect = () => SendOutDemonEffect(state),
        //    };
        //    ActionOption leaveAloneOption = new ActionOption {
        //        interactionState = state,
        //        cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
        //        description = "Leave it alone.",
        //        duration = 1,
        //        needsMinion = false,
        //        effect = () => LeaveAloneEffect(state),
        //    };

        //    state.AddActionOption(sendOutDemonOption);
        //    state.AddActionOption(leaveAloneOption);
        //}
    }
    #endregion
}
