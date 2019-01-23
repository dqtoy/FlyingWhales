using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootAction : Interaction {
    private const string Start = "Start";
    private const string Normal_Loot_Success = "Normal Loot Success";
    private const string Normal_Loot_Fail = "Normal Loot Fail";
    private const string Normal_Loot_Critical_Fail = "Normal Loot Critical Fail";

    public LootAction(BaseLandmark interactable): base(interactable, INTERACTION_TYPE.LOOT_ACTION, 0) {
        _name = "Loot Action";
        _category = INTERACTION_CATEGORY.INVENTORY;
        _alignment = INTERACTION_ALIGNMENT.EVIL;
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState normalLootSuccess = new InteractionState(Normal_Loot_Success, this);
        InteractionState normalLootFail = new InteractionState(Normal_Loot_Fail, this);
        InteractionState normalLootCriticalFail = new InteractionState(Normal_Loot_Critical_Fail, this);

        CreateActionOptions(startState);

        normalLootSuccess.SetEffect(() => NormalLootSuccessEffect(normalLootSuccess));
        normalLootFail.SetEffect(() => NormalLootFailEffect(normalLootFail));
        normalLootCriticalFail.SetEffect(() => NormalLootCriticalFailEffect(normalLootCriticalFail));

        _states.Add(startState.name, startState);
        _states.Add(normalLootSuccess.name, normalLootSuccess);
        _states.Add(normalLootFail.name, normalLootFail);
        _states.Add(normalLootCriticalFail.name, normalLootCriticalFail);


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
        if(character.isHoldingItem || interactable.tileLocation.areaOfTile.possibleSpecialTokenSpawns.Count <= 0) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Normal_Loot_Success, _characterInvolved.job.GetSuccessRate());
        effectWeights.AddElement(Normal_Loot_Fail, _characterInvolved.job.GetFailRate());
        effectWeights.AddElement(Normal_Loot_Critical_Fail, _characterInvolved.job.GetCritFailRate());

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    #endregion

    #region State Effect
    private void NormalLootSuccessEffect(InteractionState state) {
        _characterInvolved.LevelUp();

        SpecialToken lootedItem = interactable.tileLocation.areaOfTile.possibleSpecialTokenSpawns[UnityEngine.Random.Range(0, interactable.tileLocation.areaOfTile.possibleSpecialTokenSpawns.Count)];
        _characterInvolved.ObtainToken(lootedItem);
        interactable.tileLocation.areaOfTile.RemoveSpecialTokenFromLocation(lootedItem);

        state.descriptionLog.AddToFillers(lootedItem, lootedItem.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(lootedItem, lootedItem.name, LOG_IDENTIFIER.ITEM_1));
    }
    private void NormalLootFailEffect(InteractionState state) {
    }
    private void NormalLootCriticalFailEffect(InteractionState state) {
        //state.descriptionLog.AddToFillers(_characterInvolved, _characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

        //state.AddLogFiller(new LogFiller(_characterInvolved, _characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));

        _characterInvolved.Death();
    }
    #endregion
}
