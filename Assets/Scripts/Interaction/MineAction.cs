using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineAction : Interaction {
    private const string Start = "Start";
    private const string Mine_Successful = "Mine Successful";

    public MineAction(Area interactable) : base(interactable, INTERACTION_TYPE.MINE_ACTION, 0) {
        _name = "Mine Action";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState mineSuccessful = new InteractionState(Mine_Successful, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        mineSuccessful.SetEffect(() => MineSuccessfulEffect(mineSuccessful));

        _states.Add(startState.name, startState);
        _states.Add(mineSuccessful.name, mineSuccessful);

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
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Mine_Successful]);
    }
    #endregion

    #region State Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToRandomStructureInArea(STRUCTURE_TYPE.WORK_AREA);
    }
    private void MineSuccessfulEffect(InteractionState state) {
        int minedSupply = UnityEngine.Random.Range(50, 151);
        _characterInvolved.homeArea.AdjustSuppliesInBank(minedSupply);

        state.descriptionLog.AddToFillers(null, minedSupply.ToString(), LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(null, minedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    #endregion
}
