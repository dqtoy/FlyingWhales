using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflictIllness : Interaction {

    private const string Induce_Illness_Successful = "Induce Illness Successful";
    private const string Induce_Illness_Fail = "Induce Illness Fail";
    private const string Induce_Illness_Critical_Fail = "Induce Illness Critical Fail";
    private const string Do_Nothing = "Do nothing";

    public InflictIllness(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.INFLICT_ILLNESS, 0) {
        _name = "Inflict Illness";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState induceIllnessSuccessful = new InteractionState(Induce_Illness_Successful, this);
        InteractionState induceIllnessFail = new InteractionState(Induce_Illness_Fail, this);
        InteractionState induceIllnessCriticalFail = new InteractionState(Induce_Illness_Critical_Fail, this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        ////**Text Description**: [Minion Name] successfully raided [Location Name 1]. [He/She] returns with [Amount] Supplies.
        //Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        //startStateDescriptionLog.AddToFillers(null, otherData[0].ToString(), LOG_IDENTIFIER.STRING_1);
        //startState.OverrideDescriptionLog(startStateDescriptionLog);

        //startState.AddLogFiller(new LogFiller(null, otherData[0].ToString(), LOG_IDENTIFIER.STRING_1));

        //startState.SetEffect(() => RaidSuccessEffect(startState));

        induceIllnessSuccessful.SetEffect(() => InduceIllnessSuccessRewardEffect(induceIllnessSuccessful));
        induceIllnessFail.SetEffect(() => InduceIllnessFailRewardEffect(induceIllnessFail));
        induceIllnessCriticalFail.SetEffect(() => InduceIllnessCriticalFailRewardEffect(induceIllnessCriticalFail));
        doNothing.SetEffect(() => DoNothingRewardEffect(doNothing));

        _states.Add(startState.name, startState);
        _states.Add(induceIllnessSuccessful.name, induceIllnessSuccessful);
        _states.Add(induceIllnessFail.name, induceIllnessFail);
        _states.Add(induceIllnessCriticalFail.name, induceIllnessCriticalFail);
        _states.Add(doNothing.name, doNothing);
        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopThem = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Use it now.",
                duration = 0,
                effect = () => InduceIllnessOptionEffect(state),
                neededObjects = new List<System.Type>() { typeof(CharacterIntel) },
                //jobNeeded = JOB.DISSUADER,
                //doesNotMeetRequirementsStr = "Minion must be a dissuader",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "We don't need such underhanded tactics.",
                duration = 0,
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(stopThem);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    #region Action Option Effects
    private void InduceIllnessOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Induce_Illness_Successful;
                break;
            case RESULT.FAIL:
                nextState = Induce_Illness_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Induce_Illness_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region Reward Effects
    private void InduceIllnessSuccessRewardEffect(InteractionState state) {
        //**Mechanics**: Choose a random Illness Trait and add it to Character Intel
        //**Level Up**: Instigator Minion +1
        _characterInvolved.LevelUp();
    }
    private void InduceIllnessFailRewardEffect(InteractionState state) {

    }
    private void InduceIllnessCriticalFailRewardEffect(InteractionState state) {
        //**Mechanics**: Choose a random Illness Trait and add it to Instigator Minion
    }
    private void DoNothingRewardEffect(InteractionState state) {

    }
    #endregion
}
