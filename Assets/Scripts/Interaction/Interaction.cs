using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction {
    protected int _id;
    protected INTERACTION_TYPE _type;
    protected IInteractable _interactable;
    protected Dictionary<string, InteractionState> _states;
    protected InteractionState _currentState;
    protected bool _isActivated;
    protected bool _isDone;

    #region getters/setters
    public InteractionState currentState {
        get { return _currentState; }
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
        }
        _currentState = state;
        _currentState.OnSetAsCurrentState();
        Messenger.Broadcast(Signals.UPDATED_INTERACTION_STATE, this);
    }
    public void SetActivatedState(bool state) {
        _isActivated = state;
        //if (!state) {
        //    _currentState.SetChosenOption(null);
        //}
        Messenger.Broadcast(Signals.CHANGED_ACTIVATED_STATE, this);
    }
    #endregion
}
