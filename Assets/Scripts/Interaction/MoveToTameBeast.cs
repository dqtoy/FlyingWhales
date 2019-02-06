using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToTameBeast : Interaction {
    private const string Start = "Start";
    private const string Tame_Cancelled = "Tame Cancelled";
    private const string Tame_Proceeds = "Tame Proceeds";
    private const string Normal_Tame = "Normal Tame";

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }
    public override INTERACTION_TYPE pairedInteractionType {
        get { return INTERACTION_TYPE.TAME_BEAST_ACTION; }
    }

    public MoveToTameBeast(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_TAME_BEAST_ACTION, 0) {
        _name = "Move To Tame Beast";
        //_categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT };
        //_alignment = INTERACTION_ALIGNMENT.NEUTRAL;
    }

    #region Overrides
    public override void CreateStates() {
        if (_targetArea == null) {
            _targetArea = GetTargetLocation(_characterInvolved);
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState tameCancelledState = new InteractionState(Tame_Cancelled, this);
        InteractionState tameProceedsState = new InteractionState(Tame_Proceeds, this);
        InteractionState normalTameState = new InteractionState(Normal_Tame, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        tameCancelledState.SetEffect(() => TameCancelledEffect(tameCancelledState));
        tameProceedsState.SetEffect(() => TameProceedsEffect(tameProceedsState));
        normalTameState.SetEffect(() => NormalTameEffect(normalTameState));

        _states.Add(startState.name, startState);
        _states.Add(tameCancelledState.name, tameCancelledState);
        _states.Add(tameProceedsState.name, tameProceedsState);
        _states.Add(normalTameState.name, normalTameState);

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
        AddToDebugLog(_characterInvolved.name + " will now create tame action");
        CreateConnectedEvent(INTERACTION_TYPE.TAME_BEAST_ACTION, _characterInvolved.specificLocation);
    }
    #endregion

    #region Action Options
    private void PreventOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Tame_Cancelled, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Tame_Proceeds, investigatorCharacter.job.GetFailRate());
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Normal_Tame]);
    }
    #endregion

    #region State Effects
    private void TameCancelledEffect(InteractionState state) {
        //investigatorCharacter.LevelUp();
    }
    private void TameProceedsEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    private void NormalTameEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));

        StartMoveToAction();
    }
    #endregion

    //private void CreateTameAction() {
    //    AddToDebugLog(_characterInvolved.name + " will now create tame action");
    //    Interaction tame = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.TAME_BEAST_ACTION, _characterInvolved.specificLocation);
    //    //tame.SetCanInteractionBeDoneAction(IsTameStillValid);
    //    _characterInvolved.SetForcedInteraction(tame);
    //}
    private bool IsTameStillValid() {
        return !_characterInvolved.isHoldingItem && _targetArea.possibleSpecialTokenSpawns.Count > 0;
    }
    private Area GetTargetLocation(Character characterInvolved) {
        WeightedDictionary<Area> locationWeights = new WeightedDictionary<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            int weight = 0;
            //Check residents or characters at location for unaligned beast character to tame?
            for (int j = 0; j < currArea.areaResidents.Count; j++) {
                Character resident = currArea.areaResidents[j];
                if(!resident.currentParty.icon.isTravelling && resident.doNotDisturb <= 0 && resident.IsInOwnParty() 
                    && resident.specificLocation.id == currArea.id && resident.faction == FactionManager.Instance.neutralFaction
                    && resident.role.roleType == CHARACTER_ROLE.BEAST) {
                    weight += 35;
                    break;
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
