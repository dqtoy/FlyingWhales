using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestrainCriminalAction : Interaction {
    private const string Start = "Start";
    private const string Target_Arrested = "Target Arrested";
    private const string Target_Missing = "Target Missing";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public RestrainCriminalAction(Area interactable) : base(interactable, INTERACTION_TYPE.RESTRAIN_CRIMINAL_ACTION, 0) {
        _name = "Restrain Criminal Action";
    }

    #region Override
    public override void CreateStates() {
        if(_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState targetArrested = new InteractionState(Target_Arrested, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        targetArrested.SetEffect(() => TargetArrestedEffect(targetArrested));
        targetMissing.SetEffect(() => TargetMissingEffect(targetMissing));

        _states.Add(startState.name, startState);
        _states.Add(targetArrested.name, targetArrested);
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
            SetTargetCharacter(GetTargetCharacter(character));
        }
        if(_targetCharacter == null) {
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
        if(_characterInvolved.currentStructure == _targetCharacter.currentStructure) {
            SetCurrentState(_states[Target_Arrested]);
        } else {
            SetCurrentState(_states[Target_Missing]);
        }
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.currentStructure, _targetCharacter.GetNearestUnoccupiedTileFromCharacter(_targetCharacter.currentStructure));
    }
    private void TargetArrestedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.AddTrait(new Restrained(), _characterInvolved);
        LocationStructure workArea = _characterInvolved.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
        if(workArea != null) {
            _targetCharacter.MoveToAnotherStructure(workArea);
        }
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
                if (currCharacter.id != characterInvolved.id && currCharacter.faction.id == characterInvolved.faction.id
                    && currCharacter.GetTrait("Criminal") != null && currCharacter.GetTrait("Restrained") == null) {
                    characterChoices.Add(currCharacter);
                }
            }
        }
        if(characterChoices.Count > 0) {
            return characterChoices[UnityEngine.Random.Range(0, characterChoices.Count)];
        }
        return null;
    }
}
