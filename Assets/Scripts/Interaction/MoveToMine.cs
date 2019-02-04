using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToMine : Interaction {
    private const string Start = "Start";
    private const string Mine_Cancelled = "Mine Cancelled";
    private const string Mine_Proceeds = "Mine Proceeds";
    private const string Normal_Mine = "Normal Mine";

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }

    public MoveToMine(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_MINE, 0) {
        _name = "Move To Mine";
    }

    #region Overrides
    public override void CreateStates() {
        if (_targetArea == null) {
            _targetArea = GetTargetLocation(_characterInvolved);
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState mineCancelledState = new InteractionState(Mine_Cancelled, this);
        InteractionState mineProceedsState = new InteractionState(Mine_Proceeds, this);
        InteractionState normalMineState = new InteractionState(Normal_Mine, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        mineCancelledState.SetEffect(() => MineCancelledEffect(mineCancelledState));
        mineProceedsState.SetEffect(() => MineProceedsEffect(mineProceedsState));
        normalMineState.SetEffect(() => NormalMineEffect(normalMineState));

        _states.Add(startState.name, startState);
        _states.Add(mineCancelledState.name, mineCancelledState);
        _states.Add(mineProceedsState.name, mineProceedsState);
        _states.Add(normalMineState.name, normalMineState);

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
        AddToDebugLog(_characterInvolved.name + " will now create mine action");
        CreateConnectedEvent(INTERACTION_TYPE.MINE_ACTION, _characterInvolved.specificLocation);
    }
    #endregion

    #region Action Options
    private void PreventOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Mine_Cancelled, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Mine_Proceeds, investigatorCharacter.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Normal_Mine]);
    }
    #endregion

    #region State Effects
    private void MineCancelledEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
    }
    private void MineProceedsEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    private void NormalMineEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    #endregion

    //private void CreateMineAction() {
       
    //    Interaction mine = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINE_ACTION, _characterInvolved.specificLocation);
    //    _characterInvolved.SetForcedInteraction(mine);
    //}
    private Area GetTargetLocation(Character characterInvolved) {
        WeightedDictionary<Area> locationWeights = new WeightedDictionary<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.id != PlayerManager.Instance.player.playerArea.id && currArea.coreTile.landmarkOnTile.specificLandmarkType.ToString().Contains("MINE")) {
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
