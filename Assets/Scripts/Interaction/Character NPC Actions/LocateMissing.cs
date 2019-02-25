using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocateMissing : Interaction {
    private const string Start = "Start";
    private const string Target_Found = "Target Found";
    private const string Target_Missing = "Target Missing";

    public LocateMissing(Area interactable) : base(interactable, INTERACTION_TYPE.LOCATE_MISSING, 0) {
        _name = "Locate Missing";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState targetFound = new InteractionState(Target_Found, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        targetFound.SetEffect(() => TargetFoundEffect(targetFound));
        targetMissing.SetEffect(() => TargetMissingEffect(targetMissing));

        _states.Add(startState.name, startState);
        _states.Add(targetFound.name, targetFound);
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
        CharacterRelationshipData characterRelationshipData = character.GetCharacterRelationshipData(_targetCharacter);
        if (characterRelationshipData != null) {
            _targetStructure = characterRelationshipData.knownStructure;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character targetCharacter) {
        _targetCharacter = targetCharacter;
        
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect() {
        if (_characterInvolved.currentStructure == _targetCharacter.currentStructure) {
            SetCurrentState(_states[Target_Found]);
        } else {
            SetCurrentState(_states[Target_Missing]);
        }
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetStructure);
    }
    private void TargetFoundEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        CharacterRelationshipData characterRelationshipData = _characterInvolved.GetCharacterRelationshipData(_targetCharacter);
        if (characterRelationshipData != null) {
            characterRelationshipData.SetIsCharacterMissing(false);
        }
    }
    private void TargetMissingEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
