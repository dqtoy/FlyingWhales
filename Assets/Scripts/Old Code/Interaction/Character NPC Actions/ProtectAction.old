using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectAction : Interaction {
    private const string Start = "Start";
    private const string Protect_Begins = "Protect Begins";
    private const string Protect_Fails = "Protect Fails";

    public ProtectAction(Area interactable) : base(interactable, INTERACTION_TYPE.PROTECT_ACTION, 0) {
        _name = "Protect Action";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState protectBegins = new InteractionState(Protect_Begins, this);
        InteractionState protectFails = new InteractionState(Protect_Fails, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        protectBegins.SetEffect(() => ProtectBeginsEffect(protectBegins));
        protectFails.SetEffect(() => ProtectFailsEffect(protectFails));

        _states.Add(startState.name, startState);
        _states.Add(protectBegins.name, protectBegins);
        _states.Add(protectFails.name, protectFails);

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
    public override void SetTargetCharacter(Character character, Character actor) {
        _targetCharacter = character;
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect() {
        if (_characterInvolved.currentStructure == _targetCharacter.currentStructure) {
            SetCurrentState(_states[Protect_Begins]);
        } else {
            SetCurrentState(_states[Protect_Fails]);
        }
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetStructure);
    }
    private void ProtectBeginsEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.AddTrait(new PatrollingCharacter(_targetCharacter));
        _targetCharacter.AddTrait("Protected");

    }
    private void ProtectFailsEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
