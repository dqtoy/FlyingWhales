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

    #region getters/setters
    public bool isHalted {
        get { return _isHalted; }
    }
    #endregion  
    public ActionData(Character character) {
        Reset();
        _character = character;
        choices = new CharacterActionAdvertisement[3];
        actionThread = new ActionThread(_character);
        _character.onDailyAction += PerformCurrentAction;
        _isHalted = false;
        //Messenger.AddListener(Signals.DAY_END, PerformCurrentAction);

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
        action.OnChooseAction();
        _character.GoToLocation(action.state.obj.specificLocation, PATHFINDING_MODE.USE_ROADS);

    }
    public void DetachActionData() {
        Reset();
        _character.onDailyAction -= PerformCurrentAction;
        _character = null;
        //Messenger.RemoveListener(Signals.DAY_END, PerformCurrentAction);
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
            EndAction();
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
                if (characterLocation != null && currentAction.state.obj.specificLocation != null) {
                    if(characterLocation.tileLocation.id == currentAction.state.obj.specificLocation.tileLocation.id) {
                        //If somehow the object has changed state while the character is on its way to perform action, check if there is an identical action in that state and if so, assign it to this character, if not, character will look for new action
                        if (currentAction.state.stateName != currentAction.state.obj.currentState.stateName) {
                            CharacterAction newAction = currentAction.state.obj.currentState.GetActionInState(currentAction);
                            if (newAction != null) {
                                AssignAction(newAction);
                            } else {
                                EndAction();
                                return;
                            }
                        }
                        DoAction();
                    }
                } else {
                    if (currentAction.state.obj.specificLocation != null) {
                        if (currentAction.actionType == ACTION_TYPE.ATTACK) {
                            _character.GoToLocation(currentAction.state.obj.specificLocation, PATHFINDING_MODE.USE_ROADS);
                        }
                    } else {
                        if(currentAction.state.obj.currentState.stateName == "Dead") { //if object is dead
                            EndAction();
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
                        //LookForAction();
                    }
                } else {
                    //LookForAction();
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

    //public void LookForAction() {
    //    string actionLog = _character.name + "'s Action Advertisements: ";
    //    for (int i = 0; i < _character.currentRegion.landmarks.Count; i++) {
    //        BaseLandmark landmark = _character.currentRegion.landmarks[i];
    //        for (int j = 0; j < landmark.objects.Count; j++) {
    //            IObject iobject = landmark.objects[j];
    //            if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
    //                for (int k = 0; k < iobject.currentState.actions.Count; k++) {
    //                    CharacterAction action = iobject.currentState.actions[k];
    //                    if (action.MeetsRequirements(_character, landmark)) { //Filter
    //                        float advertisement = action.GetTotalAdvertisementValue(_character);
    //                        actionLog += "\n" + action.actionData.actionName + " = " + advertisement + " (" + iobject.objectName + " at " + iobject.objectLocation.landmarkName + ")";
    //                        PutToChoices(action, advertisement);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    //Debug.Log(actionLog);
    //    if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id == _character.id) {
    //        Debug.Log(actionLog);
    //    }
    //    PickAction();
    //}
    //private void PutToChoices(CharacterAction action, float advertisement) {
    //    if (choices[0].action == null) {
    //        choices[0].Set(action, advertisement);
    //    } else if (choices[1].action == null) {
    //        choices[1].Set(action, advertisement);
    //    } else if (choices[2].action == null) {
    //        choices[2].Set(action, advertisement);
    //    } else {
    //        if (choices[0].advertisement <= choices[1].advertisement && choices[0].advertisement <= choices[2].advertisement) {
    //            if (advertisement > choices[0].advertisement) {
    //                choices[0].Set(action, advertisement);
    //            }
    //        } else if (choices[1].advertisement <= choices[0].advertisement && choices[1].advertisement <= choices[2].advertisement) {
    //            if (advertisement > choices[1].advertisement) {
    //                choices[1].Set(action, advertisement);
    //            }
    //        } else if (choices[2].advertisement <= choices[0].advertisement && choices[2].advertisement <= choices[1].advertisement) {
    //            if (advertisement > choices[2].advertisement) {
    //                choices[2].Set(action, advertisement);
    //            }
    //        }
    //    }
    //}
    //private CharacterAction PickAction() {
    //    int maxChoice = 3;
    //    if (choices[1].action == null) {
    //        maxChoice = 1;
    //    } else if (choices[2].action == null) {
    //        maxChoice = 2;
    //    }
    //    int chosenIndex = Utilities.rng.Next(0, maxChoice);
    //    CharacterAction chosenAction = choices[chosenIndex].action;
    //    AssignAction(chosenAction);
    //    _character.GoToLocation(chosenAction.state.obj.objectLocation, PATHFINDING_MODE.USE_ROADS);
    //    if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id == _character.id) {
    //        Debug.Log("Chosen Action: " + chosenAction.actionData.actionName + " = " + choices[chosenIndex].advertisement + " (" + chosenAction.state.obj.objectName + " at " + chosenAction.state.obj.objectLocation.landmarkName + ")");
    //    }
    //    //Debug.Log("Chosen Action: " + chosenAction.actionData.actionName + " = " + choices[chosenIndex].advertisement + " (" + chosenAction.state.obj.objectName + " at " + chosenAction.state.obj.objectLocation.landmarkName + ")");
    //    return chosenAction;

    //}

}
