using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaidEvent : Interaction {

    private const string Alerted_Raid_Success = "Alerted Raid Success";
    private const string Alerted_Raid_Fail = "Alerted Raid Fail";
    private const string Alerted_Raid_Critically_Fail = "Alerted Raid Critically Fail";
    private const string Assisted_Raid_Success = "Assisted Raid Success";
    private const string Assisted_Raid_Failed = "Assisted Raid Failed";
    private const string Assisted_Raid_Critically_Failed = "Assisted Raid Critically Failed";
    private const string Normal_Raid_Success = "Normal Raid Success";
    private const string Normal_Raid_Fail = "Normal Raid Fail";
    private const string Normal_Raid_Critical_Fail = "Normal Raid Critical Fail";

    public RaidEvent(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.RAID_EVENT, 0) {
        _name = "Raid Event";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState alertedRaidSuccess = new InteractionState(Alerted_Raid_Success, this);
        InteractionState alertedRaidFail = new InteractionState(Alerted_Raid_Fail, this);
        InteractionState alertedRaidCriticallyFail = new InteractionState(Alerted_Raid_Critically_Fail, this);
        InteractionState assistedRaidSuccess = new InteractionState(Assisted_Raid_Success, this);
        InteractionState assistedRaidFailed = new InteractionState(Assisted_Raid_Failed, this);
        InteractionState assistedRaidCriticallyFailed = new InteractionState(Assisted_Raid_Critically_Failed, this);
        InteractionState normalRaidSuccess = new InteractionState(Normal_Raid_Success, this);
        InteractionState normalRaidFail = new InteractionState(Normal_Raid_Fail, this);
        InteractionState normalRaidCriticalFail = new InteractionState(Normal_Raid_Critical_Fail, this);

        CreateActionOptions(startState);
        alertedRaidSuccess.SetEffect(() => AlertedRaidSuccessRewardEffect(alertedRaidSuccess));
        alertedRaidFail.SetEffect(() => AlertedRaidFailRewardEffect(alertedRaidFail));
        alertedRaidCriticallyFail.SetEffect(() => AlertedRaidCriticallyFailRewardEffect(alertedRaidCriticallyFail));
        assistedRaidSuccess.SetEffect(() => AssistedRaidSuccessRewardEffect(assistedRaidSuccess));
        assistedRaidFailed.SetEffect(() => AssistedRaidFailedRewardEffect(assistedRaidFailed));
        assistedRaidCriticallyFailed.SetEffect(() => AssistedRaidCriticallyFailedRewardEffect(assistedRaidCriticallyFailed));
        normalRaidSuccess.SetEffect(() => NormalRaidSuccessRewardEffect(normalRaidSuccess));
        normalRaidFail.SetEffect(() => NormalRaidFailRewardEffect(normalRaidFail));
        normalRaidCriticalFail.SetEffect(() => NormalRaidCriticalFailRewardEffect(normalRaidCriticalFail));

        _states.Add(startState.name, startState);
        _states.Add(alertedRaidSuccess.name, alertedRaidSuccess);
        _states.Add(alertedRaidFail.name, alertedRaidFail);
        _states.Add(alertedRaidCriticallyFail.name, alertedRaidCriticallyFail);
        _states.Add(assistedRaidSuccess.name, assistedRaidSuccess);
        _states.Add(assistedRaidFailed.name, assistedRaidFailed);
        _states.Add(assistedRaidCriticallyFailed.name, assistedRaidCriticallyFailed);
        _states.Add(normalRaidSuccess.name, normalRaidSuccess);
        _states.Add(normalRaidFail.name, normalRaidFail);
        _states.Add(normalRaidCriticalFail.name, normalRaidCriticalFail);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption alert = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Alert " + interactable.name + ".",
                duration = 0,
                effect = () => AlertOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Minion must be a instigator",
            };
            ActionOption assist = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.MANA },
                name = "Assist with the raid.",
                duration = 0,
                effect = () => AssistOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Minion must be a diplomat",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(alert);
            state.AddActionOption(assist);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    #region Action Option Effect
    private void AlertOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        if (this.isChosen) {
            resultWeights.AddWeightToElement(RESULT.FAIL, 30);
            resultWeights.AddWeightToElement(RESULT.CRITICAL_FAIL, 20);
        }
        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Alerted_Raid_Success;
                break;
            case RESULT.FAIL:
                nextState = Alerted_Raid_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Alerted_Raid_Critically_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void AssistOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.AddWeightToElement(RESULT.SUCCESS, 30);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Assisted_Raid_Success;
                break;
            case RESULT.FAIL:
                nextState = Assisted_Raid_Failed;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Assisted_Raid_Critically_Failed;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Normal_Raid_Success;
                break;
            case RESULT.FAIL:
                nextState = Normal_Raid_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Normal_Raid_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    private void AlertedRaidSuccessRewardEffect(InteractionState state) {
        //**Mechanics**: Compute Supply obtained by raider and transfer it to his home area. Raider also travels back to his home area.
        //**Level Up**: Instigator Minion +1, Minion Character +1
        _characterInvolved.LevelUp(); //Minion Character
        _explorerMinion.character.LevelUp(); //Instigator
        //Player Favor Count +1 on Raided Faction
        interactable.owner.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 1);
        int obtainedSupply = _characterInvolved.job.GetSupplyObtained(interactable.tileLocation.areaOfTile);
        GoBackHome(() => TransferSupplies(obtainedSupply, _characterInvolved.homeLandmark.tileLocation.areaOfTile,
            interactable.tileLocation.areaOfTile));
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void AlertedRaidFailRewardEffect(InteractionState state) {
        //**Mechanics**: Scavenger travels back to his home area.
        explorerMinion.LevelUp(); //**Level Up**: Instigator Minion +1
        GoBackHome();
        //Player Favor Count +2 on Raided Faction
        interactable.owner.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 2);
    }
    private void AlertedRaidCriticallyFailRewardEffect(InteractionState state) {
        //**Mechanics**: Scavenger dies.
        explorerMinion.LevelUp(); //**Level Up**: Instigator Minion +1
        _characterInvolved.Death();

        //Player Favor Count +2 on Raided Faction
        interactable.owner.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 2);
    }
    private void AssistedRaidSuccessRewardEffect(InteractionState state) {
        //**Mechanics**: Compute Supply obtained by scavenger and transfer it to his home area. Scavenger also travels back to his home area.
        //**Level Up**: Diplomat Minion +1, Raider Character +1
        explorerMinion.LevelUp();
        _characterInvolved.LevelUp();

        int obtainedSupply = _characterInvolved.job.GetSupplyObtained(interactable.tileLocation.areaOfTile);
        GoBackHome(() => TransferSupplies(obtainedSupply, _characterInvolved.homeLandmark.tileLocation.areaOfTile,
            interactable.tileLocation.areaOfTile));
        //Player Favor Count +2 from Raider Faction
        _characterInvolved.faction.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 2);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void AssistedRaidFailedRewardEffect(InteractionState state) {
        //**Mechanics**: Raider travels back to his home area.
        GoBackHome();
        //Player Favor Count +1 from Raider Faction
        _characterInvolved.faction.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 1);
    }
    private void AssistedRaidCriticallyFailedRewardEffect(InteractionState state) {
        //**Mechanics**: Raider dies.
        _characterInvolved.Death();
        //Player Favor Count +2 from Raider Faction
        _characterInvolved.faction.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 2);
    }
    private void NormalRaidSuccessRewardEffect(InteractionState state) {
        //**Level Up**: Raider Character +1
        _characterInvolved.LevelUp();

        //**Mechanics**: Compute Supply obtained by Raider and transfer it to his home area. Raider also travels back to his home area.
        int obtainedSupply = _characterInvolved.job.GetSupplyObtained(interactable.tileLocation.areaOfTile);
        GoBackHome(() => TransferSupplies(obtainedSupply, _characterInvolved.homeLandmark.tileLocation.areaOfTile,
            interactable.tileLocation.areaOfTile));

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void NormalRaidFailRewardEffect(InteractionState state) {
        //**Mechanics**: Raider travels back to his home area.
        GoBackHome();
    }
    private void NormalRaidCriticalFailRewardEffect(InteractionState state) {
        //**Mechanics**: Raider dies.
        _characterInvolved.Death();
    }

    private void GoBackHome(System.Action doneAction = null) {
        _characterInvolved.ownParty.GoHome(doneAction);
    }
    private void TransferSupplies(int amount, Area recieving, Area source) {
        recieving.AdjustSuppliesInBank(amount);
        source.AdjustSuppliesInBank(-amount);
    }
}
