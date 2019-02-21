using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftItem : Interaction {
    private const string Start = "Start";
    private const string Gift_Success = "Gift Success";
    private const string Gift_Fail = "Gift Fail";
    private const string Target_Missing = "Target Missing";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public GiftItem(Area interactable) : base(interactable, INTERACTION_TYPE.GIFT_ITEM, 0) {
        _name = "Gift Item";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState giftSuccess = new InteractionState(Gift_Success, this);
        InteractionState giftFail = new InteractionState(Gift_Fail, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        giftSuccess.SetEffect(() => GiftSuccessEffect(giftSuccess));
        giftFail.SetEffect(() => GiftFailEffect(giftFail));
        targetMissing.SetEffect(() => TargetMissingEffect(targetMissing));

        _states.Add(startState.name, startState);
        _states.Add(giftSuccess.name, giftSuccess);
        _states.Add(giftFail.name, giftFail);
        _states.Add(targetMissing.name, targetMissing);

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
        if (_targetCharacter == null) {
            return false;
        }
        if (!character.isHoldingItem) {
            Debug.LogError(character.name + " CAN'T GIFT ITEM TO " + _targetCharacter.name + " BECAUSE HE/SHE IS NOT HOLDING AN ITEM!");
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
            WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
            resultWeights.AddElement(Gift_Success, _characterInvolved.job.GetSuccessRate());
            resultWeights.AddElement(Gift_Fail, _characterInvolved.job.GetFailRate());
            string result = resultWeights.PickRandomElementGivenWeights();
            SetCurrentState(_states[result]);
        } else {
            SetCurrentState(_states[Target_Missing]);
        }
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.homeStructure, _targetCharacter.GetNearestUnoccupiedTileFromCharacter(_targetCharacter.homeStructure));
    }
    private void GiftSuccessEffect(InteractionState state) {
        SpecialToken item = _characterInvolved.tokenInInventory;

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(item, item.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(item, item.name, LOG_IDENTIFIER.ITEM_1));

        _characterInvolved.UnobtainToken();
        _characterInvolved.specificLocation.AddSpecialTokenToLocation(item);

        if (_characterInvolved.faction.id != _targetCharacter.faction.id && !_characterInvolved.isFactionless && !_targetCharacter.isFactionless) {
            AdjustFactionsRelationship(_characterInvolved.faction, _targetCharacter.faction, 1, state);
        } else {
            throw new System.Exception("CAN'T DO GIFT ITEM: " + _characterInvolved.name + " of " + _characterInvolved.faction.name + " to " + _targetCharacter.name + " of " + _targetCharacter.faction.name);
        }
    }
    private void GiftFailEffect(InteractionState state) {
        SpecialToken item = _characterInvolved.tokenInInventory;

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(item, item.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(item, item.name, LOG_IDENTIFIER.ITEM_1));
    }
    private void TargetMissingEffect(InteractionState state) {
        SpecialToken item = _characterInvolved.tokenInInventory;

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(item, item.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(item, item.name, LOG_IDENTIFIER.ITEM_1));
    }
    #endregion
}
