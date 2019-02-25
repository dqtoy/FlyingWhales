using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveCurseAction : Interaction {
    private const string Start = "Start";
    private const string Curse_Removed = "Curse Removed";
    private const string Curse_Transferred = "Curse Transferred";
    private const string Target_Missing = "Target Missing";

    public RemoveCurseAction(Area interactable): base(interactable, INTERACTION_TYPE.REMOVE_CURSE_ACTION, 0) {
        _name = "Remove Curse Action";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState curseRemoved = new InteractionState(Curse_Removed, this);
        InteractionState curseTransferred = new InteractionState(Curse_Transferred, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        curseRemoved.SetEffect(() => CurseRemovedEffect(curseRemoved));
        curseTransferred.SetEffect(() => CurseTransferredEffect(curseTransferred));
        targetMissing.SetEffect(() => TargetMissingEffect(targetMissing));

        _states.Add(startState.name, startState);
        _states.Add(curseRemoved.name, curseRemoved);
        _states.Add(curseTransferred.name, curseTransferred);
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
        CharacterRelationshipData characterRelationshipData = _characterInvolved.GetCharacterRelationshipData(_targetCharacter);
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
        WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();

        if(_characterInvolved.currentStructure == _targetCharacter.currentStructure) {
            resultWeights.AddElement(Curse_Removed, 30);
            resultWeights.AddElement(Curse_Transferred, 5);
            string result = resultWeights.PickRandomElementGivenWeights();
            SetCurrentState(_states[result]);
        } else {
            SetCurrentState(_states[Target_Missing]);
        }
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
         _characterInvolved.MoveToAnotherStructure(_targetStructure);
    }
    private void CurseRemovedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.RemoveTrait("Cursed");
    }
    private void CurseTransferredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.RemoveTrait("Cursed");
        _characterInvolved.AddTrait("Cursed");
    }
    private void TargetMissingEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
