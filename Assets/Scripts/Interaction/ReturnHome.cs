using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnHome : Interaction {

    public ReturnHome(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.RETURN_HOME, 70) {
        _name = "Return Home";
        _jobFilter = new JOB[] { JOB.DISSUADER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(null, Utilities.NormalizeString(characterInvolved.race.ToString()), LOG_IDENTIFIER.STRING_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        InteractionState cancelledState = new InteractionState("Cancelled", this);
        InteractionState continuesState = new InteractionState("Continues", this);
        InteractionState doNothingState = new InteractionState("Do Nothing", this);

        CreateActionOptions(startState);

        //startState.SetEffect(() => StartEffect(startState), false);
        cancelledState.SetEffect(() => CancelledEffect(cancelledState));
        continuesState.SetEffect(() => ContinuesEffect(continuesState));
        doNothingState.SetEffect(() => DoNothingRewardEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(cancelledState.name, cancelledState);
        _states.Add(continuesState.name, continuesState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption preventOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from leaving.",
                duration = 0,
                jobNeeded = JOB.DISSUADER,
                doesNotMeetRequirementsStr = "Minion must be Dissuader.",
                effect = () => PreventOption(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(state),
            };

            state.AddActionOption(preventOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    private void PreventOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Cancelled", 20);
        effectWeights.AddElement("Continues", 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption(InteractionState state) {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement("Do Nothing", 15);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states["Do Nothing"]);
    }

    #region State Effects
    //private void StartEffect(InteractionState state) {
    //    if (!characterInvolved.characterIntel.isObtained) {
    //        PlayerManager.Instance.player.AddIntel(characterInvolved.characterIntel);
    //    }
    //}
    private void CancelledEffect(InteractionState state) {
        explorerMinion.LevelUp();
    }
    private void ContinuesEffect(InteractionState state) {
        characterInvolved.currentParty.GoHome();
    }
    private void DoNothingRewardEffect(InteractionState state) {
        ContinuesEffect(state);
    }
    #endregion
}
