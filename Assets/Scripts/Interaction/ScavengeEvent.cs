using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengeEvent : Interaction {

    private const string Trapped_Scavenge_Fail = "Trapped Scavenge Fail";
    private const string Trapped_Scavenge_Success = "Trapped Scavenge Success";
    private const string Trapped_Scavenge_Critical_Fail = "Trapped Scavenge Critical Fail";
    private const string Assisted_Scavenge_Success = "Assisted Scavenge Success";
    private const string Assisted_Scavenge_Fail = "Assisted Scavenge Fail";
    private const string Assisted_Scavenge_Critically_Fail = "Assisted Scavenge Critically Fail";
    private const string Normal_Scavenge_Success = "Normal Scavenge Success";
    private const string Normal_Scavenge_Fail = "Normal Scavenge Fail";
    private const string Normal_Scavenge_Critically_Fail = "Normal Scavenge Critically Fail";

    public ScavengeEvent(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.SCAVENGE_EVENT, 70) {
        _name = "Scavenge Event";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState scavengeTrapFailed = new InteractionState(Trapped_Scavenge_Fail, this);
        InteractionState scavengeTrapped = new InteractionState(Trapped_Scavenge_Success, this);
        InteractionState scavengeCriticallyTrapped = new InteractionState(Trapped_Scavenge_Critical_Fail, this);
        InteractionState scavengeBuffed = new InteractionState(Assisted_Scavenge_Success, this);
        InteractionState scavengeBuffFailed = new InteractionState(Assisted_Scavenge_Fail, this);
        InteractionState scavengeBuffCriticallyFailed = new InteractionState(Assisted_Scavenge_Critically_Fail, this);
        InteractionState normalScavengeSuccess = new InteractionState(Normal_Scavenge_Success, this);
        InteractionState normalScavengeFail = new InteractionState(Normal_Scavenge_Fail, this);
        InteractionState normalScavengeCriticalFail = new InteractionState(Normal_Scavenge_Critically_Fail, this);

        ////targetArea = GetTargetArea();
        ////**Text Description**: [Character Name] is about to leave for [Location Name 1] to scavenge for supplies.
        //Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        //startStateDescriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        //startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        scavengeTrapFailed.SetEffect(() => TrappedScavengeFailRewardEffect(scavengeTrapFailed));
        scavengeTrapped.SetEffect(() => TrappedScavengeSuccessRewardEffect(scavengeTrapped));
        scavengeCriticallyTrapped.SetEffect(() => TrappedScavengeCriticalFailRewardEffect(scavengeCriticallyTrapped));
        scavengeBuffed.SetEffect(() => AssistedScavengeSuccessRewardEffect(scavengeBuffed));
        scavengeBuffFailed.SetEffect(() => AssistedScavengeFailRewardEffect(scavengeBuffFailed));
        scavengeBuffCriticallyFailed.SetEffect(() => AssistedScavengeCriticallyFailRewardEffect(scavengeBuffCriticallyFailed));
        normalScavengeSuccess.SetEffect(() => NormalScavengeSuccessRewardEffect(normalScavengeSuccess));
        normalScavengeFail.SetEffect(() => NormalScavengeFailRewardEffect(normalScavengeFail));
        normalScavengeCriticalFail.SetEffect(() => NormalScavengeCriticallyFailRewardEffect(normalScavengeCriticalFail));

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
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Lay some traps.",
                duration = 0,
                effect = () => LayTrapOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Must have instigator minion."
            };
            ActionOption assist = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Assist with the scavenging.",
                duration = 0,
                effect = () => AssistOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Must have diplomat minion."
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(stopThem);
            state.AddActionOption(assist);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    #region Action Option Effect
    private void LayTrapOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> scavengerResultWeights = _characterInvolved.job.GetJobRateWeights();
        if (this.isChosen) {
            //Compute Scavenger success rate (Minion Instigator Success = +30 to Fail Rate, +20 to Critical Fail Rate)
            WeightedDictionary<RESULT> minionResultWeights = investigatorCharacter.job.GetJobRateWeights();
            if (minionResultWeights.PickRandomElementGivenWeights() == RESULT.SUCCESS) {
                scavengerResultWeights.AddWeightToElement(RESULT.FAIL, 30);
            } else {
                scavengerResultWeights.AddWeightToElement(RESULT.CRITICAL_FAIL, 20);
            }
        }
        string nextState = string.Empty;
        switch (scavengerResultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Trapped_Scavenge_Fail;
                break;
            case RESULT.FAIL:
                nextState = Trapped_Scavenge_Success;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Trapped_Scavenge_Critical_Fail;
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
                nextState = Assisted_Scavenge_Success;
                break;
            case RESULT.FAIL:
                nextState = Assisted_Scavenge_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Assisted_Scavenge_Critically_Fail;
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
                nextState = Normal_Scavenge_Critically_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    private void TrappedScavengeSuccessRewardEffect(InteractionState state) {
        _characterInvolved.LevelUp();
        int obtainedSupply = _characterInvolved.job.GetSupplyObtained(interactable.tileLocation.areaOfTile);
        _characterInvolved.homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(obtainedSupply);
        interactable.tileLocation.areaOfTile.AdjustSuppliesInBank(-obtainedSupply);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void TrappedScavengeFailRewardEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
    }
    private void TrappedScavengeCriticalFailRewardEffect(InteractionState state) {
        //**Mechanics**: Scavenger dies.
        investigatorCharacter.LevelUp();
        _characterInvolved.Death();
    }
    private void AssistedScavengeSuccessRewardEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
        _characterInvolved.LevelUp();

        int obtainedSupply = (_characterInvolved.job as Raider).GetSupplyObtained(interactable.tileLocation.areaOfTile);
        _characterInvolved.homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(obtainedSupply);
        interactable.tileLocation.areaOfTile.AdjustSuppliesInBank(-obtainedSupply);

        _characterInvolved.faction.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, 1);
        
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, 
            Utilities.NormalizeString(_characterInvolved.faction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).relationshipStatus.ToString()), LOG_IDENTIFIER.STRING_2));
    }
    private void AssistedScavengeFailRewardEffect(InteractionState state) {
        _investigatorMinion.LevelUp();
    }
    private void AssistedScavengeCriticallyFailRewardEffect(InteractionState state) {
        _characterInvolved.Death(); 
        investigatorCharacter.LevelUp();
    }
    private void NormalScavengeSuccessRewardEffect(InteractionState state) {
        //**Mechanics**: Compute Supply obtained by scavenger and transfer it to his home area. Scavenger also travels back to his home area.
        _characterInvolved.LevelUp();

        int obtainedSupply = _characterInvolved.job.GetSupplyObtained(interactable.tileLocation.areaOfTile);
        _characterInvolved.homeLandmark.tileLocation.areaOfTile.AdjustSuppliesInBank(obtainedSupply);
        interactable.tileLocation.areaOfTile.AdjustSuppliesInBank(-obtainedSupply);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void NormalScavengeFailRewardEffect(InteractionState state) {
        
    }
    private void NormalScavengeCriticallyFailRewardEffect(InteractionState state) {
        //**Mechanics**: Scavenger dies.
        _characterInvolved.Death();
    }

    //private void GoBackHome(System.Action doneAction = null) {
    //    _characterInvolved.ownParty.GoHome(doneAction);
    //}
}
