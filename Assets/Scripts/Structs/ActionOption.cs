using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionOption {
    public InteractionState interactionState;
    public string description;
    public ActionOptionCost cost;
    public int duration;
    public bool needsMinion;
    public Action effect;
    public Minion assignedMinion;

    private int _currentDuration;

    public void ActivateOption(IInteractable interactable) {
        PlayerManager.Instance.player.AdjustCurrency(cost.currency, -cost.amount);
        if(needsMinion) {
            if(assignedMinion != null) {
                assignedMinion.icharacter.currentParty.GoToLocation(interactable.specificLocation, PATHFINDING_MODE.PASSABLE, () => StartDuration());
                interactionState.SetAssignedMinion(assignedMinion);
                interactionState.interaction.SetActivatedState(true);
            } else {
                //Can't go, no minion assigned
            }
        } else {
            interactionState.interaction.SetActivatedState(true);
            StartDuration();
        }
    }
    public bool CanBeDone() {
        if(PlayerManager.Instance.player.currencies[cost.currency] >= cost.amount) {
            return true;
        }
        return false;
    }
    private void StartDuration() {
        _currentDuration = 0;
        Messenger.AddListener(Signals.HOUR_STARTED, CheckDuration);
    }
    private void CheckDuration() {
        _currentDuration++;
        if(_currentDuration >= duration) {
            interactionState.interaction.SetActivatedState(false);
            Messenger.RemoveListener(Signals.HOUR_STARTED, CheckDuration);
            if (effect != null) {
                effect();
            }
        }
    }
}