using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstAidAction : Interaction {
    private const string Start = "Start";
    private const string First_Aid_Success = "First Aid Success";
    private const string First_Aid_Fail = "First Aid Fail";
    private const string First_Aid_Critical_Fail = "First Aid Critical Fail";
    private const string Target_Missing = "Target Missing";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public FirstAidAction(Area interactable): base(interactable, INTERACTION_TYPE.FIRST_AID_ACTION, 0) {
        _name = "First Aid Action";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState firstAidSuccess = new InteractionState(First_Aid_Success, this);
        InteractionState firstAidFail = new InteractionState(First_Aid_Fail, this);
        InteractionState firstAidCritFail = new InteractionState(First_Aid_Critical_Fail, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        firstAidSuccess.SetEffect(() => FirstAidSuccessEffect(firstAidSuccess));
        firstAidFail.SetEffect(() => FirstAidFailEffect(firstAidFail));
        firstAidCritFail.SetEffect(() => FirstAidCritFailEffect(firstAidCritFail));
        targetMissing.SetEffect(() => TargetMissingEffect(targetMissing));

        _states.Add(startState.name, startState);
        _states.Add(firstAidSuccess.name, firstAidSuccess);
        _states.Add(firstAidFail.name, firstAidFail);
        _states.Add(firstAidCritFail.name, firstAidCritFail);
        _states.Add(targetMissing.name, targetMissing);

        SetCurrentState(startState);
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
    public override void SetTargetCharacter(Character targetCharacter) {
        this._targetCharacter = targetCharacter;
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect() {
        WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();

        if (_characterInvolved.currentStructure == _targetCharacter.currentStructure) {
            resultWeights.AddElement(First_Aid_Success, 30);
            resultWeights.AddElement(First_Aid_Fail, 10);
            resultWeights.AddElement(First_Aid_Critical_Fail, 3);
            string result = resultWeights.PickRandomElementGivenWeights();
            SetCurrentState(_states[result]);
        } else {
            SetCurrentState(_states[Target_Missing]);
        }
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        CharacterRelationshipData characterRelationshipData = _characterInvolved.GetCharacterRelationshipData(_targetCharacter);
        if (characterRelationshipData != null) {
            _characterInvolved.MoveToAnotherStructure(characterRelationshipData.knownStructure);
        }
    }
    private void FirstAidSuccessEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        if (!_targetCharacter.RemoveTrait("Unconscious")) {
            _targetCharacter.RemoveTrait("Sick");
        }
    }
    private void FirstAidFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void FirstAidCritFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.Death();
    }
    private void TargetMissingEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
