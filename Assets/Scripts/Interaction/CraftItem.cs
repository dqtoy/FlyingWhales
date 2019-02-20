using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftItem : Interaction {
    private const string Start = "Start";
    private const string Crafting_Cancelled = "Crafting Cancelled";
    private const string Crafting_Successful = "Crafting Successful";
    private const string Crafting_Failed = "Crafting Failed";
    private const string Normal_Crafting_Successful = "Normal Crafting Successful";
    private const string Normal_Crafting_Failed = "Normal Crafting Failed";

    public CraftItem(Area interactable): base(interactable, INTERACTION_TYPE.CRAFT_ITEM, 0) {
        _name = "Craft Item";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState craftingCancelled = new InteractionState(Crafting_Cancelled, this);
        InteractionState craftingSuccessful = new InteractionState(Crafting_Successful, this);
        InteractionState craftingFailed = new InteractionState(Crafting_Failed, this);
        InteractionState normalCraftingSuccessful = new InteractionState(Normal_Crafting_Successful, this);
        InteractionState normalCraftingFailed = new InteractionState(Normal_Crafting_Failed, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        craftingCancelled.SetEffect(() => CraftingCancelledEffect(craftingCancelled));
        craftingSuccessful.SetEffect(() => CraftingSuccessfulEffect(craftingSuccessful));
        craftingFailed.SetEffect(() => CraftingFailedEffect(craftingFailed));
        normalCraftingSuccessful.SetEffect(() => NormalCraftingSuccessfulEffect(normalCraftingSuccessful));
        normalCraftingFailed.SetEffect(() => NormalCraftingFailedEffect(normalCraftingFailed));

        _states.Add(startState.name, startState);
        _states.Add(craftingCancelled.name, craftingCancelled);
        _states.Add(craftingSuccessful.name, craftingSuccessful);
        _states.Add(craftingFailed.name, craftingFailed);
        _states.Add(normalCraftingSuccessful.name, normalCraftingSuccessful);
        _states.Add(normalCraftingFailed.name, normalCraftingFailed);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption preventOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from crafting.",
                duration = 0,
                jobNeeded = JOB.DEBILITATOR,
                disabledTooltipText = "Must be a Dissuader.",
                effect = () => PreventOption(),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOption(),
            };
            state.AddActionOption(preventOption);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (!InteractionManager.Instance.CanCreateInteraction(_type, character)) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void PreventOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Crafting_Cancelled, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement("Fail", investigatorCharacter.job.GetFailRate());
        string result = effectWeights.PickRandomElementGivenWeights();
        if(result == "Fail") {
            effectWeights.Clear();
            effectWeights.AddElement(Crafting_Successful, _characterInvolved.job.GetSuccessRate());
            effectWeights.AddElement(Crafting_Failed, _characterInvolved.job.GetFailRate());
            result = effectWeights.PickRandomElementGivenWeights();
            SetCurrentState(_states[result]);
        } else {
            SetCurrentState(_states[result]);
        }
    }
    private void DoNothingOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Normal_Crafting_Successful, _characterInvolved.job.GetSuccessRate());
        effectWeights.AddElement(Normal_Crafting_Failed, _characterInvolved.job.GetFailRate());
        string result = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[result]);
    }
    #endregion

    #region State Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToRandomStructureInArea(STRUCTURE_TYPE.WORK_AREA);
    }
    private void CraftingCancelledEffect(InteractionState state) {
    }
    private void CraftingSuccessfulEffect(InteractionState state) {
        SpecialToken craftedItem = _characterInvolved.CraftAnItem();
        LocationStructure warehouse = _characterInvolved.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        if(warehouse == null) {
            warehouse = _characterInvolved.currentStructure;
        }
        _characterInvolved.specificLocation.AddSpecialTokenToLocation(craftedItem, warehouse);

        state.descriptionLog.AddToFillers(craftedItem, craftedItem.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(craftedItem, craftedItem.name, LOG_IDENTIFIER.ITEM_1));
    }
    private void CraftingFailedEffect(InteractionState state) {
    }
    private void NormalCraftingSuccessfulEffect(InteractionState state) {
        SpecialToken craftedItem = _characterInvolved.CraftAnItem();
        LocationStructure warehouse = _characterInvolved.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        if (warehouse == null) {
            warehouse = _characterInvolved.currentStructure;
        }
        _characterInvolved.specificLocation.AddSpecialTokenToLocation(craftedItem, warehouse);

        state.descriptionLog.AddToFillers(craftedItem, craftedItem.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(craftedItem, craftedItem.name, LOG_IDENTIFIER.ITEM_1));
    }
    private void NormalCraftingFailedEffect(InteractionState state) {
    }
    #endregion
}
