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
    public int needsMinion;
    public Action effect;
    public Action onStartDurationAction;
    public Func<bool> canBeDoneAction;
    public List<ActionOptionNeededObjectChecker> neededObjectsChecker;
    public List<System.Type> neededObjects;
    public List<object> assignedObjects; //NOTE: assigned objects must be accurate with needed objects list, TODO: Change the 2 lists to use a dictionary instead
    public string doesNotMeetRequirementsStr;
    public JOB jobNeeded;

    private int _currentDuration;

    #region getters/setters
    public IUnit assignedUnit {
        get { return GetAssignedObjectOfType(typeof(IUnit)) as IUnit; }
    }
    public Minion assignedMinion {
        get { return GetAssignedObjectOfType(typeof(Minion)) as Minion; }
    }
    public LocationToken assignedLocation {
        get { return GetAssignedObjectOfType(typeof(LocationToken)) as LocationToken; }
    }
    #endregion

    public ActionOption() {
        assignedObjects = new List<object>();
    }

    public void ActivateOption(BaseLandmark interactable) {
        if (interactionState.interaction.isChosen) { //Only those interaction that pops up must have cost, all other interactions are free since they will all default
            PlayerManager.Instance.player.AdjustCurrency(cost.currency, -cost.amount);
        }
        interactionState.interaction.SetActivatedState(true);
        StartDuration();
        interactionState.SetChosenOption(this);
        SetDescription();
    }
    public bool CanBeDone() {
        if(canBeDoneAction != null) {
            if (canBeDoneAction()) {
                if(jobNeeded != JOB.NONE && jobNeeded != interactionState.interaction.explorerMinion.character.job.jobType) {
                    return false;
                }
                if(interactionState.interaction.isChosen) {
                    if(PlayerManager.Instance.player.currencies[cost.currency] >= cost.amount) {
                        return true;
                    }
                } else {
                    return true;
                }
            }
        } else {
            if (jobNeeded != JOB.NONE && jobNeeded != interactionState.interaction.explorerMinion.character.job.jobType) {
                return false;
            }
            if (interactionState.interaction.isChosen) {
                if (PlayerManager.Instance.player.currencies[cost.currency] >= cost.amount) {
                    return true;
                }
            } else {
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
            Messenger.AddListener(Signals.DAY_STARTED, CheckDuration);
        }
    }
    private void CheckDuration() {
        _currentDuration++;
        if(_currentDuration >= duration) {
            interactionState.interaction.SetActivatedState(false);
            Messenger.RemoveListener(Signals.DAY_STARTED, CheckDuration);
            if (effect != null) {
                effect();
            }
        }
    }
    private void SetDescription() {
        if(!string.IsNullOrEmpty(description) && interactionState.interaction == InteractionUI.Instance.interactionItem.interaction) {
            if (description.Contains("%minion%") && assignedMinion != null) {
                description = description.Replace("%minion%", assignedMinion.character.name);
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
            if(obj is Minion) {
                (obj as Minion).SetEnabledState(false);
            }
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

    public bool CanAfford() {
        if (PlayerManager.Instance.player.currencies[cost.currency] >= cost.amount) {
            return true;
        }
        return false;
    }
}