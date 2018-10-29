using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction {
    protected int _id;
    protected string _name;
    protected INTERACTION_TYPE _type;
    protected IInteractable _interactable;
    protected Dictionary<string, InteractionState> _states;
    protected InteractionState _currentState;
    protected InteractionItem _interactionItem;
    protected bool _isActivated;
    protected bool _isDone;

    #region getters/setters
    public INTERACTION_TYPE type {
        get { return _type; }
    }
    public string name {
        get { return _name; }
    }
    public InteractionState currentState {
        get { return _currentState; }
    }
    public InteractionItem interactionItem {
        get { return _interactionItem; }
    }
    public IInteractable interactable {
        get { return _interactable; }
    }
    public bool isActivated {
        get { return _isActivated; }
    }
    #endregion
    public Interaction(IInteractable interactable, INTERACTION_TYPE type) {
        _id = Utilities.SetID(this);
        _type = type;
        _interactable = interactable;
        _states = new Dictionary<string, InteractionState>();
        CreateStates();
        //Debug.Log("Created new interaction " + type.ToString() + " at " + interactable.name);
    }

    #region Virtuals
    public virtual void CreateStates() { }
    public virtual void CreateActionOptions(InteractionState state) { }
    public virtual void EndInteraction() {
        _isDone = true;
        _interactable.RemoveInteraction(this);
    }
    #endregion

    #region Utilities
    public void SetCurrentState(InteractionState state) {
        if(_currentState != null && _currentState.chosenOption != null) {
            state.SetAssignedMinion(_currentState.chosenOption.assignedMinion);
            _currentState.OnEndState();
        }
        _currentState = state;
        _currentState.OnStartState();
        Messenger.Broadcast(Signals.UPDATED_INTERACTION_STATE, this);
    }
    public void SetActivatedState(bool state) {
        _isActivated = state;
        //if (!state) {
        //    _currentState.SetChosenOption(null);
        //}
        Messenger.Broadcast(Signals.CHANGED_ACTIVATED_STATE, this);
    }
    public void SetInteractionItem(InteractionItem interactionItem) {
        _interactionItem = interactionItem;
    }
    protected int GetRemainingDurationFromState(InteractionState state) {
        return GameManager.Instance.GetTicksDifferenceOfTwoDates(GameManager.Instance.Today(), state.timeDate);
    }
    protected void SetDefaultActionDurationAsRemainingTicks(string optionName, InteractionState stateFrom) {
        ActionOption option = stateFrom.GetOption(optionName);
        int remainingTicks = GameManager.Instance.GetTicksDifferenceOfTwoDates(GameManager.Instance.Today(), stateFrom.timeDate);
        option.duration = remainingTicks;
    }
    #endregion

    #region Shared States and Effects
    protected void LeaveAloneEffect(InteractionState state) {
        state.EndResult();
    }
    protected void SupplyRewardState(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " discovered a small cache of Supplies.");
        SetCurrentState(_states[effectName]);
    }
    protected void SupplyRewardEffect(InteractionState state) {
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, 40);
        state.assignedMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }

    protected void ManaRewardState(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " discovered a source of magical energy. We have converted it into a small amount of Mana.");
        SetCurrentState(_states[effectName]);
    }
    protected void ManaRewardEffect(InteractionState state) {
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.MANA, 40);
        state.assignedMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }

    protected void DemonDisappearsRewardState(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " has not returned. We can only assume the worst.");
        SetCurrentState(_states[effectName]);
    }
    protected void DemonDisappearsRewardEffect(InteractionState state) {
        state.assignedMinion.icharacter.Death();
        PlayerManager.Instance.player.RemoveMinion(state.assignedMinion);
    }

    protected void NothingRewardState(InteractionState state, string effectName) {
        _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " has returned with nothing to report.");
        SetCurrentState(_states[effectName]);
    }
    protected void NothingEffect(InteractionState state) {
        state.assignedMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
    #endregion
}
