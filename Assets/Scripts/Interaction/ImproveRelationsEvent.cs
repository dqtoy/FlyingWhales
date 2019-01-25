using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImproveRelationsEvent : Interaction {

    private Faction targetFaction;

    private const string Disrupted_Improve_Relations_Success = "Disrupted Improve Relations Success";
    private const string Disrupted_Improve_Relations_Fail = "Disrupted Improve Relations Fail";
    private const string Disrupted_Improve_Relations_Critically_Fail = "Disrupted Improve Relations Critically Fail";
    private const string Assisted_Improve_Relations_Success = "Assisted Improve Relations Success";
    private const string Assisted_Improve_Relations_Fail = "Assisted Improve Relations Fail";
    private const string Assisted_Improve_Relations_Critically_Fail = "Assisted Improve Relations Critically Fail";
    private const string Normal_Improve_Relations_Success = "Normal Improve Relations Success";
    private const string Normal_Improve_Relations_Fail = "Normal Improve Relations Fail";
    private const string Normal_Improve_Relations_Critically_Fail = "Normal Improve Relations Critically Fail";

    public ImproveRelationsEvent(Area interactable) 
        : base(interactable, INTERACTION_TYPE.IMPROVE_RELATIONS_EVENT, 0) {
        _name = "Improve Relations Event";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState disruptedImproveRelationsSuccess = new InteractionState(Disrupted_Improve_Relations_Success, this);
        InteractionState disruptedImproveRelationsFail = new InteractionState(Disrupted_Improve_Relations_Fail, this);
        InteractionState disruptedImproveRelationsCriticalFail = new InteractionState(Disrupted_Improve_Relations_Critically_Fail, this);
        InteractionState assistedImproveRelationsSuccess = new InteractionState(Assisted_Improve_Relations_Success, this);
        InteractionState assistedImproveRelationsFail = new InteractionState(Assisted_Improve_Relations_Fail, this);
        InteractionState assistedImproveRelationsCriticalFail = new InteractionState(Assisted_Improve_Relations_Critically_Fail, this);
        InteractionState normalImproveRelationsSuccess = new InteractionState(Normal_Improve_Relations_Success, this);
        InteractionState normalImproveRelationsFail = new InteractionState(Normal_Improve_Relations_Fail, this);
        InteractionState normalImproveRelationsCriticalFail = new InteractionState(Normal_Improve_Relations_Critically_Fail, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        startStateDescriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        disruptedImproveRelationsSuccess.SetEffect(() => DisruptedImproveRelationsSuccessRewardEffect(disruptedImproveRelationsSuccess));
        disruptedImproveRelationsFail.SetEffect(() => DisruptedImproveRelationsFailRewardEffect(disruptedImproveRelationsFail));
        disruptedImproveRelationsCriticalFail.SetEffect(() => DisruptedImproveRelationsCriticallyFailRewardEffect(disruptedImproveRelationsCriticalFail));
        assistedImproveRelationsSuccess.SetEffect(() => AssistedImproveRelationsSuccessRewardEffect(assistedImproveRelationsSuccess));
        assistedImproveRelationsFail.SetEffect(() => AssistedImproveRelationsFailRewardEffect(assistedImproveRelationsFail));
        assistedImproveRelationsCriticalFail.SetEffect(() => AssistedImproveRelationsCriticallyFailRewardEffect(assistedImproveRelationsFail));
        normalImproveRelationsSuccess.SetEffect(() => NormalImproveRelationsSuccessRewardEffect(normalImproveRelationsSuccess));
        normalImproveRelationsFail.SetEffect(() => NormalImproveRelationsFailRewardEffect(normalImproveRelationsFail));
        normalImproveRelationsCriticalFail.SetEffect(() => NormalImproveRelationsCriticallyFailRewardEffect(normalImproveRelationsCriticalFail));

        _states.Add(startState.name, startState);
        _states.Add(disruptedImproveRelationsSuccess.name, disruptedImproveRelationsSuccess);
        _states.Add(disruptedImproveRelationsFail.name, disruptedImproveRelationsFail);
        _states.Add(disruptedImproveRelationsCriticalFail.name, disruptedImproveRelationsCriticalFail);
        _states.Add(assistedImproveRelationsSuccess.name, assistedImproveRelationsSuccess);
        _states.Add(assistedImproveRelationsFail.name, assistedImproveRelationsFail);
        _states.Add(assistedImproveRelationsCriticalFail.name, assistedImproveRelationsCriticalFail);
        _states.Add(normalImproveRelationsSuccess.name, normalImproveRelationsSuccess);
        _states.Add(normalImproveRelationsFail.name, normalImproveRelationsFail);
        _states.Add(normalImproveRelationsCriticalFail.name, normalImproveRelationsCriticalFail);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption disrupt = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Disrupt the meeting.",
                effect = () => DisruptOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Must have instigator minion.",
            };
            ActionOption assist = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Assist with the meeting.",
                effect = () => AssistOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Must have diplomat minion.",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(disrupt);
            state.AddActionOption(assist);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (character.faction == null || targetFaction == null || character.faction.isDestroyed || targetFaction.isDestroyed) {
            return false;
        }
        if (character.faction.id == targetFaction.id) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void DisruptOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.AddWeightToElement(RESULT.FAIL, 30);
        resultWeights.AddWeightToElement(RESULT.CRITICAL_FAIL, 20);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Disrupted_Improve_Relations_Success;
                break;
            case RESULT.FAIL:
                nextState = Disrupted_Improve_Relations_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Disrupted_Improve_Relations_Critically_Fail;
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
                nextState = Assisted_Improve_Relations_Success;
                break;
            case RESULT.FAIL:
                nextState = Assisted_Improve_Relations_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Assisted_Improve_Relations_Critically_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Normal_Improve_Relations_Success;
                break;
            case RESULT.FAIL:
                nextState = Normal_Improve_Relations_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Normal_Improve_Relations_Critically_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void DisruptedImproveRelationsSuccessRewardEffect(InteractionState state) {
        //**Mechanics**: Relationship +1 on the two factions
        AdjustFactionsRelationship(_characterInvolved.faction, targetFaction, 1, state);
        //_characterInvolved.faction.AdjustRelationshipFor(targetFaction, 1);
        //**Level Up**: Diplomat Character +1, Diplomat Minion +1 (if assisted)
        _characterInvolved.LevelUp();
        investigatorCharacter.LevelUp();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
        }
        FactionRelationship rel = _characterInvolved.faction.GetRelationshipWith(targetFaction);
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
        //state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(rel.relationshipStatus.ToString()), LOG_IDENTIFIER.STRING_1));
    }
    private void DisruptedImproveRelationsFailRewardEffect(InteractionState state) {
        //**Level Up**: Diplomat Minion +1
        investigatorCharacter.LevelUp();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void DisruptedImproveRelationsCriticallyFailRewardEffect(InteractionState state) {
        //**Mechanics**: Relationship -1 on the two factions
        AdjustFactionsRelationship(_characterInvolved.faction, targetFaction, -1, state);
        //_characterInvolved.faction.AdjustRelationshipFor(targetFaction, -1);
        //**Level Up**: Diplomat Minion +1
        investigatorCharacter.LevelUp();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void AssistedImproveRelationsSuccessRewardEffect(InteractionState state) {
        //**Mechanics**: Relationship +1 on the two factions
        AdjustFactionsRelationship(_characterInvolved.faction, targetFaction, 1, state);
        //_characterInvolved.faction.AdjustRelationshipFor(targetFaction, 1);
        //**Level Up**: Diplomat Character +1, Diplomat Minion +1 (if assisted)
        _characterInvolved.LevelUp();
        investigatorCharacter.LevelUp();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
        }
        FactionRelationship rel = _characterInvolved.faction.GetRelationshipWith(targetFaction);
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
        //state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(rel.relationshipStatus.ToString()), LOG_IDENTIFIER.STRING_1));
    }
    private void AssistedImproveRelationsFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void AssistedImproveRelationsCriticallyFailRewardEffect(InteractionState state) {
        //**Mechanics**: Relationship -1 on the two factions
        AdjustFactionsRelationship(_characterInvolved.faction, targetFaction, -1, state);
        //_characterInvolved.faction.AdjustRelationshipFor(targetFaction, -1);
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void NormalImproveRelationsSuccessRewardEffect(InteractionState state) {
        //**Mechanics**: Relationship +1 on the two factions
        AdjustFactionsRelationship(_characterInvolved.faction, targetFaction, 1, state);
        //_characterInvolved.faction.AdjustRelationshipFor(targetFaction, 1);
        //**Level Up**: Diplomat Character +1
        _characterInvolved.LevelUp();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
        }
        FactionRelationship rel = _characterInvolved.faction.GetRelationshipWith(targetFaction);
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
        //state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(rel.relationshipStatus.ToString()), LOG_IDENTIFIER.STRING_1));
    }
    private void NormalImproveRelationsFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    private void NormalImproveRelationsCriticallyFailRewardEffect(InteractionState state) {
        //**Mechanics**: Relationship -1 on the two factions
        AdjustFactionsRelationship(_characterInvolved.faction, targetFaction, -1, state);
        //_characterInvolved.faction.AdjustRelationshipFor(targetFaction, -1);
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2));
    }
    #endregion

    public void SetTargetFaction(Faction targetFaction) {
        this.targetFaction = targetFaction;
    }
}
