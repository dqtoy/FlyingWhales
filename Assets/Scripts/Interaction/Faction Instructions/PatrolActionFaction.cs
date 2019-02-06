using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolActionFaction : Interaction {

    private const string Patrol_Cancelled = "Patrol Cancelled";
    private const string Patrol_Continues = "Patrol Continues";
    private const string Normal_Patrol = "Normal Patrol";

    private LocationStructure structure;

    public override LocationStructure targetStructure {
        get { return structure; }
    }

    public PatrolActionFaction(Area interactable) 
        : base(interactable, INTERACTION_TYPE.PATROL_ACTION_FACTION, 0) {
        _name = "Patrol Action";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DEBILITATOR };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState patrolCancelled = new InteractionState(Patrol_Cancelled, this);
        InteractionState patrolContinues = new InteractionState(Patrol_Continues, this);
        InteractionState normalPatrol = new InteractionState(Normal_Patrol, this);

        //**Structure**: Move the character to a random Wilderness
        structure = interactable.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        _characterInvolved.MoveToAnotherStructure(structure);

        CreateActionOptions(startState);
        patrolCancelled.SetEffect(() => PatrolCancelled(patrolCancelled));
        patrolContinues.SetEffect(() => PatrolContinues(patrolContinues));
        normalPatrol.SetEffect(() => NormalPatrol(normalPatrol));
        

        _states.Add(startState.name, startState);
        _states.Add(patrolCancelled.name, patrolCancelled);
        _states.Add(patrolContinues.name, patrolContinues);
        _states.Add(normalPatrol.name, normalPatrol);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) +" from leaving.",
                effect = () => PreventOptionEffect(state),
                jobNeeded = JOB.DEBILITATOR,
                doesNotMeetRequirementsStr = "Must have dissuader minion."
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(prevent);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override object GetTarget() {
        return structure;
    }
    #endregion

    #region Option Effects
    private void PreventOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> dissuaderSuccessRate = investigatorCharacter.job.GetJobRateWeights();
        dissuaderSuccessRate.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (dissuaderSuccessRate.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Patrol_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Patrol_Continues;
                break;
        }

        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Normal_Patrol]);
    }
    #endregion

    #region Reward Effects
    private void PatrolCancelled(InteractionState state) {
        //**Level Up**: Dissuader Minion +1
        investigatorCharacter.LevelUp();

    }
    private void PatrolContinues(InteractionState state) {
        //**Mechanics**: Add Patrolling state to the character for 15 days.
        _characterInvolved.AddTrait("Patrolling");
    }
    private void NormalPatrol(InteractionState state) {
        //**Mechanics**: Add Patrolling state to the character for 15 days.
        _characterInvolved.AddTrait("Patrolling");
    }
    #endregion

}
