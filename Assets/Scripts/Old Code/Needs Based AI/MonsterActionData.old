using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MonsterActionData : IActionData {
    private MonsterParty _party;
    private CharacterAction _currentAction;
    private IObject _currentTargetObject;
    private int _currentDay;
    private bool _isDone;
    private bool _isHalted;
    private bool _cannotPerformAction;
    private bool _isNotFirstEncounter;

    public List<string> actionHistory;

    #region getters/setters
    public CharacterAction currentAction {
        get { return _currentAction; }
    }
    public int currentDay {
        get { return _currentDay; }
    }
    public bool isHalted {
        get { return _isHalted; }
    }
    public bool isDoneAction {
        get { return _isDone; }
    }
    #endregion

    public MonsterActionData(MonsterParty party) {
        _party = party;
        Reset();
        _party.onDailyAction += PerformCurrentAction;
        _isHalted = false;
        _cannotPerformAction = false;

        actionHistory = new List<string>();
    }
    public void AssignAction(CharacterAction action, IObject targetObject, Quest associatedQuest = null, GameEvent associatedEvent = null) {
        if (_party == null || _party.isDead) {
            return;
        }
        Reset();
        if(targetObject != null) {
            actionHistory.Add("[" + GameManager.Instance.continuousDays + "]" + action.actionData.actionName + " - " + targetObject.objectName + "\n");
        } else {
            actionHistory.Add("[" + GameManager.Instance.continuousDays + "]" + action.actionData.actionName + "\n");
        }
        _currentAction = action;
        _currentTargetObject = targetObject;

        if (action == null) {
            throw new System.Exception("Action of " + _party.name + " is null!");
        }
        action.OnChooseAction(_party, targetObject);
        if (action.ShouldGoToTargetObjectOnChoose()) {
            if (targetObject != null) {
                _party.GoToLocation(targetObject.specificLocation, PATHFINDING_MODE.PASSABLE);
            }
        }
        Messenger.Broadcast(Signals.ACTION_TAKEN, action, _party.GetBase());
    }
    public void PerformCurrentAction() {
        if (_party.icon.hasArrived) {
            if (!_isDone && _currentAction != null) {
                if (_isHalted || _cannotPerformAction) {
                    return;
                }
                if (_currentTargetObject == null) {
                    DoAction();
                } else {
                    ILocation characterLocation = _party.specificLocation;
                    if (characterLocation != null && _currentTargetObject.specificLocation != null
                        && characterLocation.tileLocation.id == _currentTargetObject.specificLocation.tileLocation.id && characterLocation.tileLocation.landmarkOnTile != null) {
                        //ValidateCurrentAction(); //If somehow the object has changed state while the character is on its way to perform action, check if there is an identical action in that state and if so, assign it to this character, if not, character will look for new action
                        DoAction();
                    } else {
                        ILocation location = _currentTargetObject.specificLocation;
                        if (location != null) {
                            //if (currentAction.actionType == ACTION_TYPE.ATTACK || currentAction.actionType == ACTION_TYPE.CHAT) {
                            if (_currentAction.actionType == ACTION_TYPE.STALK) {
                                _party.GoToLocation(location, PATHFINDING_MODE.PASSABLE);
                            }
                        } else {
                            if (_currentTargetObject.currentState.stateName != "Alive") { //if object is dead
                                _currentAction.EndAction(_party, _currentTargetObject);
                            }
                        }

                    }
                }
            } else {
            }
        }
    }
    public void DoAction() {
        if (!_isNotFirstEncounter) {
            _currentAction.OnFirstEncounter(_party, _currentTargetObject);
            _isNotFirstEncounter = true;
        }
        if (!_isDone) {
            _currentAction.PerformAction(_party, _currentTargetObject);
            if (_currentAction != null && _currentAction.actionData.duration > 0) {
                AdjustCurrentDay(1);
            }
        }
    }
    public void EndAction() {
        Debug.Log("[" + GameManager.Instance.continuousDays + "] Ended " + _party.name + " action " + currentAction.actionData.actionName);
        _isDone = true;
    }
    private void AdjustCurrentDay(int amount) {
        _currentDay += amount;
        Messenger.Broadcast(Signals.ACTION_DAY_ADJUSTED, _currentAction, _party.GetBase());
        if (_currentDay >= _currentAction.actionData.duration) {
            _currentAction.DoneDuration(_party, _currentTargetObject);
            _currentAction.EndAction(_party, _currentTargetObject);
        }
    }

    public void Reset() {
        _currentAction = null;
        _currentTargetObject = null;
        _currentDay = 0;
        _isDone = false;
        _isNotFirstEncounter = false;
    }
    public void DetachActionData() {
        _party.onDailyAction -= PerformCurrentAction;
        Reset();
        _party = null;
    }
    public void SetIsHalted(bool state) {
        if (_isHalted != state) {
            _isHalted = state;
            _party.icon.SetMovementState(state);
        }
    }
}
