using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpansionEvent : Interaction {

    private const string Expansionist_Killed_No_Witness = "Expansionist Killed No Witness";
    private const string Expansionist_Killed_Witnessed = "Expansionist Killed Witnessed";
    private const string Expansionist_Survives_Minion_Flees = "Expansionist Survives Minion Flees";
    private const string Expansionist_Survives_Minion_Dies = "Expansionist Survives Minion Dies";
    private const string Assisted_Expansion = "Assisted Expansion";
    private const string Normal_Expansion = "Normal Expansion";

    public ExpansionEvent(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.EXPANSION_EVENT, 0) {
        _name = "Expansion Event";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        //InteractionState scavengeCancelled = new InteractionState(Scavenge_Cancelled, this);
        //InteractionState scavengeProceeds = new InteractionState(Scavenge_Proceeds, this);
        //InteractionState normalScavenge = new InteractionState(Normal_Scavenge, this);

        //targetArea = GetTargetArea();
        //AddToDebugLog("Set target area to " + targetArea.name);
        ////**Text Description**: [Character Name] is about to leave for [Location Name 1] to scavenge for supplies.
        //Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        //startStateDescriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        //startState.OverrideDescriptionLog(startStateDescriptionLog);

        //CreateActionOptions(startState);
        //scavengeCancelled.SetEffect(() => ScavengeCancelledRewardEffect(scavengeCancelled));
        //scavengeProceeds.SetEffect(() => ScavengeProceedsRewardEffect(scavengeProceeds));
        //normalScavenge.SetEffect(() => NormalScavengeRewardEffect(normalScavenge));

        //_states.Add(startState.name, startState);
        //_states.Add(scavengeCancelled.name, scavengeCancelled);
        //_states.Add(scavengeProceeds.name, scavengeProceeds);
        //_states.Add(normalScavenge.name, normalScavenge);
        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopThem = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Pursuade " + _characterInvolved.name + " to cancel " + Utilities.GetPossessivePronounForCharacter(_characterInvolved, false) + " plans.",
                duration = 0,
                //effect = () => PursuadeToCancelEffect(state),
                jobNeeded = JOB.DISSUADER,
                doesNotMeetRequirementsStr = "Minion must be a dissuader",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                //effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(stopThem);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion
}
