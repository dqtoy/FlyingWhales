using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCharm : Interaction {

    private const string Character_Charm_Cancelled = "Character Charm Cancelled";
    private const string Character_Charm_Continues = "Character Charm Continues";
    private const string Do_Nothing = "Do Nothing";

    public Area targetLocation { get; private set; }

    public MoveToCharm(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.MOVE_TO_CHARM, 0) {
        _name = "Move To Charm";
        _jobFilter = new JOB[] { JOB.DISSUADER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState characterCharmCancelled = new InteractionState(Character_Charm_Cancelled, this);
        InteractionState characterCharmContinues = new InteractionState(Character_Charm_Continues, this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        targetLocation = GetTargetLocation();

        //Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        //startStateDescriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        //startStateDescriptionLog.AddToFillers(targetLocation.owner, targetLocation.owner.name, LOG_IDENTIFIER.FACTION_1);
        //startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        characterCharmCancelled.SetEffect(() => CharacterCharmCancelledRewardEffect(characterCharmCancelled));
        characterCharmContinues.SetEffect(() => CharacterCharmContinuesRewardEffect(characterCharmContinues));
        doNothing.SetEffect(() => DoNothingRewardEffect(doNothing));

        _states.Add(startState.name, startState);
        _states.Add(characterCharmCancelled.name, characterCharmCancelled);
        _states.Add(characterCharmContinues.name, characterCharmContinues);
        _states.Add(doNothing.name, doNothing);

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
    #endregion

    #region Option Effects
    private void PreventFromLeavingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorMinion.character.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Character_Charm_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Character_Charm_Continues;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region Reward Effects
    private void CharacterCharmCancelledRewardEffect(InteractionState state) {
        //**Mechanics**: Character will no longer leave.
        //**Level Up**: Dissuader Minion +1
        investigatorMinion.LevelUp();
        MinionSuccess();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    private void CharacterCharmContinuesRewardEffect(InteractionState state) {
        GoToTargetLocation();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    private void DoNothingRewardEffect(InteractionState state) {
        GoToTargetLocation();
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    #endregion

    private void GoToTargetLocation() {
        _characterInvolved.ownParty.GoToLocation(targetLocation.coreTile.landmarkOnTile, PATHFINDING_MODE.NORMAL, () => CreateCharmEvent());
    }

    private void CreateCharmEvent() {
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.CHARM_ACTION, _characterInvolved.specificLocation.tileLocation.landmarkOnTile);
        //(interaction as ImproveRelationsEvent).SetTargetFaction(targetFaction);
        //interaction.SetCanInteractionBeDoneAction(IsImproveRelationsValid);
        _characterInvolved.SetForcedInteraction(interaction);
    }
    //private bool IsImproveRelationsValid() {
    //    return targetLocation.owner != null && targetLocation.owner.id == targetFaction.id;
    //}

    private Area GetTargetLocation() {
        /* Location Selection Weights:
            - location is not part of any Faction: Weight +10
            - location is part of a Faction with Disliked, Neutral or Friend Faction: Weight +25 */
        WeightedDictionary<Area> choices = new WeightedDictionary<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            int weight = 0;
            if (currArea.id == PlayerManager.Instance.player.playerArea.id) {
                continue; //skip
            }
            if (currArea.owner == null) {
                weight += 10;
            } else if (currArea.owner.id != _characterInvolved.faction.id) {
                FactionRelationship rel = currArea.owner.GetRelationshipWith(_characterInvolved.faction);
                switch (rel.relationshipStatus) {
                    case FACTION_RELATIONSHIP_STATUS.DISLIKED:
                    case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
                    case FACTION_RELATIONSHIP_STATUS.FRIEND:
                        weight += 25;
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
        throw new System.Exception("Could not find target location for move to charm of " + _characterInvolved.faction.name);
    }
}
