using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction {
    protected int _id;
    protected IInteractable _interactable;
    protected Dictionary<string, InteractionState> _states;
    protected InteractionState _currentState;

    #region getters/setters
    public InteractionState currentState {
        get { return _currentState; }
    }
    public IInteractable interactable {
        get { return _interactable; }
    }
    #endregion
    public Interaction(IInteractable interactable) {
        _id = Utilities.SetID(this);
        _interactable = interactable;
        _states = new Dictionary<string, InteractionState>();
        CreateStates();
    }

    #region Virtuals
    public virtual void CreateStates() { }
    public virtual void CreateActionOptions(InteractionState state) { }
    #endregion

    #region Utilities
    public void SetCurrentState(InteractionState state) {
        _currentState = state;
        Messenger.Broadcast(Signals.UPDATED_INTERACTION_STATE, this);
    }
    #endregion
}
