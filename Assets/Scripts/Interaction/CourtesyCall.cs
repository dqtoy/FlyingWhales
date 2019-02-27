using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtesyCall : Interaction {
    private const string Start = "Start";
    private const string Courtesy_Call_Success = "Courtesy Call Success";
    private const string Courtesy_Call_Fail = "Courtesy Call Fail";
    private const string Target_Missing = "Target Missing";

    public CourtesyCall(Area interactable): base(interactable, INTERACTION_TYPE.COURTESY_CALL, 0) {
        _name = "Courtesy Call";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState courtesyCallSuccess = new InteractionState(Courtesy_Call_Success, this);
        InteractionState courtesyCallFail = new InteractionState(Courtesy_Call_Fail, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        courtesyCallSuccess.SetEffect(() => CourtesyCallSuccessEffect(courtesyCallSuccess));
        courtesyCallFail.SetEffect(() => CourtesyCallFailEffect(courtesyCallFail));
        targetMissing.SetEffect(() => TargetMissingEffect(targetMissing));

        _states.Add(startState.name, startState);
        _states.Add(courtesyCallSuccess.name, courtesyCallSuccess);
        _states.Add(courtesyCallFail.name, courtesyCallFail);
        _states.Add(targetMissing.name, targetMissing);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(),
            };
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (_targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character character, Character actor) {
        _targetCharacter = character;
        _targetStructure = _targetCharacter.homeStructure;
        targetGridLocation = _targetCharacter.GetNearestUnoccupiedTileFromThis(_targetStructure, actor);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect() {
        if (_characterInvolved.currentStructure == _targetCharacter.currentStructure) {
            WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
            resultWeights.AddElement(Courtesy_Call_Success, _characterInvolved.job.GetSuccessRate());
            resultWeights.AddElement(Courtesy_Call_Fail, _characterInvolved.job.GetFailRate());
            string result = resultWeights.PickRandomElementGivenWeights();
            SetCurrentState(_states[result]);
        } else {
            SetCurrentState(_states[Target_Missing]);
        }
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetStructure, targetGridLocation, _targetCharacter);
    }
    private void CourtesyCallSuccessEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        if(_characterInvolved.faction.id != _targetCharacter.faction.id && !_characterInvolved.isFactionless && !_targetCharacter.isFactionless) {
            AdjustFactionsRelationship(_characterInvolved.faction, _targetCharacter.faction, 1, state);
        } else {
            throw new System.Exception("CAN'T DO COURTESY CALL: " + _characterInvolved.name + " of " + _characterInvolved.faction.name + " to " + _targetCharacter.name + " of " + _targetCharacter.faction.name);
        }
    }
    private void CourtesyCallFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void TargetMissingEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
