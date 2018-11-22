using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionOption {
    public InteractionState interactionState;
    public string name;
    public string description;
    public CurrenyCost cost;
    public int duration;
    public bool needsMinion;
    public Action effect;
    public Action onStartDurationAction;
    public Func<bool> canBeDoneAction;
    public List<System.Type> neededObjects;
    public List<object> assignedObjects; //NOTE: assigned objects must be accurate with needed objects list, TODO: Change the 2 lists to use a dictionary instead

    private int _currentDuration;

    #region getters/setters
    public IUnit assignedUnit {
        get { return GetAssignedObjectOfType(typeof(IUnit)) as IUnit; }
    }
    public Minion assignedMinion {
        get { return GetAssignedObjectOfType(typeof(Minion)) as Minion; }
    }
    public LocationIntel assignedLocation {
        get { return GetAssignedObjectOfType(typeof(LocationIntel)) as LocationIntel; }
    }
    #endregion

    public ActionOption() {
        assignedObjects = new List<object>();
    }

    public void ActivateOption(IInteractable interactable) {
        PlayerManager.Instance.player.AdjustCurrency(cost.currency, -cost.amount);
        //Remove needsMinion, handle needed objects and assigned objects properly, transfer this needsMinion logic to assignedObjects
        if(needsMinion) {
            Minion minion = assignedMinion;
            if(minion != null) {
                minion.GoToAssignment(interactable);
                StartDuration();
                //assignedMinion.icharacter.currentParty.GoToLocation(interactable.specificLocation, PATHFINDING_MODE.PASSABLE, () => StartDuration());
                interactionState.interaction.SetActivatedState(true);
            } else {
                //Can't go, no minion assigned
                Debug.LogWarning("Can't go, no minion assigned");
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
        if(duration == 0) {
            interactionState.interaction.SetActivatedState(false);
            if (effect != null) {
                effect();
            }
        } else {
            Messenger.AddListener(Signals.HOUR_STARTED, CheckDuration);
        }
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
        if(!string.IsNullOrEmpty(description) && interactionState.interaction == InteractionUI.Instance.interactionItem.interaction) {
            if (description.Contains("%minion%") && assignedMinion != null) {
                description = description.Replace("%minion%", assignedMinion.icharacter.name);
            }
            InteractionUI.Instance.interactionItem.SetDescription(description, null);
        }
    }

    public void ClearAssignedObjects() {
        assignedObjects.Clear();
        InteractionUI.Instance.interactionItem.ClearAssignedObjects();
    }

    public void AddAssignedObject(object obj) {
        if (obj != null) {
            int index = assignedObjects.Count;
            assignedObjects.Add(obj);
            interactionState.SetAssignedObjects(assignedObjects);
            InteractionUI.Instance.interactionItem.AddAssignedObject(obj);
            string summary = "Assigned object to option " + obj.GetType().ToString() + " at index " + index.ToString();
            summary += "\nNeeded object types are ";
            for (int i = 0; i < neededObjects.Count; i++) {
                summary += neededObjects[i].ToString() + ", ";
            }
            Debug.Log(summary);
        }
    }

    public object GetAssignedObjectOfType(System.Type type) {
        for (int i = 0; i < assignedObjects.Count; i++) {
            object currObject = assignedObjects[i];
            if (type == typeof(IUnit)) {
                if (currObject is IUnit) { //TODO: Make this more elegant!
                    return currObject;
                }
            } else {
                if (currObject.GetType() == type) {
                    return currObject;
                }
            }
        }
        return null;
    }
}