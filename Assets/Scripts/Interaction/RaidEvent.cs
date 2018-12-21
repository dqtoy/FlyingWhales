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
    private const string Normal_Raid_Critical_Fail = "Normal Raid Critically Fail";

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
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Alert " + interactable.tileLocation.areaOfTile.name + ".",
                duration = 0,
                effect = () => AlertOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Minion must be a diplomat",
            };
            ActionOption assist = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Assist with the raid.",
                duration = 0,
                effect = () => AssistOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Minion must be an instigator",
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
        //**Mechanics**: Compute Supply obtained by raider and transfer it to his home area. Raider Faction and Raided Faction -1 Relationship.
        AdjustFactionsRelationship(_characterInvolved.faction, interactable.tileLocation.areaOfTile.owner, -1, state);
        //**Level Up**: Raider Character +1
        _characterInvolved.LevelUp(); //Raider Character
        int obtainedSupply = _characterInvolved.job.GetSupplyObtained(interactable.tileLocation.areaOfTile);
        TransferSupplies(obtainedSupply, _characterInvolved.homeLandmark.tileLocation.areaOfTile,
            interactable.tileLocation.areaOfTile);
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void AlertedRaidFailRewardEffect(InteractionState state) {
        investigatorMinion.LevelUp(); //**Level Up**: Diplomat Minion +1
        //**Mechanics**: Player Relationship +1 on Raided Faction
        interactable.owner.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, 1);
        state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(interactable.owner
            .GetRelationshipWith(PlayerManager.Instance.player.playerFaction).relationshipStatus.ToString()), LOG_IDENTIFIER.STRING_1));
    }
    private void AlertedRaidCriticallyFailRewardEffect(InteractionState state) {
        //**Mechanics**: Raider dies. Player Relationship +1 on Raided Faction
        investigatorMinion.LevelUp(); //**Level Up**: Diplomat Minion +1
        _characterInvolved.Death();

        //**Mechanics**: Raider dies. Player Relationship +1 on Raided Faction
        interactable.owner.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, 1);
        state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(interactable.owner.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).relationshipStatus.ToString()), LOG_IDENTIFIER.STRING_1));
    }
    private void AssistedRaidSuccessRewardEffect(InteractionState state) {
        //**Mechanics**: Compute Supply obtained by raider and transfer it to his home area. Player Relationship +1 on Raider Faction
        //**Level Up**: Raider +1, Instigator Minion +1
        investigatorMinion.LevelUp();
        _characterInvolved.LevelUp();

        int obtainedSupply = _characterInvolved.job.GetSupplyObtained(interactable.tileLocation.areaOfTile);
        TransferSupplies(obtainedSupply, _characterInvolved.homeLandmark.tileLocation.areaOfTile,
            interactable.tileLocation.areaOfTile);
        _characterInvolved.faction.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, 1);
        
        //Raider Faction and Raided Faction -1 Relationship.
        AdjustFactionsRelationship(_characterInvolved.faction, interactable.tileLocation.areaOfTile.owner, -1, state);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(_characterInvolved.faction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).relationshipStatus.ToString()), LOG_IDENTIFIER.STRING_2));
    }
    private void AssistedRaidFailedRewardEffect(InteractionState state) {
        //**Level Up**: Diplomat Minion +1 (if alerted)
        investigatorMinion.LevelUp();
        _characterInvolved.faction.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, 1);

        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(_characterInvolved.faction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).relationshipStatus.ToString()), LOG_IDENTIFIER.STRING_2));
    }
    private void AssistedRaidCriticallyFailedRewardEffect(InteractionState state) {
        //**Mechanics**: Raider dies.
        _characterInvolved.Death();
        investigatorMinion.LevelUp();
    }
    private void NormalRaidSuccessRewardEffect(InteractionState state) {
        //**Level Up**: Raider Character +1
        _characterInvolved.LevelUp();

        //**Mechanics**: Compute Supply obtained by Raider and transfer it to his home area. Raider also travels back to his home area.
        int obtainedSupply = _characterInvolved.job.GetSupplyObtained(interactable.tileLocation.areaOfTile);
        TransferSupplies(obtainedSupply, _characterInvolved.homeLandmark.tileLocation.areaOfTile,
            interactable.tileLocation.areaOfTile);

        //Raider Faction and Raided Faction -1 Relationship.
        AdjustFactionsRelationship(_characterInvolved.faction, interactable.tileLocation.areaOfTile.owner, -1, state);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void NormalRaidFailRewardEffect(InteractionState state) {
        
    }
    private void NormalRaidCriticalFailRewardEffect(InteractionState state) {
        //**Mechanics**: Raider dies.
        _characterInvolved.Death();
    }

    //private void GoBackHome(System.Action doneAction = null) {
    //    _characterInvolved.ownParty.GoHome(doneAction);
    //}
    private void TransferSupplies(int amount, Area recieving, Area source) {
        recieving.AdjustSuppliesInBank(amount);
        source.AdjustSuppliesInBank(-amount);
    }
}
