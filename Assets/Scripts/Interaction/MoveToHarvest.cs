using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToHarvest : Interaction {
    private const string Start = "Start";
    private const string Harvest_Cancelled = "Harvest Cancelled";
    private const string Harvest_Proceeds = "Harvest Proceeds";
    private const string Normal_Harvest = "Normal Harvest";

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }

    public MoveToHarvest(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_HARVEST, 0) {
        _name = "Move To Harvest";
    }

    #region Overrides
    public override void CreateStates() {
        if (_targetArea == null) {
            _targetArea = GetTargetLocation(_characterInvolved);
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState harvestCancelledState = new InteractionState(Harvest_Cancelled, this);
        InteractionState harvestProceedsState = new InteractionState(Harvest_Proceeds, this);
        InteractionState normalHarvestState = new InteractionState(Normal_Harvest, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        harvestCancelledState.SetEffect(() => HarvestCancelledEffect(harvestCancelledState));
        harvestProceedsState.SetEffect(() => HarvestProceedsEffect(harvestProceedsState));
        normalHarvestState.SetEffect(() => NormalHarvestEffect(normalHarvestState));

        _states.Add(startState.name, startState);
        _states.Add(harvestCancelledState.name, harvestCancelledState);
        _states.Add(harvestProceedsState.name, harvestProceedsState);
        _states.Add(normalHarvestState.name, normalHarvestState);

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
        AddToDebugLog(_characterInvolved.name + " will now create harvest action");
        CreateConnectedEvent(INTERACTION_TYPE.HARVEST_ACTION, _characterInvolved.specificLocation);
    }
    #endregion

    #region Action Options
    private void PreventOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Harvest_Cancelled, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Harvest_Proceeds, investigatorCharacter.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Normal_Harvest]);
    }
    #endregion

    #region State Effects
    private void HarvestCancelledEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
    }
    private void HarvestProceedsEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    private void NormalHarvestEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    #endregion

    //private void CreateHarvestAction() {
    //    AddToDebugLog(_characterInvolved.name + " will now create harvest action");
    //    Interaction harvest = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.HARVEST_ACTION, _characterInvolved.specificLocation);
    //    _characterInvolved.SetForcedInteraction(harvest);
    //}
    private Area GetTargetLocation(Character characterInvolved) {
        WeightedDictionary<Area> locationWeights = new WeightedDictionary<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.id != PlayerManager.Instance.player.playerArea.id && currArea.coreTile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.FARM) {
                int weight = 1000 - (100 * characterInvolved.specificLocation.coreTile.GetTileDistanceTo(currArea.coreTile));
                if (weight < 50) {
                    weight = 50;
                }
                locationWeights.AddElement(currArea, weight);
            }
        }
        if (locationWeights.Count > 0) {
            return locationWeights.PickRandomElementGivenWeights();
        }
        return null;
    }
}
