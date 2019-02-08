using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealActionNPC : Interaction {
    private const string Start = "Start";
    private const string Steal_Success = "Steal Success";
    private const string Steal_Failed = "Steal Failed";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public StealActionNPC(Area interactable): base(interactable, INTERACTION_TYPE.STEAL_ACTION_NPC, 0) {
        _name = "Steal Action NPC";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState stealSuccess = new InteractionState(Steal_Success, this);
        InteractionState stealFailed = new InteractionState(Steal_Failed, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        stealSuccess.SetEffect(() => StealSuccessEffect(stealSuccess));
        stealFailed.SetEffect(() => StealFailedEffect(stealFailed));

        _states.Add(startState.name, startState);
        _states.Add(stealSuccess.name, stealSuccess);
        _states.Add(stealFailed.name, stealFailed);

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
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();

        int successRate = _characterInvolved.speed;
        int failRate = _targetCharacter.speed;

        if(_targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER)) {
            successRate = (int) (successRate * 1.5f);
        }
        if (_targetCharacter.GetTrait("Protected") != null) {
            failRate = (int) (failRate * 1.5f);
        }

        effectWeights.AddElement(Steal_Success, successRate);
        effectWeights.AddElement(Steal_Failed, failRate);

        string result = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[result]);
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.currentStructure);
    }
    private void StealSuccessEffect(InteractionState state) {
        SpecialToken item = _targetCharacter.tokenInInventory;

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(item, item.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(item, item.name, LOG_IDENTIFIER.ITEM_1));

        _targetCharacter.UnobtainToken();
        _characterInvolved.ObtainToken(item);
    }
    private void StealFailedEffect(InteractionState state) {
        SpecialToken item = _targetCharacter.tokenInInventory;

        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(item, item.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(item, item.name, LOG_IDENTIFIER.ITEM_1));
    }
    #endregion
}
