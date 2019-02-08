using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForageAction : Interaction {
    private const string Start = "Start";
    private const string Forage_Success = "Forage Success";
    private const string Forage_Mild_Success = "Forage Mild Success";
    private const string Forage_Fail = "Forage Fail";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public ForageAction(Area interactable): base(interactable, INTERACTION_TYPE.FORAGE_ACTION, 0) {
        _name = "Forage Action";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState forageSuccess = new InteractionState(Forage_Success, this);
        InteractionState forageMildSuccess = new InteractionState(Forage_Mild_Success, this);
        InteractionState forageFail = new InteractionState(Forage_Fail, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        forageSuccess.SetEffect(() => ForageSuccessEffect(forageSuccess));
        forageMildSuccess.SetEffect(() => ForageMildSuccessEffect(forageMildSuccess));
        forageFail.SetEffect(() => ForageFailEffect(forageFail));

        _states.Add(startState.name, startState);
        _states.Add(forageSuccess.name, forageSuccess);
        _states.Add(forageMildSuccess.name, forageMildSuccess);
        _states.Add(forageFail.name, forageFail);

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
        if (!InteractionManager.Instance.CanCreateInteraction(type, character)) {
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
        WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
        resultWeights.AddElement(Forage_Success, 20);
        resultWeights.AddElement(Forage_Mild_Success, 40);
        resultWeights.AddElement(Forage_Fail, 10);

        string result = resultWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[result]);
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToRandomStructureInArea(STRUCTURE_TYPE.WILDERNESS);
    }
    private void ForageSuccessEffect(InteractionState state) {
        _characterInvolved.ResetFullnessMeter();
    }
    private void ForageMildSuccessEffect(InteractionState state) {
        _characterInvolved.AdjustFullness(80);
    }
    private void ForageFailEffect(InteractionState state) {
    }
    #endregion
}
