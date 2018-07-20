using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ActionData {
    private CharacterParty _party;
    public CharacterAction currentAction;
    public CharacterQuestData questDataAssociatedWithCurrentAction;
    public IObject currentTargetObject;
    //public ChainAction currentChainAction;
    public object specificTarget;
    public int currentDay;
    public bool isDone;
    public bool isWaiting; //is still waiting from other thread?

    private CharacterActionAdvertisement[] choices;
    private ActionThread actionThread;
    private bool _isNotFirstEncounter;
    private bool _isHalted;
    private bool _hasDoneActionAtHome;
    private float _homeMultiplier;

    #if UNITY_EDITOR
    public List<string> actionHistory;
    #endif

    #region getters/setters
    public float homeMultiplier {
        get { return _homeMultiplier; }
    }
    public bool isHalted {
        get { return _isHalted; }
    }
    #endregion

    public ActionData(CharacterParty party) {
        Reset();
        _party = party;
        choices = new CharacterActionAdvertisement[3];
        actionThread = new ActionThread(_party);
        _party.onDailyAction += PerformCurrentAction;
        _homeMultiplier = 1f;
        _hasDoneActionAtHome = false;
        _isHalted = false;
#if !WORLD_CREATION_TOOL
        SchedulingManager.Instance.AddEntry(GameManager.Instance.EndOfTheMonth(), () => CheckDoneActionHome());
#endif
#if UNITY_EDITOR
        actionHistory = new List<string>();
#endif
    }

    public void Reset() {
        this.currentAction = null;
        //this.currentChainAction = null;
        this.currentDay = 0;
        this.isDone = false;
        this.isWaiting = false;
        this._isNotFirstEncounter = false;
    }

    public void SetSpecificTarget(object target) {
        specificTarget = target;
    }
    public void ReturnActionFromThread(CharacterAction characterAction, IObject targetObject, ChainAction chainAction) {
        AssignAction(characterAction, targetObject, chainAction);
    }
    public void AssignAction(CharacterAction action, IObject targetObject, ChainAction chainAction = null) {
        if (_party.isDead) {
            return;
        }
        Reset();
        if (chainAction != null) {
            action = chainAction.action;
        }
        //this.currentChainAction = chainAction;
        SetCurrentAction(action);
        SetCurrentTargetObject(targetObject);
        if (action == null) {
            throw new System.Exception("Action of " + _party.name + " is null!");
        }
        action.OnChooseAction(_party, targetObject);
        if(targetObject != null) {
            _party.GoToLocation(targetObject.specificLocation, PATHFINDING_MODE.USE_ROADS);
            if (targetObject.objectType == OBJECT_TYPE.STRUCTURE) {
                Area areaOfStructure = targetObject.objectLocation.tileLocation.areaOfTile;
                if (areaOfStructure != null && _party.home != null && areaOfStructure.id == _party.home.id) {
                    _homeMultiplier = 1f;
                    _hasDoneActionAtHome = true;
                }
            }
        }
        //if (action.state.obj.icharacterType == ICHARACTER_TYPE.CHARACTERObj) {
        //    CharacterObj characterObj = action.state.obj as CharacterObj;
        //    _party.GoToLocation(characterObj.character.icon.gameObject, PATHFINDING_MODE.USE_ROADS);
        //} else {
        //    _party.GoToLocation(action.state.obj.specificLocation, PATHFINDING_MODE.USE_ROADS);
        //}
    }
    public void DetachActionData() {
        Reset();
        _party.onDailyAction -= PerformCurrentAction;
        _party = null;
        //Messenger.RemoveListener(Signals.HOUR_ENDED, PerformCurrentAction);
    }

    public void EndAction() {
        isDone = true;
    }
    public void SetCurrentAction(CharacterAction action) {
        this.currentAction = action;
    }
    public void SetCurrentTargetObject(IObject targetObject) {
        this.currentTargetObject = targetObject;
    }
    public void SetCurrentDay(int day) {
        this.currentDay = day;
    }
    public void SetIsHalted(bool state) {
        if (_isHalted != state) {
            if (!_isHalted){
                _party.icon.SetMovementState(state);
                _isHalted = state;
            } else {
                _isHalted = state;
                _party.icon.SetMovementState(state);
            }
        }
    }
    private void AdjustCurrentDay(int amount) {
        this.currentDay += amount;
        if(this.currentDay >= currentAction.actionData.duration) {
            currentAction.DoneDuration(_party, currentTargetObject);
            currentAction.EndAction(_party, currentTargetObject);
        }
    }
    public void SetIsDone(bool state) {
        this.isDone = state;
    }

    private void PerformCurrentAction() {
        if (!isWaiting && _party.icon.targetLocation == null) {
            if (!isDone && currentAction != null){
                if (_isHalted) {
                    return;
                }
                //if (!_isNotFirstEncounter) {
                //    DoAction();
                //    return;
                //}
                if(currentTargetObject == null) {
                    DoAction();
                } else {
                    ILocation characterLocation = _party.specificLocation;
                    if (characterLocation != null && currentTargetObject.specificLocation != null && characterLocation.tileLocation.id == currentTargetObject.specificLocation.tileLocation.id) {
                        //If somehow the object has changed state while the character is on its way to perform action, check if there is an identical action in that state and if so, assign it to this character, if not, character will look for new action
                        //if (currentAction.state.stateName != currentAction.state.obj.currentState.stateName) {
                        CharacterAction newAction = currentTargetObject.currentState.GetActionInState(currentAction);
                        if (newAction != null) {
                            if (newAction != currentAction) {
                                currentAction.EndAction(_party, currentTargetObject);
                                AssignAction(newAction, currentTargetObject);
                            }
                        } else {
                            if (!_party.mainCharacter.desperateActions.Contains(currentAction) && !_party.mainCharacter.idleActions.Contains(currentAction)) {
                                currentAction.EndAction(_party, currentTargetObject);
                                return;
                            }
                        }
                        //}
                        DoAction();
                    } else {
                        ILocation location = currentTargetObject.specificLocation;
                        if (location != null) {
                            if (currentAction.actionType == ACTION_TYPE.ATTACK) { //|| currentAction.actionType == ACTION_TYPE.CHAT
                                _party.GoToLocation(location, PATHFINDING_MODE.USE_ROADS);
                            }
                        } else {
                            if (currentTargetObject.currentState.stateName == "Dead") { //if object is dead
                                currentAction.EndAction(_party, currentTargetObject);
                            }
                        }

                    }
                }
                //else {
                //    Debug.Log(_party.name + " can't perform " + currentAction.actionData.actionName + " because he is not in the same location!");
                //}
            } else {
                LookForAction();
                //if (currentChainAction != null && currentChainAction.parentChainAction != null) {
                //    if(currentChainAction.IsPrerequisiteFinished(_party, currentChainAction)) {
                //        if(currentChainAction.parentChainAction.satisfiedPrerequisites.Count > 0) {
                //            AssignAction(currentChainAction.parentChainAction.satisfiedPrerequisites[0].action, currentChainAction.parentChainAction.satisfiedPrerequisites[0]);
                //        } else {
                //            AssignAction(currentChainAction.parentChainAction.action, currentChainAction.parentChainAction);
                //        }
                //    } else {
                //        LookForAction();
                //    }
                //} else {
                //    LookForAction();
                //}
            }
        }
    }
    public void DoAction() {
        if (!_isNotFirstEncounter) {
            currentAction.OnFirstEncounter(_party, currentTargetObject);
            _isNotFirstEncounter = true;
        }
        if (!isDone) {
            currentAction.PerformAction(_party, currentTargetObject);
            if (currentAction.actionData.duration > 0) {
                AdjustCurrentDay(1);
            }
        }
    }

    private void LookForAction() {
        isWaiting = true;
        GameManager.Instance.StartCoroutine(LookForActionCoroutine());
        //for (int i = 0; i < _party.questData.Count; i++) {
        //    CharacterQuestData questData = _party.questData[i];
        //    GameManager.Instance.StartCoroutine(questData.SetupValuesCoroutine());
        //}
        //MultiThreadPool.Instance.AddToThreadPool(actionThread);
    }

    private IEnumerator LookForActionCoroutine() {
        for (int i = 0; i < _party.questData.Count; i++) {
            CharacterQuestData questData = _party.questData[i];
            yield return GameManager.Instance.StartCoroutine(questData.SetupValuesCoroutine());
        }
        MultiThreadPool.Instance.AddToThreadPool(actionThread);
        //Debug.Log(_party.mainCharacter.name + " Look For action coroutine done!");
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

    /*
     NOTE: This is only for testing purposes!
     This will end the character's current action and assign the new action.
         */
    public void ForceDoAction(CharacterAction newAction, IObject targetObject) {
        if (currentAction != null) {
            EndAction();
        }
        AssignAction(newAction, targetObject);
    }
}
