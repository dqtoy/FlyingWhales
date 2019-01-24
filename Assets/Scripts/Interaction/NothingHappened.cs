using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NothingHappened : Interaction {

    public NothingHappened(Area interactable) : base(interactable, INTERACTION_TYPE.SUSPICIOUS_SOLDIER_MEETING, 70) {
        _name = "Nothing Happened";
    }

    #region Overrides
    public override void CreateStates() {
        CreateExploreStates();
        InteractionState startState = new InteractionState("Start", this);

        //string startStateDesc = explorerMinion.name + " did not find anything worth reporting.";
        //startState.SetDescription(startStateDesc);
        CreateActionOptions(startState);

        _states.Add(startState.name, startState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption keepLookingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Keep looking.",
                duration = 0,
                effect = () => ExploreContinuesOption(state),
            };
            ActionOption okOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "OK.",
                duration = 0,
                effect = () => ExploreEndsOption(state),
            };

            state.AddActionOption(keepLookingOption);
            state.AddActionOption(okOption);
            state.SetDefaultOption(okOption);
        }
    }
    #endregion

    
}
