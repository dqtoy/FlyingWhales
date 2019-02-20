using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TameBeastAction : Interaction {
    private const string Start = "Start";
    private const string Normal_Tame_Success = "Normal Tame Success";
    private const string Normal_Tame_Fail = "Normal Tame Fail";
    private const string Normal_Tame_Critical_Fail = "Normal Tame Critical Fail";
    private const string Target_Missing = "Target Missing";

    private Character _targetBeast;

    public override Character targetCharacter {
        get { return _targetBeast; }
    }

    public TameBeastAction(Area interactable) : base(interactable, INTERACTION_TYPE.TAME_BEAST_ACTION, 0) {
        _name = "Tame Beast Action";
    }

    #region Override
    public override void CreateStates() {
        if (_targetBeast == null) {
            _targetBeast = GetTargetCharacter(_characterInvolved);
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState normalTameSuccess = new InteractionState(Normal_Tame_Success, this);
        InteractionState normalTameFail = new InteractionState(Normal_Tame_Fail, this);
        InteractionState normalTameCriticalFail = new InteractionState(Normal_Tame_Critical_Fail, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetBeast, _targetBeast.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartStateEffect(startState), false);
        normalTameSuccess.SetEffect(() => NormalTameSuccessEffect(normalTameSuccess));
        normalTameFail.SetEffect(() => NormalTameFailEffect(normalTameFail));
        normalTameCriticalFail.SetEffect(() => NormalTameCriticalFailEffect(normalTameCriticalFail));
        targetMissing.SetEffect(() => TargetMissingEffect(targetMissing));

        _states.Add(startState.name, startState);
        _states.Add(normalTameSuccess.name, normalTameSuccess);
        _states.Add(normalTameFail.name, normalTameFail);
        _states.Add(normalTameCriticalFail.name, normalTameCriticalFail);
        _states.Add(targetMissing.name, targetMissing);

        //SetCurrentState(startState);
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
        if (_targetBeast == null) {
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
        if(_characterInvolved.currentStructure == _targetBeast.currentStructure) {
            int successWeight = 0, failWeight = 0, critFailWeight = 0;
            bool isHungry = false, isStarving = false, isTired = false, isExhausted = false,
                isInjured = false, isSick = false, isLycanthrope = false;

            for (int i = 0; i < _targetBeast.traits.Count; i++) {
                Trait trait = _targetBeast.traits[i];
                if (trait.name == "Hungry") {
                    isHungry = true;
                } else if (trait.name == "Starving") {
                    isStarving = true;
                } else if (trait.name == "Tired") {
                    isTired = true;
                } else if (trait.name == "Exhausted") {
                    isExhausted = true;
                } else if (trait.name == "Injured") {
                    isInjured = true;
                } else if (trait.name == "Sick") {
                    isSick = true;
                } else if (trait.name == "Lycanthrope") {
                    isLycanthrope = true;
                }
            }

            if (_characterInvolved.level > _targetBeast.level) {
                successWeight = 100;
                failWeight = 50;
                critFailWeight = 10;
            } else if (_characterInvolved.level == _targetBeast.level) {
                successWeight = 50;
                failWeight = 50;
                critFailWeight = 20;
            } else if (_characterInvolved.level < _targetBeast.level) {
                successWeight = 20;
                failWeight = 100;
                critFailWeight = 50;
            }

            if (_targetBeast.race == RACE.DRAGON) {
                critFailWeight *= 2;
            }

            if (isHungry) {
                critFailWeight = (int) (critFailWeight * 1.5f);
            } else if (isStarving) {
                critFailWeight *= 3;
            }

            if (isLycanthrope) {
                successWeight *= 0;
            } else {
                if (isTired || isExhausted) {
                    successWeight = (int) (successWeight * 1.5f);
                }
                if (isInjured) {
                    successWeight = (int) (successWeight * 1.5f);
                }
                if (isSick) {
                    successWeight = (int) (successWeight * 1.5f);
                }
            }

            WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
            effectWeights.AddElement(Normal_Tame_Success, successWeight);
            effectWeights.AddElement(Normal_Tame_Fail, failWeight);
            effectWeights.AddElement(Normal_Tame_Critical_Fail, critFailWeight);

            string chosenEffect = effectWeights.PickRandomElementGivenWeights();
            SetCurrentState(_states[chosenEffect]);
        } else {
            SetCurrentState(_states[Target_Missing]);
        }
    }
    #endregion

    #region State Effect
    private void StartStateEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetBeast.currentStructure);
    }
    private void NormalTameSuccessEffect(InteractionState state) {
        //_characterInvolved.LevelUp();

        _targetBeast.ChangeFactionTo(_characterInvolved.faction);
        _targetBeast.MigrateHomeTo(_characterInvolved.homeArea);
        Interaction returnHome = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, interactable);
        _targetBeast.InduceInteraction(returnHome);
        CharacterManager.Instance.CreateNewRelationshipBetween(_characterInvolved, _targetBeast, RELATIONSHIP_TRAIT.SERVANT);

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

        _targetBeast.ResetFullnessMeter();
        _characterInvolved.Death();
    }
    private void TargetMissingEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetBeast, _targetBeast.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetBeast, _targetBeast.name, LOG_IDENTIFIER.TARGET_CHARACTER));
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
