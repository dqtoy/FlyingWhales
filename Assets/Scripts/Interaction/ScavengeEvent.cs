using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengeEvent : Interaction {

    private const string Scavenge_Trap_Failed = "Scavenge Trap Failed";
    private const string Scavenge_Trapped = "Scavenge Trapped";
    private const string Scavenge_Critically_Trapped = "Scavenge Critically Trapped";
    private const string Scavenge_Buffed = "Scavenge Buffed";
    private const string Scavenge_Buff_Failed = "Scavenge Buff Failed";
    private const string Scavenge_Buff_Critically_Failed = "Scavenge Buff Critically Failed";
    private const string Normal_Scavenge_Success = "Normal Scavenge Success";
    private const string Normal_Scavenge_Fail = "Normal Scavenge Fail";
    private const string Normal_Scavenge_Critical_Fail = "Normal Scavenge Critical Fail";

    public ScavengeEvent(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.SCAVENGE_EVENT, 70) {
        _name = "Scavenge Event";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState scavengeTrapFailed = new InteractionState(Scavenge_Trap_Failed, this);
        InteractionState scavengeTrapped = new InteractionState(Scavenge_Trapped, this);
        InteractionState scavengeCriticallyTrapped = new InteractionState(Scavenge_Critically_Trapped, this);
        InteractionState scavengeBuffed = new InteractionState(Scavenge_Buffed, this);
        InteractionState scavengeBuffFailed = new InteractionState(Scavenge_Buff_Failed, this);
        InteractionState scavengeBuffCriticallyFailed = new InteractionState(Scavenge_Buff_Critically_Failed, this);
        InteractionState normalScavengeSuccess = new InteractionState(Normal_Scavenge_Success, this);
        InteractionState normalScavengeFail = new InteractionState(Normal_Scavenge_Fail, this);
        InteractionState normalScavengeCriticalFail = new InteractionState(Normal_Scavenge_Critical_Fail, this);

        ////targetArea = GetTargetArea();
        ////**Text Description**: [Character Name] is about to leave for [Location Name 1] to scavenge for supplies.
        //Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        //startStateDescriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        //startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        scavengeTrapFailed.SetEffect(() => ScavengeTrapFailedRewardEffect(scavengeTrapFailed));
        scavengeTrapped.SetEffect(() => ScavengeTrappedRewardEffect(scavengeTrapped));
        scavengeCriticallyTrapped.SetEffect(() => ScavengeCriticallyTrappedRewardEffect(scavengeCriticallyTrapped));
        scavengeBuffed.SetEffect(() => ScavengeBuffedRewardEffect(scavengeBuffed));
        scavengeBuffFailed.SetEffect(() => ScavengeBuffFailedRewardEffect(scavengeBuffFailed));
        scavengeBuffCriticallyFailed.SetEffect(() => ScavengeBuffCriticallyFailedRewardEffect(scavengeBuffCriticallyFailed));
        normalScavengeSuccess.SetEffect(() => NormalScavengeSuccessRewardEffect(normalScavengeSuccess));
        normalScavengeFail.SetEffect(() => NormalScavengeFailRewardEffect(normalScavengeFail));
        normalScavengeCriticalFail.SetEffect(() => NormalScavengeCriticalFailRewardEffect(normalScavengeCriticalFail));

        _states.Add(startState.name, startState);
        _states.Add(scavengeTrapFailed.name, scavengeTrapFailed);
        _states.Add(scavengeTrapped.name, scavengeTrapped);
        _states.Add(scavengeCriticallyTrapped.name, scavengeCriticallyTrapped);
        _states.Add(scavengeBuffed.name, scavengeBuffed);
        _states.Add(scavengeBuffFailed.name, scavengeBuffFailed);
        _states.Add(scavengeBuffCriticallyFailed.name, scavengeBuffCriticallyFailed);
        _states.Add(normalScavengeSuccess.name, normalScavengeSuccess);
        _states.Add(normalScavengeFail.name, normalScavengeFail);
        _states.Add(normalScavengeCriticalFail.name, normalScavengeCriticalFail);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopThem = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Lay some traps.",
                duration = 0,
                needsMinion = false,
                effect = () => LayTrapOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Minion must be a instigator",
            };
            ActionOption assist = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.MANA },
                name = "Assist with the scavenging.",
                duration = 0,
                needsMinion = false,
                effect = () => AssistOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Minion must be a diplomat",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(stopThem);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    #region Action Option Effect
    private void LayTrapOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> scavengerResultWeights = _characterInvolved.job.GetJobRateWeights();
        if (this.isChosen) {
            //Compute Scavenger success rate (Minion Instigator Success = +30 to Fail Rate, +2 to Critical Fail Rate)
            WeightedDictionary<RESULT> minionResultWeights = explorerMinion.character.job.GetJobRateWeights();
            if (minionResultWeights.PickRandomElementGivenWeights() == RESULT.SUCCESS) {
                scavengerResultWeights.AddWeightToElement(RESULT.FAIL, 30);
            } else {
                scavengerResultWeights.AddWeightToElement(RESULT.CRITICAL_FAIL, 2);
            }
        }
        string nextState = string.Empty;
        switch (scavengerResultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Scavenge_Trap_Failed;
                break;
            case RESULT.FAIL:
                nextState = Scavenge_Trapped;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Scavenge_Critically_Trapped;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void AssistOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> scavengerResultWeights = _characterInvolved.job.GetJobRateWeights();
        scavengerResultWeights.AddWeightToElement(RESULT.SUCCESS, 30);

        string nextState = string.Empty;
        switch (scavengerResultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Scavenge_Buffed;
                break;
            case RESULT.FAIL:
                nextState = Scavenge_Buff_Failed;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Scavenge_Buff_Critically_Failed;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        WeightedDictionary<RESULT> scavengerResultWeights = _characterInvolved.job.GetJobRateWeights();

        string nextState = string.Empty;
        switch (scavengerResultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Normal_Scavenge_Success;
                break;
            case RESULT.FAIL:
                nextState = Normal_Scavenge_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Normal_Scavenge_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    private void ScavengeTrapFailedRewardEffect(InteractionState state) {
        //**Mechanics**: Compute Supply obtained by scavenger and transfer it to his home area. Scavenger also travels back to his home area.
        _characterInvolved.LevelUp();
        int obtainedSupply = (_characterInvolved.job as Raider).GetSupplyObtained(interactable.tileLocation.areaOfTile);
        GoBackHome(() => _characterInvolved.homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(obtainedSupply));
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void ScavengeTrappedRewardEffect(InteractionState state) {
        //**Mechanics**: Scavenger travels back to his home area.
        explorerMinion.LevelUp();
        GoBackHome();
    }
    private void ScavengeCriticallyTrappedRewardEffect(InteractionState state) {
        //**Mechanics**: Scavenger dies.
        explorerMinion.LevelUp();
        _characterInvolved.Death();
    }
    private void ScavengeBuffedRewardEffect(InteractionState state) {
        //**Mechanics**: Compute Supply obtained by scavenger and transfer it to his home area. Scavenger also travels back to his home area.
        //+2 Favor from Faction 1
        explorerMinion.LevelUp();
        _characterInvolved.LevelUp();

        int obtainedSupply = (_characterInvolved.job as Raider).GetSupplyObtained(interactable.tileLocation.areaOfTile);
        GoBackHome(() => _characterInvolved.homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(obtainedSupply));
        _characterInvolved.faction.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 2);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void ScavengeBuffFailedRewardEffect(InteractionState state) {
        //**Mechanics**: Compute Supply obtained by scavenger and transfer it to his home area. Scavenger also travels back to his home area.
        //+2 Favor from Faction 1
        int obtainedSupply = (_characterInvolved.job as Raider).GetSupplyObtained(interactable.tileLocation.areaOfTile);
        GoBackHome(() => _characterInvolved.homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(obtainedSupply));
        _characterInvolved.faction.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 1);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void ScavengeBuffCriticallyFailedRewardEffect(InteractionState state) {
        //**Mechanics**: Compute Supply obtained by scavenger and transfer it to his home area. Scavenger also travels back to his home area.
        //+2 Favor from Faction 1
        explorerMinion.LevelUp();

        int obtainedSupply = (_characterInvolved.job as Raider).GetSupplyObtained(interactable.tileLocation.areaOfTile);
        GoBackHome(() => _characterInvolved.homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(obtainedSupply));
        _characterInvolved.faction.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 2);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void NormalScavengeSuccessRewardEffect(InteractionState state) {
        //**Mechanics**: Compute Supply obtained by scavenger and transfer it to his home area. Scavenger also travels back to his home area.
        _characterInvolved.LevelUp();

        int obtainedSupply = (_characterInvolved.job as Raider).GetSupplyObtained(interactable.tileLocation.areaOfTile);
        GoBackHome(() => _characterInvolved.homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(obtainedSupply));

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void NormalScavengeFailRewardEffect(InteractionState state) {
        //**Mechanics**: Scavenger travels back to his home area.
        GoBackHome();
    }
    private void NormalScavengeCriticalFailRewardEffect(InteractionState state) {
        //**Mechanics**: Scavenger dies.
        _characterInvolved.Death();
    }

    private void GoBackHome(System.Action doneAction = null) {
        _characterInvolved.ownParty.GoHome(doneAction);
    }
}
