using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootAction : Interaction {
    private const string Start = "Start";
    private const string Loot_Found = "Loot Found";
    private const string No_Loot_Found = "No Loot Found";

    public LootAction(Area interactable): base(interactable, INTERACTION_TYPE.LOOT_ACTION, 0) {
        _name = "Loot Action";
        _category = INTERACTION_CATEGORY.INVENTORY;
        _alignment = INTERACTION_ALIGNMENT.EVIL;
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState lootFound = new InteractionState(Loot_Found, this);
        InteractionState noLootFound = new InteractionState(No_Loot_Found, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        lootFound.SetEffect(() => LootFoundEffect(lootFound));
        noLootFound.SetEffect(() => NoLootFoundEffect(noLootFound));

        _states.Add(startState.name, startState);
        _states.Add(lootFound.name, lootFound);
        _states.Add(noLootFound.name, noLootFound);

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
        if(character.isHoldingItem || interactable.possibleSpecialTokenSpawns.Count <= 0) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        if(_characterInvolved.currentStructure.itemsInStructure.Count > 0) {
            SetCurrentState(_states[Loot_Found]);
        } else {
            SetCurrentState(_states[No_Loot_Found]);
        }
    }
    #endregion

    #region State Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToRandomStructureInArea();
    }
    private void LootFoundEffect(InteractionState state) {
        _characterInvolved.LevelUp();

        SpecialToken lootedItem = _characterInvolved.currentStructure.itemsInStructure[UnityEngine.Random.Range(0, _characterInvolved.currentStructure.itemsInStructure.Count)];
        _characterInvolved.ObtainToken(lootedItem);
        _characterInvolved.currentStructure.RemoveItem(lootedItem);

        state.descriptionLog.AddToFillers(lootedItem, lootedItem.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(lootedItem, lootedItem.name, LOG_IDENTIFIER.ITEM_1));
    }
    private void NoLootFoundEffect(InteractionState state) {
    }
    #endregion
}
