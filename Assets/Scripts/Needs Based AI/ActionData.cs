using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System;

public class ActionData : IActionData {
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
    private bool _isBeingAssistedByPlayer;
    private float _homeMultiplier;

    public GameEvent eventAssociatedWithAction { get; private set; } //the event that this party's current action is from
    public bool isCurrentActionFromEvent { get { return eventAssociatedWithAction != null; } } //if the eventConnectedWithAction is not null, the current action is from an event
    public SCHEDULE_PHASE_TYPE currentActionPhaseType { get; private set; }

    public List<string> actionHistory;

    #region getters/setters
    public float homeMultiplier {
        get { return _homeMultiplier; }
    }
    public bool isHalted {
        get { return _isHalted; }
    }
    public bool isDoneAction {
        get { return isDone; }
    }
    public bool isNotFirstEncounter {
        get { return _isNotFirstEncounter; }
    }
    public bool isBeingAssistedByPlayer {
        get { return _isBeingAssistedByPlayer; }
    }
    #endregion

    public ActionData(CharacterParty party) {
        _party = party;
        Reset();
        choices = new CharacterActionAdvertisement[3];
        actionThread = new ActionThread(_party);
        _party.onDailyAction += PerformCurrentAction;
        _homeMultiplier = 1f;
        _hasDoneActionAtHome = false;
        _isHalted = false;
        _cannotPerformAction = false;
#if !WORLD_CREATION_TOOL
        SchedulingManager.Instance.AddEntry(GameManager.Instance.EndOfTheMonth(), () => CheckDoneActionHome());
        //Messenger.AddListener<CharacterParty, ObjectState>(Signals.STATE_ENDED, APartyEndedState);
#endif
        actionHistory = new List<string>();
    }

    public void Reset() {
        this.currentAction = null;
        this.currentTargetObject = null;
        //this.currentChainAction = null;
        this.currentDay = 0;
        SetIsDone (false);
        this.isWaiting = false;
        this._isNotFirstEncounter = false;
        SetQuestAssociatedWithAction(null);
        SetEventAssociatedWithAction(null);
        _isBeingAssistedByPlayer = false;
    }

    public void SetSpecificTarget(object target) {
        specificTarget = target;
    }
    public void ReturnActionFromThread(CharacterAction characterAction, IObject targetObject, Quest associatedQuest, GameEvent associatedEvent) {
        AssignAction(characterAction, targetObject, associatedQuest, associatedEvent);
    }
    public void AssignAction(CharacterAction action, IObject targetObject, Quest associatedQuest = null, GameEvent associatedEvent = null) {
        if (_party == null || _party.isDead) {
            return;
        }
        Reset();
        //if (chainAction != null) {
        //    action = chainAction.action;
        //}
        //this.currentChainAction = chainAction;

        actionHistory.Add("[" + GameManager.Instance.Today().GetDayAndTicksString() + "]" + action.actionData.actionName + " - " + targetObject.objectName + "\n");
        SetCurrentAction(action);
        SetCurrentTargetObject(targetObject);
        SetQuestAssociatedWithAction(associatedQuest);
        SetEventAssociatedWithAction(associatedEvent);
        if (action == null) {
            throw new System.Exception("Action of " + _party.name + " is null!");
        }
        action.OnChooseAction(_party, targetObject);
        if (action.ShouldGoToTargetObjectOnChoose()) {
            if (targetObject != null) {
                _party.GoToLocation(targetObject.specificLocation, PATHFINDING_MODE.PASSABLE);
                if (targetObject.objectType == OBJECT_TYPE.STRUCTURE) {
                    Area areaOfStructure = targetObject.objectLocation.tileLocation.areaOfTile;
                    if (areaOfStructure != null && _party.homeLandmark.tileLocation.areaOfTile != null && areaOfStructure.id == _party.homeLandmark.tileLocation.areaOfTile.id) {
                        _homeMultiplier = 1f;
                        _hasDoneActionAtHome = true;
                    }
                }
            }
        }
        Messenger.Broadcast(Signals.ACTION_TAKEN, action, _party.GetBase());
    }
    public void DetachActionData() {
        _party.onDailyAction -= PerformCurrentAction;
        Reset();
        _party = null;
        //Messenger.RemoveListener<CharacterParty, ObjectState>(Signals.STATE_ENDED, APartyEndedState);
        //Messenger.RemoveListener(Signals.HOUR_ENDED, PerformCurrentAction);
    }

    public void EndAction() {
        if (currentAction != null){
            Debug.Log("[" + GameManager.Instance.Today().GetDayAndTicksString() + "] Ended " + _party.name + " action " + currentAction.actionData.actionName);
        }
        SetIsDone(true);
    }
    public void EndCurrentAction() {
        currentAction.EndAction(_party, currentTargetObject);
    }
    public void SetCurrentActionPhaseType(SCHEDULE_PHASE_TYPE phaseType) {
        currentActionPhaseType = phaseType;
    }
    public void SetCurrentAction(CharacterAction action) {
        this.currentAction = action;
        //Debug.Log(GameManager.Instance.TodayLogString() + "Set current action of " + _party.name + " to " + this.currentAction.actionData.actionName);
    }
    public void SetCurrentTargetObject(IObject targetObject) {
        this.currentTargetObject = targetObject;
        //if (this.currentTargetObject == null) {
        //    Debug.Log(GameManager.Instance.TodayLogString() + "Set current target object of " + _party.name + " to null.");
        //} else {
        //    Debug.Log(GameManager.Instance.TodayLogString() + "Set current target object of " + _party.name + " to " + this.currentTargetObject.objectName);
        //}
       
    }
    private void SetQuestAssociatedWithAction(Quest quest) {
        questAssociatedWithCurrentAction = quest;
    }
    private void SetEventAssociatedWithAction(GameEvent gameEvent) {
        eventAssociatedWithAction = gameEvent;
        if (gameEvent != null) {
            SetCurrentActionPhaseType(SCHEDULE_PHASE_TYPE.SPECIAL);
        }
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
    public void SetIsBeingAssisted(bool state) {
        _isBeingAssistedByPlayer = state;
    }
    private void AdjustCurrentDay(int amount) {
        this.currentDay += amount;
        Messenger.Broadcast(Signals.ACTION_DAY_ADJUSTED, currentAction, _party.GetBase());
        if(this.currentDay >= currentAction.actionData.duration) {
            currentAction.DoneDuration(_party, currentTargetObject);
            currentAction.EndAction(_party, currentTargetObject);
        }
    }
    public void SetIsDone(bool state) {
        this.isDone = state;
        //Debug.Log(_party.name + " Set is done to " + state.ToString());
    }

    public void PerformCurrentAction() {
        if (!isWaiting && !_party.icon.isTravelling) {
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
                        //ValidateCurrentAction(); //If somehow the object has changed state while the character is on its way to perform action, check if there is an identical action in that state and if so, assign it to this character, if not, character will look for new action
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
                //DO NOT LOOK FOR ACTION IF MINION OR IF ARMY
                if (_party.owner is CharacterArmyUnit || _party.owner is MonsterArmyUnit) { //TODO: Make this more elegant!
                    return;
                }
                if(_party.owner.minion == null) {
                    LookForAction();
                }
            }
        }
    }
    public void DoAction() {
        if (!_isNotFirstEncounter) {
            currentAction.OnFirstEncounter(_party, currentTargetObject);
            _isNotFirstEncounter = true;
        }
        if (!isDone && currentAction != null) {
            currentAction.PerformAction(_party, currentTargetObject);
            if (currentAction != null && currentAction.actionData.duration > 0) {
                AdjustCurrentDay(1);
            }
        }
    }
    public void LookForAction() {
        if (_cannotPerformAction) {
            return;
        }
        isWaiting = true;
        //GameManager.Instance.StartCoroutine(LookForActionCoroutine());
        //for (int i = 0; i < _party.questData.Count; i++) {
        //    CharacterQuestData questData = _party.questData[i];
        //    GameManager.Instance.StartCoroutine(questData.SetupValuesCoroutine());
        //}
        MultiThreadPool.Instance.AddToThreadPool(actionThread);
    }
    private void ValidateCurrentAction() {
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
     This will end the character's current action and assign the new action.
         */
    public void ForceDoAction(CharacterAction newAction, IObject targetObject) {
        //Debug.Log("Forced " + _party.name + " to perform " + newAction.actionData.actionName + " at " + targetObject.objectName);
        if (currentAction != null) {
            currentAction.EndAction(_party, currentTargetObject);
        }
        AssignAction(newAction, targetObject);
    }
    public void ForceDoAction(QuestAction newAction) {
        if (currentAction != null) {
            currentAction.EndAction(_party, currentTargetObject);
        }
        AssignAction(newAction.action, newAction.targetObject);
        SetQuestAssociatedWithAction(newAction.associatedQuest);
        Debug.Log("Forced " + _party.name + " to perform action " + newAction.action.actionData.actionName + " from quest " + newAction.associatedQuest.name + " at " + newAction.targetObject.objectName);
    }
    public void ForceDoAction(EventAction newAction) {
        if (currentAction != null) {
            currentAction.EndAction(_party, currentTargetObject);
        }
        AssignAction(newAction.action, newAction.targetObject);
        SetEventAssociatedWithAction(newAction.associatedEvent);
        Debug.Log("Forced " + _party.name + " to perform action " + newAction.action.actionData.actionName + " from event " + newAction.associatedEvent.name + " at " + newAction.targetObject.objectName);
    }

    //private void APartyEndedState(CharacterParty partyThatChangedState, ObjectState stateEnded) {
    //    if(currentAction != null) {
    //        currentAction.APartyHasEndedItsState(_party, currentTargetObject, partyThatChangedState, stateEnded);
    //    }
    //}
}
