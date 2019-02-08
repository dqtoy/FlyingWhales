using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToVisit : Interaction {
    private const string Start = "Start";
    private const string Visit_Cancelled = "Visit Cancelled";
    private const string Visit_Proceeds = "Visit Proceeds";
    private const string Normal_Visit = "Normal Visit";

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }

    public MoveToVisit(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_VISIT, 0) {
        _name = "Move To Visit";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }


    #region Overrides
    public override void CreateStates() {
        if(_targetArea == null) {
            _targetArea = GetTargetLocation(_characterInvolved);
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState visitCancelledState = new InteractionState(Visit_Cancelled, this);
        InteractionState visitProceedsState = new InteractionState(Visit_Proceeds, this);
        InteractionState normalVisitState = new InteractionState(Normal_Visit, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        visitCancelledState.SetEffect(() => VisitCancelledEffect(visitCancelledState));
        visitProceedsState.SetEffect(() => VisitProceedsEffect(visitProceedsState));
        normalVisitState.SetEffect(() => NormalVisitEffect(normalVisitState));

        _states.Add(startState.name, startState);
        _states.Add(visitCancelledState.name, visitCancelledState);
        _states.Add(visitProceedsState.name, visitProceedsState);
        _states.Add(normalVisitState.name, normalVisitState);

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
        effectWeights.AddElement(Visit_Cancelled, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Visit_Proceeds, investigatorCharacter.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Normal_Visit]);
    }
    #endregion

    #region State Effects
    private void VisitCancelledEffect(InteractionState state) {
        //investigatorCharacter.LevelUp();
    }
    private void VisitProceedsEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    private void NormalVisitEffect(InteractionState state) {
        //state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    #endregion

    private Area GetTargetLocation(Character characterInvolved) {
        WeightedDictionary<Area> locationWeights = new WeightedDictionary<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.id == PlayerManager.Instance.player.playerArea.id) {
                continue; //skip
            }
            int weight = 0;
            if(currArea.owner == null) {
                weight += 30;
            } else {
                if(currArea.owner.id == characterInvolved.faction.id) {
                    weight += 100;
                } else {
                    FactionRelationship rel = currArea.owner.GetRelationshipWith(characterInvolved.faction);
                    if (rel.relationshipStatus != FACTION_RELATIONSHIP_STATUS.ENEMY) {
                        weight += 30;
                    }else if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.AT_WAR) {
                        weight -= 50;
                    }
                }
            }
            foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in characterInvolved.relationships) {
                if(kvp.Key.homeArea.id == currArea.id) {
                    weight += 50;
                }
                if(kvp.Value.trouble.Count > 0 && kvp.Value.knownStructure.location.id == currArea.id && !kvp.Value.HasRelationshipTrait(RELATIONSHIP_TRAIT.ENEMY)) {
                    weight += 200;
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
