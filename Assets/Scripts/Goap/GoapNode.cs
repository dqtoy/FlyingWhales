using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapNode {
    public GoapNode parent;
    public int runningCost;
    public int index;
    public GoapAction action;

    public GoapNode(GoapNode parent, int runningCost, GoapAction action) {
        this.parent = parent;
        this.runningCost = runningCost;
        this.action = action;
    }
}

//actual nodes located in a finished plan that is going to be executed by a character
public class ActualGoapNode {
    public IPointOfInterest poiTarget { get; private set; }
    public AlterEgoData poiTargetAlterEgo { get; private set; } //The alter ego the target was using while doing this action. only occupied if target is a character
    public Character actor { get; private set; }
    public AlterEgoData actorAlterEgo { get; private set; } //The alter ego the character was using while doing this action.
    public object[] otherData { get; private set; }

    public GoapAction action { get; private set; }
    public ACTION_STATUS actionStatus { get; private set; }
    public Log thoughtBubbleLog { get; private set; } //used if the current state of this action has a duration
    public Log thoughtBubbleMovingLog { get; private set; } //used when the actor is moving with this as his/her current action
    public LocationStructure targetStructure { get; private set; }

    public string currentStateName { get; private set; }
    public int currentStateDuration { get; private set; }

    public ActualGoapNode(GoapAction action, Character actor, IPointOfInterest poiTarget, object[] otherData) {
        this.action = action;
        this.actor = actor;
        this.poiTarget = poiTarget;
        this.otherData = otherData;
        CreateThoughtBubbleLog();
        Messenger.AddListener<GoapAction, string, Character, IPointOfInterest, object[]>(Signals.ACTION_STATE_SET, OnActionStateSet);
    }

    public void DestroyNode() {
        Messenger.RemoveListener<GoapAction, string, Character, IPointOfInterest, object[]>(Signals.ACTION_STATE_SET, OnActionStateSet);

    }

    #region Action
    public virtual void DoAction() {
        actionStatus = ACTION_STATUS.STARTED;
        actor.SetCurrentAction(this);
        //parentPlan?.SetPlanState(GOAP_PLAN_STATE.IN_PROGRESS);
        Messenger.Broadcast(Signals.CHARACTER_DOING_ACTION, actor, this);
        actor.marker.UpdateActionIcon();
        poiTarget.AddTargettedByAction(this);

        //Move To Do Action
        actor.marker.pathfindingAI.ResetEndReachedDistance();
        MoveToDoAction();
    }
    private void MoveToDoAction() {
        if (targetStructure == null) {
            targetStructure = action.GetTargetStructure(actor, poiTarget, otherData);
            if (targetStructure == null) { throw new System.Exception(actor.name + " target structure of action " + action.goapName + " is null."); }
        }
        if (actor.specificLocation != targetStructure.location) {
            actor.currentParty.GoToLocation(targetStructure.location.region, PATHFINDING_MODE.NORMAL, doneAction: MoveToDoAction);
        } else {
            if (action.actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET) {
                actor.marker.GoTo(poiTarget, OnArriveAtTargetLocation);
            } else if (action.actionLocationType == ACTION_LOCATION_TYPE.IN_PLACE) {
                actor.PerformGoapAction();
            } else if (action.actionLocationType == ACTION_LOCATION_TYPE.NEARBY) {
                List<LocationGridTile> choices = actor.specificLocation.areaMap.GetTilesInRadius(actor.gridTileLocation, 3);
                if (choices.Count > 0) {
                    actor.marker.GoTo(choices[Utilities.rng.Next(0, choices.Count)], OnArriveAtTargetLocation);
                } else {
                    actor.PerformGoapAction();
                }
            } else if (action.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION) {
                List<LocationGridTile> choices = targetStructure.unoccupiedTiles;
                if (choices.Count > 0) {
                    actor.marker.GoTo(choices[Utilities.rng.Next(0, choices.Count)], OnArriveAtTargetLocation);
                } else {
                    throw new System.Exception(actor.name + " target tile of action " + action.goapName + " for " + action.actionLocationType.ToString() + " is null.");
                }
            } else if (action.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION_B) {
                List<LocationGridTile> choices = targetStructure.unoccupiedTiles.Where(x => x.UnoccupiedNeighbours.Count > 0).ToList();
                if (choices.Count > 0) {
                    actor.marker.GoTo(choices[Utilities.rng.Next(0, choices.Count)], OnArriveAtTargetLocation);
                } else {
                    throw new System.Exception(actor.name + " target tile of action " + action.goapName + " for " + action.actionLocationType.ToString() + " is null.");
                }
            } else if (action.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION_B) {
                List<LocationGridTile> choices = targetStructure.unoccupiedTiles.Where(x => x.UnoccupiedNeighbours.Count > 0).ToList();
                if (choices.Count > 0) {
                    actor.marker.GoTo(choices[Utilities.rng.Next(0, choices.Count)], OnArriveAtTargetLocation);
                } else {
                    throw new System.Exception(actor.name + " target tile of action " + action.goapName + " for " + action.actionLocationType.ToString() + " is null.");
                }
            } else if (action.actionLocationType == ACTION_LOCATION_TYPE.TARGET_IN_VISION) {
                if (actor.marker.inVisionPOIs.Contains(poiTarget)) {
                    actor.PerformGoapAction();
                } else {
                    actor.marker.GoTo(poiTarget);
                }
            }
        }
    }
    private void OnArriveAtTargetLocation() {
        actor.PerformGoapAction();
    }
    public void PerformAction() {
        actionStatus = ACTION_STATUS.PERFORMING;
        actorAlterEgo = actor.currentAlterEgo;
        if(poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character targetCharacter = poiTarget as Character;
            poiTargetAlterEgo = targetCharacter.currentAlterEgo;
            if (!action.doesNotStopTargetCharacter && actor != poiTarget && !targetCharacter.isDead) {
                if (targetCharacter.currentParty.icon.isTravelling) {
                    if (targetCharacter.currentParty.icon.travelLine == null) {
                        //This means that the actor currently travelling to another tile in tilemap
                        targetCharacter.marker.StopMovement();
                    } 
                    //else {
                    //    //This means that the actor is currently travelling to another area
                    //    targetCharacter.currentParty.icon.SetOnArriveAction(() => targetCharacter.OnArriveAtAreaStopMovement());
                    //}
                }
                if (targetCharacter.currentAction != null) {
                    targetCharacter.StopCurrentAction(false);
                }
                //if (targetCharacter.stateComponent.currentState != null) {
                //    targetCharacter.stateComponent.currentState.PauseState();
                //} else if (targetCharacter.stateComponent.stateToDo != null) {
                //    targetCharacter.stateComponent.SetStateToDo(null, false, false);
                //}
                targetCharacter.marker.pathfindingAI.AdjustDoNotMove(1);
                targetCharacter.AdjustIsStoppedByOtherCharacter(1);
                targetCharacter.FaceTarget(actor);
            }
        }
        action.Perform(actor, poiTarget, otherData);
    }
    #endregion

    #region Action State
    private void OnActionStateSet(GoapAction action, string stateName, Character actor, IPointOfInterest target, object[] otherData) {
        if (this.actor == actor && poiTarget == target && this.action == action) {
            currentStateName = stateName;
            OnPerformActualActionToTarget();
            ExecuteCurrentActionState();
        }
    }
    private void ExecuteCurrentActionState() {
        GoapActionState currentState = action.states[currentStateName];
        currentState.preEffect?.Invoke(actor, poiTarget, otherData);
        //parentAction.SetExecutionDate(GameManager.Instance.Today());

        if (currentState.duration > 0) {
            currentStateDuration = 0;
            StartPerTickEffect();
        } else if (currentState.duration != -1) {
            EndPerTickEffect();
        }
    }
    private void StartPerTickEffect() {
        Messenger.AddListener(Signals.TICK_STARTED, PerTickEffect);
    }
    public void StopPerTickEffect() {
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickEffect);
    }
    public void EndPerTickEffect(bool shouldDoAfterEffect = true) {
        //if (isDone) {
        //    return;
        //}
        //isDone = true;
        parentAction.ReturnToActorTheActionResult(status);
        if (shouldDoAfterEffect) {
            if (afterEffect != null) {
                afterEffect();
            }
            if (parentAction.shouldAddLogs && this.shouldAddLogs) { //only add logs if both the parent action and this state should add logs
                if (descriptionLog != null) {
                    AddArrangedLog("description", descriptionLog, null);
                }
                for (int i = 0; i < arrangedLogs.Count; i++) {
                    arrangedLogs[i].log.SetDate(GameManager.Instance.Today());
                    arrangedLogs[i].log.AddLogToInvolvedObjects();
                }
            }
        } else {
            parentAction.SetShowIntelNotification(false);
        }
        parentAction.actor.OnCharacterDoAction(parentAction); //Moved this here to fix intel not being shown, because arranged logs are not added until after the ReturnToActorTheActionResult() call.
        if (shouldDoAfterEffect) {
            parentAction.AfterAfterEffect();
        }
    }
    private void PerTickEffect() {
        GoapActionState currentState = action.states[currentStateName];
        currentStateDuration++;
        currentState.perTickEffect?.Invoke(actor, poiTarget, otherData);
        if (currentStateDuration >= currentState.duration) {
            EndPerTickEffect();
        }
    }
    private void OnPerformActualActionToTarget() {
        if (GoapActionStateDB.GetStateResult(action.goapType, currentStateName) != InteractionManager.Goap_State_Success) {
            return;
        }
        if (poiTarget is TileObject) {
            TileObject target = poiTarget as TileObject;
            target.OnDoActionToObject(action);
        }
        //else if (poiTarget is Character) {
        //    if (currentState.name != "Target Missing" && !doesNotStopTargetCharacter) {
        //        AddAwareCharacter(poiTarget as Character);
        //    }
        //}
    }
    #endregion

    #region Log
    private void CreateThoughtBubbleLog() {
        if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", action.GetType().ToString(), "thought_bubble")) {
            thoughtBubbleLog = new Log(GameManager.Instance.Today(), "GoapAction", action.GetType().ToString(), "thought_bubble", action);
            action.AddFillersToLog(thoughtBubbleLog, actor, poiTarget, otherData);
        }
        if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", action.GetType().ToString(), "thought_bubble_m")) {
            thoughtBubbleMovingLog = new Log(GameManager.Instance.Today(), "GoapAction", action.GetType().ToString(), "thought_bubble_m", action);
            action.AddFillersToLog(thoughtBubbleMovingLog, actor, poiTarget, otherData);
        }
    }
    #endregion
}