using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestAction : Interaction {
    private const string Start = "Start";
    private const string Harvest_Successful = "Harvest Successful";

    public HarvestAction(Area interactable) : base(interactable, INTERACTION_TYPE.HARVEST_ACTION, 0) {
        _name = "Harvest Action";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState harvestSuccessful = new InteractionState(Harvest_Successful, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        harvestSuccessful.SetEffect(() => HarvestSuccessfulEffect(harvestSuccessful));

        _states.Add(startState.name, startState);
        _states.Add(harvestSuccessful.name, harvestSuccessful);

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
        SetCurrentState(_states[Harvest_Successful]);
    }
    #endregion

    #region State Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(STRUCTURE_TYPE.WORK_AREA);
    }
    private void HarvestSuccessfulEffect(InteractionState state) {
        int harvestdSupply = UnityEngine.Random.Range(50, 151);
        _characterInvolved.homeArea.AdjustSuppliesInBank(harvestdSupply);

        state.descriptionLog.AddToFillers(null, harvestdSupply.ToString(), LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(null, harvestdSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    #endregion
}
