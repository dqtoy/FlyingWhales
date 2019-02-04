using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToImproveRelations : Interaction {

    private const string Improve_Relations_Cancelled = "Improve Relations Cancelled";
    private const string Improve_Relations_Proceeds = "Improve Relations Proceeds";
    private const string Normal_Improve_Relations = "Normal Improve Relations";

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }
    private Faction targetFaction;

    public MoveToImproveRelations(Area interactable) 
        : base(interactable, INTERACTION_TYPE.MOVE_TO_IMPROVE_RELATIONS, 0) {
        _name = "Move To Improve Relations";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState improveRelationsCancelled = new InteractionState(Improve_Relations_Cancelled, this);
        InteractionState improveRelationsProceeds = new InteractionState(Improve_Relations_Proceeds, this);
        InteractionState normalImproveRelations = new InteractionState(Normal_Improve_Relations, this);

        _targetArea = GetTargetLocation();
        targetFaction = _targetArea.owner;

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        startStateDescriptionLog.AddToFillers(_targetArea.owner, _targetArea.owner.name, LOG_IDENTIFIER.FACTION_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        improveRelationsCancelled.SetEffect(() => ImproveRelationsCancelledRewardEffect(improveRelationsCancelled));
        improveRelationsProceeds.SetEffect(() => ImproveRelationsProceedsRewardEffect(improveRelationsProceeds));
        normalImproveRelations.SetEffect(() => NormalImproveRelationsRewardEffect(normalImproveRelations));

        _states.Add(startState.name, startState);
        _states.Add(improveRelationsCancelled.name, improveRelationsCancelled);
        _states.Add(improveRelationsProceeds.name, improveRelationsProceeds);
        _states.Add(normalImproveRelations.name, normalImproveRelations);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from leaving.",
                duration = 0,
                effect = () => DiscourageFromLeavingOptionEffect(state),
                jobNeeded = JOB.DEBILITATOR,
                doesNotMeetRequirementsStr = "Must have dissuader minion."
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(prevent);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override void DoActionUponMoveToArrival() {
        Interaction interaction = CreateConnectedEvent(INTERACTION_TYPE.IMPROVE_RELATIONS_EVENT, _characterInvolved.specificLocation);
        (interaction as ImproveRelationsEvent).SetTargetFaction(targetFaction);
        interaction.SetCanInteractionBeDoneAction(IsImproveRelationsValid);
    }
    #endregion

    #region Option Effects
    private void DiscourageFromLeavingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Improve_Relations_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Improve_Relations_Proceeds;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Normal_Improve_Relations]);
    }
    #endregion

    #region Reward Effects
    private void ImproveRelationsCancelledRewardEffect(InteractionState state) {
        //**Mechanics**: Character will no longer leave.
        //**Level Up**: Dissuader Minion +1
        investigatorCharacter.LevelUp();
        MinionSuccess();
    }
    private void ImproveRelationsProceedsRewardEffect(InteractionState state) {
        //**Mechanics**: Character travels to the Location to start an Expansion event.
        StartMoveToAction();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
            state.descriptionLog.AddToFillers(_targetArea.owner, _targetArea.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
        state.AddLogFiller(new LogFiller(_targetArea.owner, _targetArea.owner.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void NormalImproveRelationsRewardEffect(InteractionState state) {
        //**Mechanics**: Character travels to the Location to start an Expansion event.
        StartMoveToAction();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
            state.descriptionLog.AddToFillers(_targetArea.owner, _targetArea.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
        state.AddLogFiller(new LogFiller(_targetArea.owner, _targetArea.owner.name, LOG_IDENTIFIER.FACTION_1));
    }
    #endregion

    //private void CreateEvent() {
    //    Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.IMPROVE_RELATIONS_EVENT, _characterInvolved.specificLocation);
    //    (interaction as ImproveRelationsEvent).SetTargetFaction(targetFaction);
    //    interaction.SetCanInteractionBeDoneAction(IsImproveRelationsValid);
    //    _characterInvolved.SetForcedInteraction(interaction);
    //}
    private bool IsImproveRelationsValid() {
        return _targetArea.owner != null && _targetArea.owner.id == targetFaction.id;
    }

    private Area GetTargetLocation() {
        List<Area> choices = new List<Area>();
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in _characterInvolved.faction.relationships) {
            if (kvp.Key.isActive && kvp.Key.id != PlayerManager.Instance.player.playerFaction.id && kvp.Value.relationshipStatus != FACTION_RELATIONSHIP_STATUS.ALLY) {
                choices.AddRange(kvp.Key.ownedAreas);
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        throw new System.Exception("Could not find target location for improve relations of faction " + _characterInvolved.faction.name);
    }
}
