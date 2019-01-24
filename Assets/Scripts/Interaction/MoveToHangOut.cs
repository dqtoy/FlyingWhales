using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToHangOut : Interaction {

    private const string Hang_Out_Cancelled = "Hang Out Cancelled";
    private const string Hang_Out_Continues = "Hang Out Continues";
    private const string Do_Nothing = "Do Nothing";

    public Area targetLocation { get; private set; }

    public MoveToHangOut(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.MOVE_TO_HANG_OUT, 0) {
        _name = "Move To Hang Out";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState hangOutCancelled = new InteractionState(Hang_Out_Cancelled, this);
        InteractionState hangOutContinues = new InteractionState(Hang_Out_Continues, this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        targetLocation = GetTargetLocation(_characterInvolved);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        hangOutCancelled.SetEffect(() => HangOutCancelledRewardEffect(hangOutCancelled));
        hangOutContinues.SetEffect(() => HangOutContinuesRewardEffect(hangOutContinues));
        doNothing.SetEffect(() => DoNothingRewardEffect(doNothing));

        _states.Add(startState.name, startState);
        _states.Add(hangOutCancelled.name, hangOutCancelled);
        _states.Add(hangOutContinues.name, hangOutContinues);
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
                jobNeeded = JOB.DEBILITATOR,
                doesNotMeetRequirementsStr = "Must have debilitator minion.",
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
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Hang_Out_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Hang_Out_Continues;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region Reward Effects
    private void HangOutCancelledRewardEffect(InteractionState state) {
        //**Mechanics**: Character will no longer leave.
        //**Level Up**: Dissuader Minion +1
        investigatorCharacter.LevelUp();
        MinionSuccess();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    private void HangOutContinuesRewardEffect(InteractionState state) {
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
        _characterInvolved.ownParty.GoToLocation(targetLocation, PATHFINDING_MODE.NORMAL, () => CreateCharmEvent());
    }

    private void CreateCharmEvent() {
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.CHARM_ACTION, _characterInvolved.specificLocation.coreTile.landmarkOnTile);
        //(interaction as ImproveRelationsEvent).SetTargetFaction(targetFaction);
        //interaction.SetCanInteractionBeDoneAction(IsImproveRelationsValid);
        _characterInvolved.SetForcedInteraction(interaction);
    }
    private Area GetTargetLocation(Character character) {
        /* A character decides to hang out with another character that he has a Positive or Neutral relationship with. Select by weights:
            - for each positive relationship with the character: +50 weight
            - for each neutral relationship with the character: +25 weight
            - for each negative relationship with the character: -25 weight */
        WeightedDictionary<Character> choices = new WeightedDictionary<Character>();
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character currArea = CharacterManager.Instance.allCharacters[i];
            int weight = 0;
            if (currArea.id == PlayerManager.Instance.player.playerArea.id) {
                continue; //skip
            }
            //if (currArea.owner == null) {
            //    weight += 10;
            //} else if (currArea.owner.id != character.faction.id) {
            //    FactionRelationship rel = currArea.owner.GetRelationshipWith(character.faction);
            //    switch (rel.relationshipStatus) {
            //        case FACTION_RELATIONSHIP_STATUS.DISLIKED:
            //        case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
            //        case FACTION_RELATIONSHIP_STATUS.FRIEND:
            //            weight += 25;
            //            break;
            //        default:
            //            break;
            //    }
            //}
            if (weight > 0) {
                choices.AddElement(currArea, weight);
            }
        }

        if (choices.GetTotalOfWeights() > 0) {
            //return choices.PickRandomElementGivenWeights();
        }
        return null;
        //throw new System.Exception("Could not find target location for move to charm of " + _characterInvolved.faction.name);
    }
}
