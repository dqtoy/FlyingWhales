using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureAction : Interaction {

    private Character _targetCharacter;

    private const string Start = "Start";
    private const string Target_Tortured_Died = "Target Tortured Died";
    private const string Target_Tortured_Injured = "Target Tortured Injured";
    private const string Target_Tortured_Recruited = "Target Tortured Recruited";
    private const string Target_Missing = "Target Missing";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }
    private LocationStructure _targetStructure;
    public override LocationStructure actionStructureLocation {
        get { return _targetStructure; }
    }

    public TortureAction(Area interactable): base(interactable, INTERACTION_TYPE.TORTURE_ACTION, 0) {
        _name = "Torture Action";
    }

    #region Override
    public override void CreateStates() {
        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState targetTorturedDied = new InteractionState(Target_Tortured_Died, this);
        InteractionState targetTorturedInjured = new InteractionState(Target_Tortured_Injured, this);
        InteractionState targetTorturedRecruited = new InteractionState(Target_Tortured_Recruited, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        targetTorturedDied.SetEffect(() => TargetDiedEffect(targetTorturedDied));
        targetTorturedInjured.SetEffect(() => TargetInjuredEffect(targetTorturedInjured));
        targetTorturedRecruited.SetEffect(() => TargetRecruitedEffect(targetTorturedRecruited));
        targetMissing.SetEffect(() => TargetMissingEffect(targetMissing));

        _states.Add(startState.name, startState);
        _states.Add(targetTorturedDied.name, targetTorturedDied);
        _states.Add(targetTorturedInjured.name, targetTorturedInjured);
        _states.Add(targetTorturedRecruited.name, targetTorturedRecruited);
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
        if(_targetCharacter == null) {
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
            WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
            effectWeights.AddElement(Target_Tortured_Died, 10);
            effectWeights.AddElement(Target_Tortured_Injured, 40);
            effectWeights.AddElement(Target_Tortured_Recruited, 20);

            string result = effectWeights.PickRandomElementGivenWeights();
            SetCurrentState(_states[result]);
        } else {
            SetCurrentState(_states[Target_Missing]);
        }
    }
    #endregion

    #region Reward Effect
    private void StartEffect(InteractionState state) {
        _targetStructure = _targetCharacter.currentStructure;
        _characterInvolved.MoveToAnotherStructure(_targetStructure);
    }
    private void TargetDiedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.Death();
    }
    private void TargetInjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void TargetRecruitedEffect(InteractionState state) {
        //_characterInvolved.LevelUp();

        Abducted abductedTrait = _targetCharacter.GetTrait("Abducted") as Abducted;
        _targetCharacter.RemoveTrait(abductedTrait);
        _targetCharacter.ChangeFactionTo(_characterInvolved.faction);

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter.faction, _targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void TargetMissingEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion

    public Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < interactable.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.charactersAtLocation[i];
            if (currCharacter.id != characterInvolved.id && !currCharacter.currentParty.icon.isTravelling && currCharacter.IsInOwnParty()
                && currCharacter.currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA && currCharacter.GetTrait("Abducted") != null) {
                int weight = 0;
                if (currCharacter.faction == FactionManager.Instance.neutralFaction) {
                    weight += 80;
                } else {
                    if (currCharacter.faction != characterInvolved.faction) {
                        weight += 30;
                    } else {
                        weight += 15;
                    }
                }
                List<RelationshipTrait> relationships = characterInvolved.GetAllRelationshipTraitWith(currCharacter);
                if(relationships != null && relationships.Count > 0) {
                    for (int j = 0; j < relationships.Count; j++) {
                        if (relationships[j].effect == TRAIT_EFFECT.POSITIVE) {
                            weight -= 70;
                        } else if (relationships[j].effect == TRAIT_EFFECT.NEGATIVE) {
                            weight += 30;
                        }
                    }
                }
                if (weight > 0) {
                    characterWeights.AddElement(currCharacter, weight);
                }
            }
        }
        if (characterWeights.Count > 0) {
            return characterWeights.PickRandomElementGivenWeights();
        }
        return null;
        //throw new System.Exception("Could not find any character to recruit!");
    }
}
