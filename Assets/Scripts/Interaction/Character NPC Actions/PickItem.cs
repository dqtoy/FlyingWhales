using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickItem : Interaction {

    private const string Start = "Start";
    private const string Item_Obtained = "Item Obtained";
    private const string Item_Missing = "Item Missing";

    List<SpecialToken> items = new List<SpecialToken>();

    private SpecialToken targetToken;
    public override LocationStructure actionStructureLocation {
        get { return _targetStructure; }
    }

    public PickItem(Area interactable) : base(interactable, INTERACTION_TYPE.PICK_ITEM, 0) {
        _name = "Pick Item";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState itemObtained = new InteractionState(Item_Obtained, this);
        InteractionState itemMissing = new InteractionState(Item_Missing, this);

        Log descriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), "start_description", this);
        descriptionLog.AddToFillers(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        descriptionLog.AddToFillers(interactable, interactable.name, LOG_IDENTIFIER.LANDMARK_1);
        startState.OverrideDescriptionLog(descriptionLog);

        CreateActionOptions(startState);

        //LoadItemChoices(characterInvolved);
        startState.SetEffect(() => StartRewardEffect(startState), false);
        itemObtained.SetEffect(() => ItemObtainedRewardEffect(itemObtained));
        itemMissing.SetEffect(() => ItemMissingRewardEffect(itemMissing));

        _states.Add(startState.name, startState);
        _states.Add(itemObtained.name, itemObtained);
        _states.Add(itemMissing.name, itemMissing);

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
        if (character.isHoldingItem) {
            return false;
        }
        LoadItemChoices(character);
        if (items.Count <= 0) {
            return false;
        }
        targetToken = items[UnityEngine.Random.Range(0, items.Count)];
        _targetStructure = targetToken.structureLocation;
        targetGridLocation = targetToken.GetNearestUnoccupiedTileFromThis(_targetStructure, _characterInvolved);
        return base.CanInteractionBeDoneBy(character);
    }
    public override bool CanStillDoInteraction(Character character) {
        if (character.isHoldingItem) {
            return false;
        }
        return base.CanStillDoInteraction(character);
    }
    public override object GetTarget() {
        return targetToken;
    }
    #endregion

    private void LoadItemChoices(Character character) {
        items = new List<SpecialToken>();
        for (int i = 0; i < interactable.possibleSpecialTokenSpawns.Count; i++) {
            SpecialToken token = interactable.possibleSpecialTokenSpawns[i];
            if (token.structureLocation.structureType == STRUCTURE_TYPE.WAREHOUSE ||
                (character.isAtHomeStructure && token.structureLocation == character.homeStructure)) {
                items.Add(token);
            }
        }
    }

    #region Option Effects
    private void DoNothingOptionEffect() {
        if (_characterInvolved.currentStructure == targetToken.structureLocation) {
            SetCurrentState(_states[Item_Obtained]);
        } else {
            SetCurrentState(_states[Item_Missing]);
        }
    }
    #endregion

    #region Reward Effect
    private void StartRewardEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetStructure, targetGridLocation, targetToken);
    }
    private void ItemObtainedRewardEffect(InteractionState state) {
        _characterInvolved.PickUpToken(targetToken);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _characterInvolved.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, _characterInvolved.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
    }
    private void ItemMissingRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, targetToken.nameInBold, LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, targetToken.nameInBold, LOG_IDENTIFIER.STRING_1));
    }
    #endregion

}
