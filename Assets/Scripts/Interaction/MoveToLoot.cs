using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToLoot : Interaction {
    private const string Start = "Start";
    private const string Loot_Cancelled = "Loot Cancelled";
    private const string Loot_Proceeds = "Loot Proceeds";
    private const string Normal_Loot = "Normal Loot";

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }

    public MoveToLoot(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_LOOT, 0) {
        _name = "Move To Loot";
        _categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY };
        _alignment = INTERACTION_ALIGNMENT.EVIL;
    }

    #region Overrides
    public override void CreateStates() {
        if (_targetArea == null) {
            _targetArea = GetTargetLocation(_characterInvolved);
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState lootCancelledState = new InteractionState(Loot_Cancelled, this);
        InteractionState lootProceedsState = new InteractionState(Loot_Proceeds, this);
        InteractionState normalLootState = new InteractionState(Normal_Loot, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        lootCancelledState.SetEffect(() => LootCancelledEffect(lootCancelledState));
        lootProceedsState.SetEffect(() => LootProceedsEffect(lootProceedsState));
        normalLootState.SetEffect(() => NormalLootEffect(normalLootState));

        _states.Add(startState.name, startState);
        _states.Add(lootCancelledState.name, lootCancelledState);
        _states.Add(lootProceedsState.name, lootProceedsState);
        _states.Add(normalLootState.name, normalLootState);

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
    public override void DoActionUponMoveToArrival() {
        CreateLootAction();
    }
    #endregion

    #region Action Options
    private void PreventOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Loot_Cancelled, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Loot_Proceeds, investigatorCharacter.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Normal_Loot]);
    }
    #endregion

    #region State Effects
    private void LootCancelledEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
    }
    private void LootProceedsEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    private void NormalLootEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    #endregion

    private void CreateLootAction() {
        AddToDebugLog(_characterInvolved.name + " will now create loot action");
        Interaction loot = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.LOOT_ACTION, _characterInvolved.specificLocation);
        //loot.SetCanInteractionBeDoneAction(IsLootStillValid);
        _characterInvolved.SetForcedInteraction(loot);
    }
    private bool IsLootStillValid() {
        return !_characterInvolved.isHoldingItem && _targetArea.possibleSpecialTokenSpawns.Count > 0;
    }
    private Area GetTargetLocation(Character characterInvolved) {
        WeightedDictionary<Area> locationWeights = new WeightedDictionary<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            int weight = 0;
            if (currArea.owner != null && currArea.owner.id != PlayerManager.Instance.player.playerFaction.id && currArea.owner.id != characterInvolved.faction.id) {
                FactionRelationship rel = currArea.owner.GetRelationshipWith(characterInvolved.faction);
                if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY || rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED) {
                    weight += 30;
                } else if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NEUTRAL) {
                    weight += 15;
                } else if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND || rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ALLY) {
                    weight += 5;
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
