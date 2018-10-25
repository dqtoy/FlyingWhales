using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionOption {
    public InteractionState interactionState;
    public string name;
    public string description;
    public ActionOptionCost cost;
    public int duration;
    public bool needsMinion;
    public Action effect;
    public Action onStartDurationAction;
    public Func<bool> canBeDoneAction;
    public Minion assignedMinion;
    public List<System.Type> neededObjects;

    private int _currentDuration;

    public void ActivateOption(IInteractable interactable) {
        PlayerManager.Instance.player.AdjustCurrency(cost.currency, -cost.amount);
        if(needsMinion) {
            if(assignedMinion != null) {
                assignedMinion.GoToAssignment(interactable);
                StartDuration();
                //assignedMinion.icharacter.currentParty.GoToLocation(interactable.specificLocation, PATHFINDING_MODE.PASSABLE, () => StartDuration());
                interactionState.interaction.SetActivatedState(true);
            } else {
                //Can't go, no minion assigned
            }
        } else {
            interactionState.interaction.SetActivatedState(true);
            StartDuration();
        }
        interactionState.SetChosenOption(this);
        SetDescription();
    }
    public bool CanBeDone() {
        if(canBeDoneAction != null) {
            if (canBeDoneAction() && PlayerManager.Instance.player.currencies[cost.currency] >= cost.amount) {
                return true;
            }
        } else {
            if (PlayerManager.Instance.player.currencies[cost.currency] >= cost.amount) {
                return true;
            }
        }
        return false;
    }
    private void StartDuration() {
        _currentDuration = 0;
        if (onStartDurationAction != null) {
            onStartDurationAction();
        }
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
    private void SetDescription() {
        if(!string.IsNullOrEmpty(description) && interactionState.interaction.interactionItem != null) {
            if (description.Contains("%minion%") && assignedMinion != null) {
                description = description.Replace("%minion%", assignedMinion.icharacter.name);
            }
            interactionState.interaction.interactionItem.SetDescription(description);
        }
    }
}