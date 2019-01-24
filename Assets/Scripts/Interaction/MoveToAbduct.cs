using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToAbduct : Interaction {
    private const string Start = "Start";
    private const string Abduct_Cancelled = "Abduct Cancelled";
    private const string Abduct_Proceeds = "Abduct Proceeds";
    private const string Normal_Abduct = "Normal Abduct";

    private Area _targetArea;

    public MoveToAbduct(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_ABDUCT, 0) {
        _name = "Move To Abduct";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }


    #region Overrides
    public override void CreateStates() {
        if(_targetArea == null) {
            _targetArea = GetTargetLocation(_characterInvolved);
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState abductCancelledState = new InteractionState(Abduct_Cancelled, this);
        InteractionState abductProceedsState = new InteractionState(Abduct_Proceeds, this);
        InteractionState normalAbductState = new InteractionState(Normal_Abduct, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        abductCancelledState.SetEffect(() => AbductCancelledEffect(abductCancelledState));
        abductProceedsState.SetEffect(() => AbductProceedsEffect(abductProceedsState));
        normalAbductState.SetEffect(() => NormalAbductEffect(normalAbductState));

        _states.Add(startState.name, startState);
        _states.Add(abductCancelledState.name, abductCancelledState);
        _states.Add(abductProceedsState.name, abductProceedsState);
        _states.Add(normalAbductState.name, normalAbductState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption preventOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + ".",
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
        effectWeights.AddElement(Abduct_Cancelled, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Abduct_Proceeds, investigatorCharacter.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Normal_Abduct]);
    }
    #endregion

    #region State Effects
    private void AbductCancelledEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
    }
    private void AbductProceedsEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMove();
    }
    private void NormalAbductEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMove();
    }
    #endregion
    private void StartMove() {
        AddToDebugLog(_characterInvolved.name + " starts moving towards " + _targetArea.name + "(" + _targetArea.coreTile.landmarkOnTile.name + ") to abduct!");
        _characterInvolved.currentParty.GoToLocation(_targetArea, PATHFINDING_MODE.NORMAL, () => CreateAbductAction());
    }
    private void CreateAbductAction() {
        AddToDebugLog(_characterInvolved.name + " will now create abduct action");
        Interaction abduct = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.ABDUCT_ACTION, _characterInvolved.specificLocation.coreTile.landmarkOnTile);
        //abduct.SetCanInteractionBeDoneAction(IsAbductStillValid);
        _characterInvolved.SetForcedInteraction(abduct);
    }
    private bool IsAbductStillValid() {
        if (!_characterInvolved.homeArea.IsResidentsFull()) {
            for (int i = 0; i < _targetArea.charactersAtLocation.Count; i++) {
                Character currCharacter = _targetArea.charactersAtLocation[i];
                if (currCharacter.id != _characterInvolved.id && !currCharacter.currentParty.icon.isTravelling && currCharacter.IsInOwnParty()) {
                    if (currCharacter.isFactionless || currCharacter.faction.id != _characterInvolved.faction.id) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private Area GetTargetLocation(Character characterInvolved) {
        WeightedDictionary<Area> locationWeights = new WeightedDictionary<Area>();
        bool areaFitsCriteria = false;
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            areaFitsCriteria = false;
            if (currArea.owner == null || currArea.owner.id != PlayerManager.Instance.player.playerFaction.id && currArea.owner.id != characterInvolved.faction.id) {
                for (int j = 0; j < currArea.charactersAtLocation.Count; j++) {
                    Character character = currArea.charactersAtLocation[j];
                    if (character.id != characterInvolved.id && character.IsInOwnParty() && !character.currentParty.icon.isTravelling && (character.isFactionless || character.faction.id != characterInvolved.faction.id)) {
                        areaFitsCriteria = true;
                        break;
                    }
                }
            }
            if (areaFitsCriteria) {
                int weight = 0;
                if (currArea.owner == null) {
                    weight += 15;
                } else if (currArea.owner.id != PlayerManager.Instance.player.playerFaction.id && currArea.owner.id != characterInvolved.faction.id) {
                    FactionRelationship rel = currArea.owner.GetRelationshipWith(characterInvolved.faction);
                    if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY || rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED) {
                        weight += 25;
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
        }
        if (locationWeights.GetTotalOfWeights() > 0) {
            return locationWeights.PickRandomElementGivenWeights();
        }
        return null;
    }
}
