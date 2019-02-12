using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToReanimate : Interaction {
    
    private const string Normal_Undeath = "Normal Undeath";

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }
    public override INTERACTION_TYPE pairedInteractionType {
        get { return INTERACTION_TYPE.REANIMATE_ACTION; }
    }

    public MoveToReanimate(Area interactable) 
        : base(interactable, INTERACTION_TYPE.MOVE_TO_REANIMATE_ACTION, 0) {
        _name = "Move To Reanimate";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState normalUndeath = new InteractionState(Normal_Undeath, this);

        _targetArea = GetTargetLocation(_characterInvolved);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        normalUndeath.SetEffect(() => NormalUndeathRewardEffect(normalUndeath));

        _states.Add(startState.name, startState);
        _states.Add(normalUndeath.name, normalUndeath);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingEffect(state),
            };
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
        CreateConnectedEvent(INTERACTION_TYPE.REANIMATE_ACTION, _targetArea);
    }
    #endregion

    #region Option Effects
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Normal_Undeath]);
    }
    #endregion

    #region Reward Effects
    private void NormalUndeathRewardEffect(InteractionState state) {
        StartMoveToAction();
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    #endregion

    //private void CreateEvent() {
    //    Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.REANIMATE_ACTION, _targetArea);
    //    _characterInvolved.SetForcedInteraction(interaction);
    //}

    private Area GetTargetLocation(Character character) {
        WeightedDictionary<Area> choices = new WeightedDictionary<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.id == character.specificLocation.id || currArea.corpsesInArea.Count == 0) {
                continue; //skip
            }
            int weight = 0;
            if (currArea.owner == null) {
                weight += 15; //- Location is not owned by any faction: Weight +15
            } else if (currArea.owner.id == character.faction.id) {
                weight += 10; //- Location is owned by this faction: Weight +10
            } else {
                FactionRelationship rel = currArea.owner.GetRelationshipWith(character.faction);
                switch (rel.relationshipStatus) {
                    case FACTION_RELATIONSHIP_STATUS.ENEMY:
                    case FACTION_RELATIONSHIP_STATUS.DISLIKED:
                    case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
                        weight += 20; // - Location is owned by a Faction that is Enemy, Disliked or Neutral with Resurrector's Faction: Weight +20
                        break;
                    case FACTION_RELATIONSHIP_STATUS.FRIEND:
                        weight += 5; //- Location is owned by a Faction with Friend relationship with Resurrector's Faction: Weight +5
                        break;
                    case FACTION_RELATIONSHIP_STATUS.ALLY:
                        weight += 2; //- Location is owned by a Faction with Ally relationship with Resurrector's Faction: Weight +2
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
    }
}
