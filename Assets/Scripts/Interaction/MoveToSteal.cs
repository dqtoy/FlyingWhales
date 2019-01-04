using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToSteal : Interaction {

    private const string Steal_Cancelled = "Steal Cancelled";
    private const string Steal_Proceeds = "Steal Proceeds";
    private const string Normal_Steal = "Normal Steal";

    public Area targetLocation { get; private set; }

    public MoveToSteal(BaseLandmark interactable)
        : base(interactable, INTERACTION_TYPE.MOVE_TO_STEAL, 0) {
        _name = "Move To Steal";
        _jobFilter = new JOB[] { JOB.DISSUADER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState stealCancelled = new InteractionState(Steal_Cancelled, this);
        InteractionState stealProceeds = new InteractionState(Steal_Proceeds, this);
        InteractionState normalSteal = new InteractionState(Normal_Steal, this);

        targetLocation = GetTargetLocation(_characterInvolved);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        stealCancelled.SetEffect(() => CancelledRewardEffect(stealCancelled));
        stealProceeds.SetEffect(() => ProceedsRewardEffect(stealProceeds));
        normalSteal.SetEffect(() => NormalRewardEffect(normalSteal));

        _states.Add(startState.name, startState);
        _states.Add(stealCancelled.name, stealCancelled);
        _states.Add(stealProceeds.name, stealProceeds);
        _states.Add(normalSteal.name, normalSteal);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from leaving.",
                duration = 0,
                effect = () => PreventFromLeavingOptionEffect(state),
                jobNeeded = JOB.DISSUADER,
                doesNotMeetRequirementsStr = "Minion must be a dissuader",
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
    public override bool CanInteractionBeDoneBy(Character character) {
        if (GetTargetLocation(character) == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effects
    private void PreventFromLeavingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorMinion.character.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Steal_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Steal_Proceeds;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Normal_Steal]);
    }
    #endregion

    #region Reward Effects
    private void CancelledRewardEffect(InteractionState state) {
        //**Mechanics**: Character will no longer leave.
        //**Level Up**: Dissuader Minion +1
        investigatorMinion.LevelUp();
        MinionSuccess();
    }
    private void ProceedsRewardEffect(InteractionState state) {
        GoToTargetLocation();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    private void NormalRewardEffect(InteractionState state) {
        GoToTargetLocation();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    #endregion

    private void GoToTargetLocation() {
        _characterInvolved.ownParty.GoToLocation(targetLocation.coreTile.landmarkOnTile, PATHFINDING_MODE.NORMAL, () => CreateEvent());
    }

    private void CreateEvent() {
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.STEAL_ACTION, _characterInvolved.specificLocation.tileLocation.landmarkOnTile);
        //(interaction as ImproveRelationsEvent).SetTargetFaction(targetFaction);
        //interaction.SetCanInteractionBeDoneAction(IsImproveRelationsValid);
        _characterInvolved.SetForcedInteraction(interaction);
    }

    private Area GetTargetLocation(Character character) {
        WeightedDictionary<Area> choices = new WeightedDictionary<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            int weight = 0;
            if (currArea.id == PlayerManager.Instance.player.playerArea.id) {
                continue; //skip
            }
            if (currArea.owner == null) {
                weight += 5; //- Location is not owned by any faction: Weight +5
            } else if (currArea.owner.id != character.faction.id) {
                FactionRelationship rel = currArea.owner.GetRelationshipWith(character.faction);
                switch (rel.relationshipStatus) {
                    case FACTION_RELATIONSHIP_STATUS.ENEMY:
                    case FACTION_RELATIONSHIP_STATUS.DISLIKED:
                        weight += 30;//- Location is owned by a Faction that is Enemy or Disliked by Abductor's Faction: Weight +30
                        break;
                    case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
                        weight += 20;//- Location is owned by a Faction with Neutral relationship with Abductor's Faction: Weight +20
                        break;
                    case FACTION_RELATIONSHIP_STATUS.FRIEND:
                    case FACTION_RELATIONSHIP_STATUS.ALLY:
                        weight += 10; //- Location is owned by a Faction with Friend or Ally relationship with Abductor's Faction: Weight +10
                        break;
                    default:
                        break;
                }
            }
            if (weight > 0) {
                choices.AddElement(currArea, weight);
            }
        }

        if (choices.GetTotalOfWeights() > 0) {
            return choices.PickRandomElementGivenWeights();
        }
        throw new System.Exception("Could not find target location for move to steal of " + _characterInvolved.name);
    }
}
