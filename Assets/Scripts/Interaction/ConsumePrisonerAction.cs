using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumePrisonerAction : Interaction {
    private const string Start = "Start";
    private const string Character_Consumed = "Character Consumed";
    private const string Target_Missing = "Target Missing";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public ConsumePrisonerAction(Area interactable) : base(interactable, INTERACTION_TYPE.CONSUME_PRISONER_ACTION, 0) {
        _name = "Consume Prisoner Action";
    }

    #region Override
    public override void CreateStates() {
        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState characterConsumed = new InteractionState(Character_Consumed, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        characterConsumed.SetEffect(() => CharacterConsumedEffect(characterConsumed));
        targetMissing.SetEffect(() => TargetMissingEffect(targetMissing));

        _states.Add(startState.name, startState);
        _states.Add(characterConsumed.name, characterConsumed);
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
            SetTargetCharacter(GetTargetCharacter(character));
        }
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
        if (_characterInvolved.currentStructure == _targetCharacter.currentStructure) {
            SetCurrentState(_states[Character_Consumed]);
        } else {
            SetCurrentState(_states[Target_Missing]);
        }
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.currentStructure);
    }
    private void CharacterConsumedEffect(InteractionState state) {
        int supplyObtained = UnityEngine.Random.Range(35, 76);

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(null, supplyObtained.ToString(), LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(null, supplyObtained.ToString(), LOG_IDENTIFIER.STRING_1));

        _characterInvolved.homeArea.AdjustSuppliesInBank(supplyObtained);
        _targetCharacter.Death();
    }
    private void TargetMissingEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion

    private Character GetTargetCharacter(Character characterInvolved) {
        List<Character> characterChoices = new List<Character>();
        List<LocationStructure> insideSettlements = characterInvolved.specificLocation.GetStructuresAtLocation(true);
        for (int i = 0; i < insideSettlements.Count; i++) {
            LocationStructure currStructure = insideSettlements[i];
            for (int j = 0; j < currStructure.charactersHere.Count; j++) {
                Character currCharacter = currStructure.charactersHere[j];
                if (currCharacter.id != characterInvolved.id && currCharacter.GetTraitOr("Abducted", "Restrained") != null) {
                    characterChoices.Add(currCharacter);
                }
            }
        }
        if (characterChoices.Count > 0) {
            return characterChoices[UnityEngine.Random.Range(0, characterChoices.Count)];
        }
        return null;
    }
}
