using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCharmFaction : Interaction {

    private const string Character_Charm_Cancelled = "Character Charm Cancelled";
    private const string Character_Charm_Continues = "Character Charm Continues";
    private const string Do_Nothing = "Do Nothing";

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }

    public MoveToCharmFaction(Area interactable) 
        : base(interactable, INTERACTION_TYPE.MOVE_TO_CHARM_FACTION, 0) {
        _name = "Move To Charm Faction";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
        //_categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT };
        //_alignment = INTERACTION_ALIGNMENT.NEUTRAL;
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState characterCharmCancelled = new InteractionState(Character_Charm_Cancelled, this);
        InteractionState characterCharmContinues = new InteractionState(Character_Charm_Continues, this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        _targetArea = GetTargetLocation(_characterInvolved);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

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
    public override void DoActionUponMoveToArrival() {
        CreateEvent();
    }
    #endregion

    #region Option Effects
    private void PreventFromLeavingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
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
        investigatorCharacter.LevelUp();
        MinionSuccess();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void CharacterCharmContinuesRewardEffect(InteractionState state) {
        StartMoveToAction();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void DoNothingRewardEffect(InteractionState state) {
        StartMoveToAction();
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    #endregion

    private void CreateEvent() {
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.CHARM_ACTION_FACTION, _characterInvolved.specificLocation);
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
                weight += 10; // location is not part of any Faction: Weight +10
            } else if (currArea.owner.id != character.faction.id) {
                FactionRelationship rel = currArea.owner.GetRelationshipWith(character.faction);
                switch (rel.relationshipStatus) {
                    case FACTION_RELATIONSHIP_STATUS.DISLIKED:
                    case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
                    case FACTION_RELATIONSHIP_STATUS.ENEMY:
                    case FACTION_RELATIONSHIP_STATUS.AT_WAR:
                        weight += 25; //location is part of a Faction with War, Enemy, Disliked or Neutral Faction: Weight +25
                        break;
                    default:
                        weight += 5; //location is part of a Faction with Friendly Faction: Weight +5
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
        return null;
        //throw new System.Exception("Could not find target location for move to charm of " + _characterInvolved.faction.name);
    }
}
