using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkEvent : Interaction {

    private const string Stop_Work_Successful = "Stop Work Successful";
    private const string Stop_Work_Fail = "Stop Work Fail";
    private const string Steal_Supply_Success = "Steal Supply Success";
    private const string Steal_Supply_Fail = "Steal Supply Fail";
    private const string Normal_Work = "Normal Work";

    public WorkEvent(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.WORK_EVENT, 0) {
        _name = "Work Event";
        _jobFilter = new JOB[] { JOB.DISSUADER, JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState stopWorkSuccessful = new InteractionState(Stop_Work_Successful, this);
        InteractionState stopWorkFail = new InteractionState(Stop_Work_Fail, this);
        InteractionState stealSupplySuccess = new InteractionState(Steal_Supply_Success, this);
        InteractionState stealSupplyFail = new InteractionState(Steal_Supply_Fail, this);
        InteractionState normalWork = new InteractionState(Normal_Work, this);

        //Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        //startStateDescriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2);
        //startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        stopWorkSuccessful.SetEffect(() => StopWorkSuccessfulRewardEffect(stopWorkSuccessful));
        stopWorkFail.SetEffect(() => StopWorkFailRewardEffect(stopWorkFail));
        stealSupplySuccess.SetEffect(() => StealSupplySuccessRewardEffect(stealSupplySuccess));
        stealSupplyFail.SetEffect(() => StealSupplyFailRewardEffect(stealSupplyFail));
        normalWork.SetEffect(() => NormalWorkRewardEffect(normalWork));

        _states.Add(startState.name, startState);
        _states.Add(stopWorkSuccessful.name, stopWorkSuccessful);
        _states.Add(stopWorkFail.name, stopWorkFail);
        _states.Add(stealSupplySuccess.name, stealSupplySuccess);
        _states.Add(stealSupplyFail.name, stealSupplyFail);
        _states.Add(normalWork.name, normalWork);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stop = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Stop " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from delivering the Supplies.",
                duration = 0,
                effect = () => StopOptionEffect(state),
                jobNeeded = JOB.DISSUADER,
                doesNotMeetRequirementsStr = "Minion must be a dissuader",
            };
            ActionOption steal = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Steal the Supplies.",
                duration = 0,
                effect = () => StealOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Minion must be an instigator",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(stop);
            state.AddActionOption(steal);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    #region Option Effects
    private void StopOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = explorerMinion.character.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Stop_Work_Successful;
                break;
            case RESULT.FAIL:
                nextState = Stop_Work_Fail;
                break;
            default:
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void StealOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = explorerMinion.character.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Steal_Supply_Success;
                break;
            case RESULT.FAIL:
                nextState = Steal_Supply_Fail;
                break;
            default:
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Normal_Work]);
    }
    #endregion

    #region Reward Effects
    private void StopWorkSuccessfulRewardEffect(InteractionState state) {
        //**Level Up**: Dissuader Minion +1
        explorerMinion.LevelUp();

        int obtainedSupplies = interactable.tileLocation.areaOfTile.workSupplyProduction;
        state.AddLogFiller(new LogFiller(null, obtainedSupplies.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void StopWorkFailRewardEffect(InteractionState state) {
        //**Mechanics**: Produce amount based on Location Supply Production and add to Area
        int obtainedSupplies = interactable.tileLocation.areaOfTile.workSupplyProduction;
        interactable.tileLocation.areaOfTile.AdjustSuppliesInBank(obtainedSupplies);
        //**Level Up**: Worker Character +1
        _characterInvolved.LevelUp();
        state.AddLogFiller(new LogFiller(null, obtainedSupplies.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void StealSupplySuccessRewardEffect(InteractionState state) {
        //**Mechanics**: Produce amount based on Location Supply Production and add to player
        int obtainedSupplies = interactable.tileLocation.areaOfTile.workSupplyProduction;
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, obtainedSupplies);
        //**Level Up**: Instigator Minion +1
        explorerMinion.LevelUp();
        //**Mechanics**: Favor Count -2
        _characterInvolved.faction.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, -2);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupplies.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void StealSupplyFailRewardEffect(InteractionState state) {
        //**Mechanics**: Produce amount based on Location Supply Production and add to Area
        int obtainedSupplies = interactable.tileLocation.areaOfTile.workSupplyProduction;
        interactable.tileLocation.areaOfTile.AdjustSuppliesInBank(obtainedSupplies);
        //**Level Up**: Worker Character +1
        _characterInvolved.LevelUp();
        //**Mechanics**: Favor Count -1
        _characterInvolved.faction.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, -1);

        state.AddLogFiller(new LogFiller(null, obtainedSupplies.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void NormalWorkRewardEffect(InteractionState state) {
        //**Mechanics**: Produce amount based on Location Supply Production and add to Area
        int obtainedSupplies = interactable.tileLocation.areaOfTile.workSupplyProduction;
        interactable.tileLocation.areaOfTile.AdjustSuppliesInBank(obtainedSupplies);
        //**Level Up**: Worker Character +1
        _characterInvolved.LevelUp();

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupplies.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupplies.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    #endregion
}
