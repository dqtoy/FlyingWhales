using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolActionFaction : Interaction {

    private const string Normal_Patrol = "Normal Patrol";

    private LocationStructure structure;

    public override LocationStructure actionStructureLocation {
        get { return structure; }
    }

    public PatrolActionFaction(Area interactable) 
        : base(interactable, INTERACTION_TYPE.PATROL_ACTION_FACTION, 0) {
        _name = "Patrol Action";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState normalPatrol = new InteractionState(Normal_Patrol, this);
        
        CreateActionOptions(startState);
        startState.SetEffect(() => StartEffect(startState), false);
        normalPatrol.SetEffect(() => NormalPatrol(normalPatrol));
        
        _states.Add(startState.name, startState);
        _states.Add(normalPatrol.name, normalPatrol);

        //SetCurrentState(startState);
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
    public override object GetTarget() {
        return structure;
    }
    #endregion

    #region Option Effects
    private void StartEffect(InteractionState state) {
        //**Structure**: Move the character to a random Wilderness
        structure = interactable.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        _characterInvolved.MoveToAnotherStructure(structure);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Normal_Patrol]);
    }
    #endregion

    #region Reward Effects
    private void NormalPatrol(InteractionState state) {
        //**Mechanics**: Add Patrolling state to the character for 15 days.
        _characterInvolved.AddTrait("Patrolling");
    }
    #endregion

}
