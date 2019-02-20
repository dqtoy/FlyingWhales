using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToGiftBeast : Interaction {
    private const string Start = "Start";
    private const string Gifting_Proceeds = "Gifting Proceeds";

    private Character _targetCharacter;

    public override Area targetArea {
        get { return _targetCharacter.homeArea; }
    }

    public override INTERACTION_TYPE pairedInteractionType {
        get { return INTERACTION_TYPE.GIFT_BEAST; }
    }

    public MoveToGiftBeast(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_GIFT_BEAST, 0) {
        _name = "Move To Gift Beast";
    }

    #region Overrides
    public override void CreateStates() {
        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }
        InteractionState startState = new InteractionState(Start, this);
        InteractionState giftingProceeds = new InteractionState(Gifting_Proceeds, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        startStateDescriptionLog.AddToFillers(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        giftingProceeds.SetEffect(() => GiftingProceedsEffect(giftingProceeds));

        _states.Add(startState.name, startState);
        _states.Add(giftingProceeds.name, giftingProceeds);

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
        if (!InteractionManager.Instance.CanCreateInteraction(type, character)) {
            return false;
        }
        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(character));
        }
        if (_targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void DoActionUponMoveToArrival() {
        Interaction interaction = CreateConnectedEvent(INTERACTION_TYPE.GIFT_BEAST, targetArea);
        interaction.SetTargetCharacter(targetCharacter);
    }
    public override void SetTargetCharacter(Character character) {
        _targetCharacter = character;
    }
    #endregion

    #region Option Effects
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Gifting_Proceeds]);
    }
    #endregion

    #region Reward Effects
    private void GiftingProceedsEffect(InteractionState state) {
        List<Character> idleBeasts = new List<Character>();
        for (int i = 0; i < _characterInvolved.specificLocation.areaResidents.Count; i++) {
            Character resident = _characterInvolved.specificLocation.areaResidents[i];
            if (resident.id != _characterInvolved.id && resident.role.roleType == CHARACTER_ROLE.BEAST && resident.faction.id == _characterInvolved.faction.id && resident.isIdle) {
                idleBeasts.Add(resident);
            }
        }
        Character chosenBeast = idleBeasts[UnityEngine.Random.Range(0, idleBeasts.Count)];
        chosenBeast.AddTrait(AttributeManager.Instance.allTraits["Packaged"]);
        _characterInvolved.currentParty.AddCharacter(chosenBeast);

        state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        state.descriptionLog.AddToFillers(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(chosenBeast, chosenBeast.name, LOG_IDENTIFIER.CHARACTER_3);

        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
        state.AddLogFiller(new LogFiller(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(chosenBeast, chosenBeast.name, LOG_IDENTIFIER.CHARACTER_3));

        StartMoveToAction();
    }
    #endregion

    private Character GetTargetCharacter(Character character) {
        WeightedDictionary<Character> weights = new WeightedDictionary<Character>();
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in character.faction.relationships) {
            if (kvp.Key == PlayerManager.Instance.player.playerFaction) { continue; }
            if (kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY) {
                if(kvp.Key.leader is Character) {
                    Character factionLeader = kvp.Key.leader as Character;
                    weights.AddElement(factionLeader, 20);
                }
            } else if (kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED || kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL) {
                if (kvp.Key.leader is Character) {
                    Character factionLeader = kvp.Key.leader as Character;
                    weights.AddElement(factionLeader, 15);
                }   
            } else if (kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                if (kvp.Key.leader is Character) {
                    Character factionLeader = kvp.Key.leader as Character;
                    weights.AddElement(factionLeader, 5);
                }
            }
        }
        if (weights.Count > 0) {
            return weights.PickRandomElementGivenWeights();
        }
        return null;
    }
}
