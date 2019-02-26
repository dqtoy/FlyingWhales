using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonHouseFood : Interaction {

    private const string Poisoning_Cancelled = "Poisoning Cancelled";
    private const string Character_Poisons = "Character Poisons";

    public PoisonHouseFood(Area interactable)
        : base(interactable, INTERACTION_TYPE.POISON_HOUSE_FOOD, 0) {
        _name = "Poison House Food";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState poisoningCancelled = new InteractionState(Poisoning_Cancelled, this);
        InteractionState characterPoisons = new InteractionState(Character_Poisons, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartRewardEffect(startState), false);
        poisoningCancelled.SetEffect(() => PoisoningCancelledRewardEffect(poisoningCancelled));
        characterPoisons.SetEffect(() => CharacterPoisonsRewardEffect(characterPoisons));

        _states.Add(startState.name, startState);
        _states.Add(poisoningCancelled.name, poisoningCancelled);
        _states.Add(characterPoisons.name, characterPoisons);

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
        if (_targetCharacter.homeArea.id != character.specificLocation.id) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character character) {
        _targetCharacter = character;
        _targetStructure = _targetCharacter.homeStructure;
        targetGridLocation = _targetCharacter.GetNearestUnoccupiedTileFromCharacter(_targetStructure);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        if (characterInvolved.currentStructure.charactersHere.Count == 1) {
            //no other characters
            SetCurrentState(_states[Character_Poisons]);
        } else {
            SetCurrentState(_states[Poisoning_Cancelled]);
        }
        
    }
    #endregion

    #region Reward Effect
    private void StartRewardEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetStructure, _targetCharacter.GetNearestUnoccupiedTileFromCharacter(_targetStructure));
    }
    private void PoisoningCancelledRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void CharacterPoisonsRewardEffect(InteractionState state) {
        //**Mechanics**: Add https://trello.com/c/waFphC2I/1180-poisoned-food Trait to the current Structure
        _targetCharacter.homeStructure.AddTrait("Poisoned Food");
        state.descriptionLog.AddToFillers(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
