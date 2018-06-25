using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ActionData {
    private Character _character;
    public CharacterAction currentAction;
    public ChainAction currentChainAction;
    public object specificTarget;
    public int currentDay;
    public bool isDone;
    public bool isWaiting; //is still waiting from other thread?

    private CharacterActionAdvertisement[] choices;
    private ActionThread actionThread;
    private bool _isNotFirstEncounter;
    private bool _isHalted;
    private float _homeMultiplier;
    private bool _hasDoneActionAtHome;

    #region getters/setters
    public bool isHalted {
        get { return _isHalted; }
    }
    public float homeMultiplier {
        get { return _homeMultiplier; }
    }
    #endregion

    public ActionData(Character character) {
        Reset();
        _character = character;
        choices = new CharacterActionAdvertisement[3];
        actionThread = new ActionThread(_character);
        _character.onDailyAction += PerformCurrentAction;
        _isHalted = false;
        _homeMultiplier = 1f;
        _hasDoneActionAtHome = false;
#if !WORLD_CREATION_TOOL
        //Messenger.AddListener(Signals.HOUR_ENDED, PerformCurrentAction);
        SchedulingManager.Instance.AddEntry(GameManager.Instance.EndOfTheMonth(), () => CheckDoneActionHome());
#endif
    }

    public void Reset() {
        this.currentAction = null;
        this.currentChainAction = null;
        this.currentDay = 0;
        this.isDone = false;
        this.isWaiting = false;
        this._isNotFirstEncounter = false;
    }

    public void SetSpecificTarget(object target) {
        specificTarget = target;
    }

    public void ReturnActionFromThread(CharacterAction characterAction, ChainAction chainAction) {
        AssignAction(characterAction, chainAction);
    }
    public void AssignAction(CharacterAction action, ChainAction chainAction = null) {
        Reset();
        if (chainAction != null) {
            action = chainAction.action;
        }
        this.currentChainAction = chainAction;
        SetCurrentAction(action);
        action.OnChooseAction(_character);
        _character.GoToLocation(action.state.obj.specificLocation, PATHFINDING_MODE.USE_ROADS);
        //if (action.state.obj is CharacterObj) {
        //    CharacterObj characterObj = action.state.obj as CharacterObj;
        //    _character.GoToLocation(characterObj.character.icon.gameObject, PATHFINDING_MODE.USE_ROADS);
        //} else {
        //    _character.GoToLocation(action.state.obj.specificLocation, PATHFINDING_MODE.USE_ROADS);
        //}

        if (action.state.obj.objectType == OBJECT_TYPE.STRUCTURE) {
            Area areaOfStructure = action.state.obj.objectLocation.tileLocation.areaOfTile;
            if (areaOfStructure != null && _character.home != null && areaOfStructure.id == _character.home.id) {
                _homeMultiplier = 1f;
                _hasDoneActionAtHome = true;
            }
        }
    }
    public void DetachActionData() {
        Reset();
        _character.onDailyAction -= PerformCurrentAction;
        _character = null;
        //Messenger.RemoveListener(Signals.HOUR_ENDED, PerformCurrentAction);
    }

    public void EndAction() {
        isDone = true;
    }
    public void SetCurrentAction(CharacterAction action) {
        this.currentAction = action;
    }
    public void SetCurrentDay(int day) {
        this.currentDay = day;
    }
    private void AdjustCurrentDay(int amount) {
        this.currentDay += amount;
        if(this.currentDay >= currentAction.actionData.duration) {
            currentAction.DoneDuration();
            currentAction.EndAction(_character);
        }
    }
    public void SetIsDone(bool state) {
        this.isDone = state;
    }
    public void SetIsHalted(bool state) {
        if (_isHalted != state) {
            _isHalted = state;
            if (state) {
                _character.icon.aiPath.maxSpeed = 0f;
            }
        }
    }

    private void PerformCurrentAction() {
        if (!isWaiting && !_character.isIdle && _character.icon.targetLocation == null) {
            if (!isDone && currentAction != null){
                if (_isHalted) {
                    return;
                }
                ILocation characterLocation = _character.specificLocation;
                if (characterLocation != null && currentAction.state.obj.specificLocation != null && characterLocation.tileLocation.id == currentAction.state.obj.specificLocation.tileLocation.id) {
                    //If somehow the object has changed state while the character is on its way to perform action, check if there is an identical action in that state and if so, assign it to this character, if not, character will look for new action
                    if (currentAction.state.stateName != currentAction.state.obj.currentState.stateName) {
                        CharacterAction newAction = currentAction.state.obj.currentState.GetActionInState(currentAction);
                        if (newAction != null) {
                            AssignAction(newAction);
                        } else {
                            currentAction.EndAction(_character);
                            return;
                        }
                    }
                    DoAction();
                } else {
                    if (currentAction.state.obj.specificLocation != null) {
                        if (currentAction.actionType == ACTION_TYPE.ATTACK) {
                            _character.GoToLocation(currentAction.state.obj.specificLocation, PATHFINDING_MODE.USE_ROADS);
                        }
                    } else {
                        if(currentAction.state.obj.currentState.stateName == "Dead") { //if object is dead
                            currentAction.EndAction(_character);
                        }
                    }
                    
                }
                //else {
                //    Debug.Log(_character.name + " can't perform " + currentAction.actionData.actionName + " because he is not in the same location!");
                //}
            } else {
                if(currentChainAction != null && currentChainAction.parentChainAction != null) {
                    if(currentChainAction.IsPrerequisiteFinished(_character, currentChainAction)) {
                        if(currentChainAction.parentChainAction.satisfiedPrerequisites.Count > 0) {
                            AssignAction(currentChainAction.parentChainAction.satisfiedPrerequisites[0].action, currentChainAction.parentChainAction.satisfiedPrerequisites[0]);
                        } else {
                            AssignAction(currentChainAction.parentChainAction.action, currentChainAction.parentChainAction);
                        }
                    } else {
                        LookForAction();
                    }
                } else {
                    LookForAction();
                }
            }
        }
    }
    public void DoAction() {
        if (!_isNotFirstEncounter) {
            currentAction.OnFirstEncounter(_character);
            _isNotFirstEncounter = true;
        }
        currentAction.PerformAction(_character);
        if (currentAction.actionData.duration > 0) {
            AdjustCurrentDay(1);
        }
    }

    private void LookForAction() {
        isWaiting = true;
        MultiThreadPool.Instance.AddToThreadPool(actionThread);
    }

    //Checks if the character has already done an action in his home settlement
    private void CheckDoneActionHome() {
        if (_hasDoneActionAtHome) {
            _hasDoneActionAtHome = false;
        } else {
            _homeMultiplier += 0.25f;
        }

        GameDate newDate = GameManager.Instance.Today();
        newDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(newDate.month, GameManager.daysInMonth[newDate.month], newDate.year, newDate.hour, () => CheckDoneActionHome());
    }
}
