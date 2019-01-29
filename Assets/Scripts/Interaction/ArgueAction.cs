﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArgueAction : Interaction {

    private Character _targetCharacter;

    private const string Start = "Start";
    private const string Both_Gets_Annoyed = "Both Gets Annoyed";
    private const string Character_1_Injured = "Character 1 Injured"; //Character 1 = Active Character / Character Involved
    private const string Character_2_Injured = "Character 2 Injured"; //Character 2 = Target Character

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public ArgueAction(Area interactable): base(interactable, INTERACTION_TYPE.ARGUE_ACTION, 0) {
        _name = "Argue Action";
        //_categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.PERSONAL, INTERACTION_CATEGORY.SOCIAL };
        //_alignment = INTERACTION_ALIGNMENT.NEUTRAL;
    }

    #region Override
    public override void CreateStates() {

        InteractionState startState = new InteractionState(Start, this);
        InteractionState bothGetsAnnoyed = new InteractionState(Both_Gets_Annoyed, this);
        InteractionState character1Injured = new InteractionState(Character_1_Injured, this);
        InteractionState character2Injured = new InteractionState(Character_2_Injured, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        bothGetsAnnoyed.SetEffect(() => BothGetsAnnoyedEffect(bothGetsAnnoyed));
        character1Injured.SetEffect(() => Character1InjuredEffect(character1Injured));
        character2Injured.SetEffect(() => Character2InjuredEffect(character2Injured));

        _states.Add(startState.name, startState);
        _states.Add(bothGetsAnnoyed.name, bothGetsAnnoyed);
        _states.Add(character1Injured.name, character1Injured);
        _states.Add(character2Injured.name, character2Injured);

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
        if (_targetCharacter == null || _targetCharacter.isDead || _targetCharacter.specificLocation != character.specificLocation || _targetCharacter.currentStructure != _targetCharacter.homeStructure) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Both_Gets_Annoyed, 20);
        effectWeights.AddElement(Character_1_Injured, 5);
        effectWeights.AddElement(Character_2_Injured, 5);

        string result = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[result]);
    }
    #endregion

    #region Reward Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.currentStructure);
    }
    private void BothGetsAnnoyedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Annoyed"]);
        _targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Annoyed"]);
    }
    private void Character1InjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
        _targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Cheery"]);
    }
    private void Character2InjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Cheery"]);
        _targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    #endregion

    public void SetTargetCharacter(Character targetCharacter) {
        this._targetCharacter = targetCharacter;
    }
}