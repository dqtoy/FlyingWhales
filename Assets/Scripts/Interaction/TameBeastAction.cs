using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TameBeastAction : Interaction {
    private const string Start = "Start";
    private const string Normal_Tame_Success = "Normal Tame Success";
    private const string Normal_Tame_Fail = "Normal Tame Fail";
    private const string Normal_Tame_Critical_Fail = "Normal Tame Critical Fail";

    private Character _targetBeast;

    public override Character targetCharacter {
        get { return _targetBeast; }
    }
    private LocationStructure _targetStructure;
    public override LocationStructure actionStructureLocation {
        get { return _targetStructure; }
    }

    public TameBeastAction(Area interactable): base(interactable, INTERACTION_TYPE.TAME_BEAST_ACTION, 0) {
        _name = "Tame Beast Action";
        //_categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT };
        //_alignment = INTERACTION_ALIGNMENT.NEUTRAL;
    }

    #region Override
    public override void CreateStates() {
        if(_targetBeast == null) {
            _targetBeast = GetTargetCharacter(_characterInvolved);
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState normalTameSuccess = new InteractionState(Normal_Tame_Success, this);
        InteractionState normalTameFail = new InteractionState(Normal_Tame_Fail, this);
        InteractionState normalTameCriticalFail = new InteractionState(Normal_Tame_Critical_Fail, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetBeast, _targetBeast.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartStateEffect(startState), false);
        normalTameSuccess.SetEffect(() => NormalTameSuccessEffect(normalTameSuccess));
        normalTameFail.SetEffect(() => NormalTameFailEffect(normalTameFail));
        normalTameCriticalFail.SetEffect(() => NormalTameCriticalFailEffect(normalTameCriticalFail));

        _states.Add(startState.name, startState);
        _states.Add(normalTameSuccess.name, normalTameSuccess);
        _states.Add(normalTameFail.name, normalTameFail);
        _states.Add(normalTameCriticalFail.name, normalTameCriticalFail);


        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if(_targetBeast == null) {
            _targetBeast = GetTargetCharacter(character);
        }
        if (_targetBeast == null || character.homeArea.IsResidentsFull()) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Normal_Tame_Success, _characterInvolved.job.GetSuccessRate());
        effectWeights.AddElement(Normal_Tame_Fail, _characterInvolved.job.GetFailRate());
        effectWeights.AddElement(Normal_Tame_Critical_Fail, _characterInvolved.job.GetCritFailRate());

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    #endregion

    #region State Effect
    private void StartStateEffect(InteractionState state) {
        _targetStructure = _targetBeast.currentStructure;
        _characterInvolved.MoveToAnotherStructure(_targetStructure);
    }
    private void NormalTameSuccessEffect(InteractionState state) {
        //_characterInvolved.LevelUp();

        _targetBeast.ChangeFactionTo(_characterInvolved.faction);
        _targetBeast.MigrateHomeTo(_characterInvolved.homeArea);
        Interaction returnHome = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, interactable);
        _targetBeast.InduceInteraction(returnHome);

        state.descriptionLog.AddToFillers(_targetBeast, _targetBeast.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetBeast, _targetBeast.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalTameFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetBeast, _targetBeast.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetBeast, _targetBeast.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalTameCriticalFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetBeast, _targetBeast.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetBeast, _targetBeast.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        //state.descriptionLog.AddToFillers(_characterInvolved, _characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

        //state.AddLogFiller(new LogFiller(_characterInvolved, _characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));

        _characterInvolved.Death();
    }
    #endregion

    private Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        //Check residents or characters at location for unaligned beast character to tame?
        for (int j = 0; j < interactable.areaResidents.Count; j++) {
            Character resident = interactable.areaResidents[j];
            if (!resident.currentParty.icon.isTravelling && resident.doNotDisturb <= 0 && resident.IsInOwnParty() 
                && resident.specificLocation.id == interactable.id && resident.faction == FactionManager.Instance.neutralFaction
                && resident.role.roleType == CHARACTER_ROLE.BEAST) {
                int weight = 0;
                if(resident.level < characterInvolved.level) {
                    weight += 50;
                }else if (resident.level == characterInvolved.level) {
                    weight += 30;
                }else if (resident.level > characterInvolved.level) {
                    weight += 5;
                }
                characterWeights.AddElement(resident, weight);
            }
        }
        if (characterWeights.GetTotalOfWeights() > 0) {
            return characterWeights.PickRandomElementGivenWeights();
        }
        return null;
    }
}
