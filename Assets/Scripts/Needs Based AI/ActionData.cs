using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ActionData {
    private CharacterParty _party;
    public CharacterAction currentAction;
    public Quest questAssociatedWithCurrentAction { get; private set; }
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
    private bool _cannotPerformAction;
    private float _homeMultiplier;

    public bool isCurrentActionFromEvent { get { return false; }} //TODO: change this to actually be set if a character's action comes from an event.
    public SCHEDULE_PHASE_TYPE currentActionPhaseType { get; private set; }

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
    public bool isNotFirstEncounter {
        get { return _isNotFirstEncounter; }
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
        _cannotPerformAction = false;
#if !WORLD_CREATION_TOOL
        SchedulingManager.Instance.AddEntry(GameManager.Instance.EndOfTheMonth(), () => CheckDoneActionHome());
        Messenger.AddListener<CharacterParty, ObjectState>(Signals.STATE_ENDED, APartyEndedState);
#endif
#if UNITY_EDITOR
        actionHistory = new List<string>();
#endif
    }

    public void Reset() {
        this.currentAction = null;
        this.currentTargetObject = null;
        //this.currentChainAction = null;
        this.currentDay = 0;
        SetIsDone (false);
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
        if (_party == null || _party.isDead) {
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
        if (action.ShouldGoToTargetObjectOnChoose()) {
            if (targetObject != null) {
                _party.GoToLocation(targetObject.specificLocation, PATHFINDING_MODE.PASSABLE);
                if (targetObject.objectType == OBJECT_TYPE.STRUCTURE) {
                    Area areaOfStructure = targetObject.objectLocation.tileLocation.areaOfTile;
                    if (areaOfStructure != null && _party.home != null && areaOfStructure.id == _party.home.id) {
                        _homeMultiplier = 1f;
                        _hasDoneActionAtHome = true;
                    }
                }
            }
        }
        Messenger.Broadcast(Signals.ACTION_TAKEN, action, _party);
    }
    public void DetachActionData() {
        Reset();
        _party.onDailyAction -= PerformCurrentAction;
        _party = null;
        Messenger.RemoveListener<CharacterParty, ObjectState>(Signals.STATE_ENDED, APartyEndedState);
        //Messenger.RemoveListener(Signals.HOUR_ENDED, PerformCurrentAction);
    }

    public void EndAction() {
        Debug.Log("[" + GameManager.Instance.Today().GetDayAndTicksString() +"] Ended " + _party.name + " action " + currentAction.actionData.actionName);
        SetIsDone(true);
    }
    public void SetCurrentActionPhaseType(SCHEDULE_PHASE_TYPE phaseType) {
        currentActionPhaseType = phaseType;
    }
    public void SetCurrentAction(CharacterAction action) {
        this.currentAction = action;
        Debug.Log("Set current action of " + _party.name + " to " + this.currentAction.actionData.actionName);
    }
    public void SetCurrentTargetObject(IObject targetObject) {
        this.currentTargetObject = targetObject;
        if (this.currentTargetObject == null) {
            Debug.Log("Set current target object of " + _party.name + " to null.");
        } else {
            Debug.Log("Set current target object of " + _party.name + " to " + this.currentTargetObject.objectName);
        }
       
    }
    public void SetQuestAssociatedWithAction(Quest quest) {
        questAssociatedWithCurrentAction = quest;
    }
    public void SetCurrentDay(int day) {
        this.currentDay = day;
    }
    public void SetIsHalted(bool state) {
        if (_isHalted != state) {
            _isHalted = state;
            _party.icon.SetMovementState(state);
            //if (!_isHalted){
            //    _party.icon.SetMovementState(state);
            //    _isHalted = state;
            //} else {
            //    _isHalted = state;
            //    _party.icon.SetMovementState(state);
            //}
        }
    }
    public void SetCannotPerformAction(bool state) {
        _cannotPerformAction = state;
    }
    private void AdjustCurrentDay(int amount) {
        this.currentDay += amount;
        Messenger.Broadcast(Signals.ACTION_DAY_ADJUSTED, currentAction, _party);
        if(this.currentDay >= currentAction.actionData.duration) {
            currentAction.DoneDuration(_party, currentTargetObject);
            currentAction.EndAction(_party, currentTargetObject);
        }
    }
    public void SetIsDone(bool state) {
        this.isDone = state;
        Debug.Log("Set is done to " + state.ToString());
    }

    private void PerformCurrentAction() {
        if (!isWaiting && _party.icon.hasArrived) {
            if (!isDone && currentAction != null){
                if (_isHalted || _cannotPerformAction) {
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
                    if (characterLocation != null && currentTargetObject.specificLocation != null 
                        && characterLocation.tileLocation.id == currentTargetObject.specificLocation.tileLocation.id) {
                        //If somehow the object has changed state while the character is on its way to perform action, check if there is an identical action in that state and if so, assign it to this character, if not, character will look for new action
                        //if (currentAction.state.stateName != currentAction.state.obj.currentState.stateName) {
                        CharacterAction newAction = currentTargetObject.currentState.GetActionInState(currentAction);
                        if (newAction != null) {
                            if (newAction != currentAction) {
                                currentAction.EndAction(_party, currentTargetObject);
                                AssignAction(newAction, currentTargetObject);
                            }
                        } else {
                            if (!_party.mainCharacter.miscActions.Contains(currentAction)) {
                                currentAction.EndAction(_party, currentTargetObject);
                                return;
                            }
                        }
                        //}
                        DoAction();
                    } else {
                        ILocation location = currentTargetObject.specificLocation;
                        if (location != null) {
                            //if (currentAction.actionType == ACTION_TYPE.ATTACK || currentAction.actionType == ACTION_TYPE.CHAT) {
                            if (currentAction.actionType == ACTION_TYPE.STALK) {
                                _party.GoToLocation(location, PATHFINDING_MODE.PASSABLE);
                            }
                        } else {
                            if (currentTargetObject.currentState.stateName != "Alive") { //if object is dead
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

    public void LookForAction() {
        isWaiting = true;
        //GameManager.Instance.StartCoroutine(LookForActionCoroutine());
        //for (int i = 0; i < _party.questData.Count; i++) {
        //    CharacterQuestData questData = _party.questData[i];
        //    GameManager.Instance.StartCoroutine(questData.SetupValuesCoroutine());
        //}
        MultiThreadPool.Instance.AddToThreadPool(actionThread);
    }

    //private IEnumerator LookForActionCoroutine() {
    //    List<CharacterQuestData> dataToSetup = new List<CharacterQuestData>(_party.questData);
    //    if (_party.owner is ECS.Character && (_party.owner as ECS.Character).IsSquadLeader()) {
    //        dataToSetup.AddRange(_party.owner.squad.GetSquadQuestData());
    //    } else {
    //        dataToSetup.AddRange(_party.questData);
    //    }

    //    for (int i = 0; i < dataToSetup.Count; i++) {
    //        CharacterQuestData questData = dataToSetup[i];
    //        yield return GameManager.Instance.StartCoroutine(questData.SetupValuesCoroutine());
    //    }
    //    MultiThreadPool.Instance.AddToThreadPool(actionThread);
    //    //Debug.Log(_party.mainCharacter.name + " Look For action coroutine done!");
    //}

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
            currentAction.EndAction(_party, currentTargetObject);
        }
        AssignAction(newAction, targetObject);
        Debug.Log("Forced " + _party.name + " to perform " + newAction.actionData.actionName + " at " + targetObject.objectName);
    }

    private void APartyEndedState(CharacterParty partyThatChangedState, ObjectState stateEnded) {
        if(currentAction != null) {
            currentAction.APartyHasEndedItsState(_party, currentTargetObject, partyThatChangedState, stateEnded);
        }
    }
}
