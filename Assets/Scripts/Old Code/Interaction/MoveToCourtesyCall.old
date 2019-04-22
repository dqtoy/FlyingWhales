using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCourtesyCall : Interaction {
    private const string Start = "Start";
    private const string Courtesy_Call_Proceeds = "Courtesy Call Proceeds";

    public override Area targetArea {
        get { return _targetCharacter.homeArea; }
    }

    public override INTERACTION_TYPE pairedInteractionType {
        get { return INTERACTION_TYPE.COURTESY_CALL; }
    }

    public MoveToCourtesyCall(Area interactable): base(interactable, INTERACTION_TYPE.MOVE_TO_COURTESY_CALL, 0) {
        _name = "Move To Courtesy Call";
    }

    #region Overrides
    public override void CreateStates() {
        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved), _characterInvolved);
        }
        InteractionState startState = new InteractionState(Start, this);
        InteractionState courtesyCallProceeds = new InteractionState(Courtesy_Call_Proceeds, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        startStateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        courtesyCallProceeds.SetEffect(() => CourtesyCallProceedsEffect(courtesyCallProceeds));

        _states.Add(startState.name, startState);
        _states.Add(courtesyCallProceeds.name, courtesyCallProceeds);

        //SetCurrentState(startState);
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
        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(character), character);
        }
        if (_targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void DoActionUponMoveToArrival() {
        Interaction interaction = CreateConnectedEvent(INTERACTION_TYPE.COURTESY_CALL, targetArea);
        interaction.SetTargetCharacter(targetCharacter, _characterInvolved);
    }
    public override void SetTargetCharacter(Character character, Character actor) {
        _targetCharacter = character;
    }
    #endregion

    #region Option Effects
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Courtesy_Call_Proceeds]);
    }
    #endregion

    #region Reward Effects
    private void CourtesyCallProceedsEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        StartMoveToAction();
    }
    #endregion

    private Character GetTargetCharacter(Character character) {
        WeightedDictionary<Character> weights = new WeightedDictionary<Character>();
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in character.faction.relationships) {
            if(kvp.Key == PlayerManager.Instance.player.playerFaction) { continue; }
            if(kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY) {
                for (int i = 0; i < kvp.Key.ownedAreas.Count; i++) {
                    Area currArea = kvp.Key.ownedAreas[i];
                    for (int k = 0; k < currArea.areaResidents.Count; k++) {
                        Character resident = currArea.areaResidents[k];
                        if (resident.id != character.id && resident.faction.id == kvp.Key.id && (resident.role.roleType == CHARACTER_ROLE.NOBLE || resident.isLeader)) {
                            weights.AddElement(resident, 20);
                        }
                    }
                }
            }else if (kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED || kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL) {
                for (int i = 0; i < kvp.Key.ownedAreas.Count; i++) {
                    Area currArea = kvp.Key.ownedAreas[i];
                    for (int k = 0; k < currArea.areaResidents.Count; k++) {
                        Character resident = currArea.areaResidents[k];
                        if (resident.id != character.id && resident.faction.id == kvp.Key.id && (resident.role.roleType == CHARACTER_ROLE.NOBLE || resident.isLeader)) {
                            weights.AddElement(resident, 15);
                        }
                    }
                }
            } else if (kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                for (int i = 0; i < kvp.Key.ownedAreas.Count; i++) {
                    Area currArea = kvp.Key.ownedAreas[i];
                    for (int k = 0; k < currArea.areaResidents.Count; k++) {
                        Character resident = currArea.areaResidents[k];
                        if (resident.id != character.id && resident.faction.id == kvp.Key.id && (resident.role.roleType == CHARACTER_ROLE.NOBLE || resident.isLeader)) {
                            weights.AddElement(resident, 5);
                        }
                    }
                }
            }
        }
        if (weights.Count > 0) {
            return weights.PickRandomElementGivenWeights();
        }
        return null;
    }
}
