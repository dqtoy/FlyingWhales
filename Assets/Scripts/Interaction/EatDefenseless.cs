using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatDefenseless : Interaction {

    private Character _targetCharacter;

    private const string Start = "Start";
    private const string Eat_Cancelled = "Eat Cancelled";
    private const string Eat_Continues = "Eat Continues";
    private const string Character_Eaten = "Character Eaten";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }
    private LocationStructure _targetStructure;
    public override LocationStructure targetStructure {
        get { return _targetStructure; }
    }

    public EatDefenseless(Area interactable): base(interactable, INTERACTION_TYPE.EAT_DEFENSELESS, 0) {
        _name = "Eat Defenseless";
    }

    #region Override
    public override void CreateStates() {
        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState eatCancelled = new InteractionState(Eat_Cancelled, this);
        InteractionState eatContinues = new InteractionState(Eat_Continues, this);
        InteractionState characterEaten = new InteractionState(Character_Eaten, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        eatCancelled.SetEffect(() => EatCancelledEffect(eatCancelled));
        eatContinues.SetEffect(() => EatContinuesEffect(eatContinues));
        characterEaten.SetEffect(() => CharacterEatenEffect(characterEaten));

        _states.Add(startState.name, startState);
        _states.Add(eatCancelled.name, eatCancelled);
        _states.Add(eatContinues.name, eatContinues);
        _states.Add(characterEaten.name, characterEaten);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from eating " + _targetCharacter.name + ".",
                effect = () => PreventOptionEffect(),
                jobNeeded = JOB.DEBILITATOR,
                disabledTooltipText = "Minion must be a Dissuader",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(),
            };
            state.AddActionOption(prevent);
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
    private void PreventOptionEffect() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Eat_Cancelled, _characterInvolved.job.GetSuccessRate());
        effectWeights.AddElement(Eat_Continues, _characterInvolved.job.GetFailRate());
        string result = effectWeights.PickRandomElementGivenWeights();

        SetCurrentState(_states[result]);
    }
    private void DoNothingOptionEffect() {
        SetCurrentState(_states[Character_Eaten]);
    }
    #endregion

    #region Reward Effect
    private void StartEffect(InteractionState state) {
        _targetStructure = _targetCharacter.currentStructure;
        _characterInvolved.MoveToAnotherStructure(_targetStructure);
    }
    private void EatCancelledEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //investigatorCharacter.LevelUp();
    }
    private void EatContinuesEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.Death();
    }
    private void CharacterEatenEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.Death();
    }
    #endregion

    public Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < interactable.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.charactersAtLocation[i];
            if (currCharacter.id != characterInvolved.id && !currCharacter.currentParty.icon.isTravelling && currCharacter.IsInOwnParty() && currCharacter.GetTraitOr("Abducted", "Unconscious") != null) {
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
                if (relationships != null && relationships.Count > 0) {
                    for (int j = 0; j < relationships.Count; j++) {
                        if (relationships[j].effect == TRAIT_EFFECT.POSITIVE) {
                            weight -= 70;
                        } else if (relationships[j].effect == TRAIT_EFFECT.NEGATIVE) {
                            weight += 30;
                        }
                    }
                }
                if(weight > 0) {
                    characterWeights.AddElement(currCharacter, weight);
                }
            }
        }
        if(characterWeights.Count > 0) {
            return characterWeights.PickRandomElementGivenWeights();
        }
        return null;
        //throw new System.Exception("Could not find any character to recruit!");
    }
}
