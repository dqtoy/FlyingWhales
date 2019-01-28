using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToHunt : Interaction {
    private const string Start = "Start";
    private const string Hunt_Cancelled = "Hunt Cancelled";
    private const string Hunt_Proceeds = "Hunt Proceeds";
    private const string Normal_Hunt = "Normal Hunt";

    private Area _targetArea;

    public MoveToHunt(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_HUNT, 0) {
        _name = "Move To Hunt";
        _categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.OFFENSE };
        _alignment = INTERACTION_ALIGNMENT.NEUTRAL;
    }


    #region Overrides
    public override void CreateStates() {
        if(_targetArea == null) {
            _targetArea = GetTargetLocation(_characterInvolved);
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState huntCancelledState = new InteractionState(Hunt_Cancelled, this);
        InteractionState huntProceedsState = new InteractionState(Hunt_Proceeds, this);
        InteractionState normalHuntState = new InteractionState(Normal_Hunt, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        huntCancelledState.SetEffect(() => HuntCancelledEffect(huntCancelledState));
        huntProceedsState.SetEffect(() => HuntProceedsEffect(huntProceedsState));
        normalHuntState.SetEffect(() => NormalHuntEffect(normalHuntState));

        _states.Add(startState.name, startState);
        _states.Add(huntCancelledState.name, huntCancelledState);
        _states.Add(huntProceedsState.name, huntProceedsState);
        _states.Add(normalHuntState.name, normalHuntState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption preventOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from leaving.",
                duration = 0,
                jobNeeded = JOB.DEBILITATOR,
                disabledTooltipText = "Must be a Dissuader.",
                effect = () => PreventOption(),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(preventOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        _targetArea = GetTargetLocation(character);
        if (_targetArea == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Action Options
    private void PreventOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Hunt_Cancelled, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Hunt_Proceeds, investigatorCharacter.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Normal_Hunt]);
    }
    #endregion

    #region State Effects
    private void HuntCancelledEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
    }
    private void HuntProceedsEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMove();
    }
    private void NormalHuntEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMove();
    }
    #endregion
    private void StartMove() {
        AddToDebugLog(_characterInvolved.name + " starts moving towards " + _targetArea.name + "(" + _targetArea.coreTile.landmarkOnTile.name + ") to hunt!");
        _characterInvolved.currentParty.GoToLocation(_targetArea, PATHFINDING_MODE.NORMAL, null, () => CreateHuntAction());
    }
    private void CreateHuntAction() {
        AddToDebugLog(_characterInvolved.name + " will now create hunt action");
        Interaction hunt = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.HUNT_ACTION, _characterInvolved.specificLocation);
        hunt.SetCanInteractionBeDoneAction(IsHuntStillValid);
        _characterInvolved.SetForcedInteraction(hunt);
    }
    private bool IsHuntStillValid() {
        return _targetArea.owner == null || (_targetArea.owner != null && _targetArea.owner.id != _characterInvolved.faction.id && _targetArea.owner.id != PlayerManager.Instance.player.playerFaction.id);
    }

    private Area GetTargetLocation(Character characterInvolved) {
        WeightedDictionary<Area> locationWeights = new WeightedDictionary<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            int weight = 0;
            if (currArea.owner == null) {
                weight += 25;
            } else if (currArea.owner.id != PlayerManager.Instance.player.playerFaction.id && currArea.owner.id != characterInvolved.faction.id) {
                FactionRelationship rel = currArea.owner.GetRelationshipWith(characterInvolved.faction);
                if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY || rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED) {
                    weight += 20;
                } else if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL) {
                    weight += 15;
                } else if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                    weight += 5;
                } else if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ALLY) {
                    weight += 2;
                }
            }
            if (weight > 0) {
                locationWeights.AddElement(currArea, weight);
            }
        }
        if (locationWeights.GetTotalOfWeights() > 0) {
            return locationWeights.PickRandomElementGivenWeights();
        }
        return null;
    }
}
