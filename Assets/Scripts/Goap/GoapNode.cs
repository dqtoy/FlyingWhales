using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Traits;

public struct GoapNode {
    //public GoapNode parent;
    //public int index;
    public int cost;
    public int level;
    public GoapAction action;
    public IPointOfInterest target;

    public GoapNode(int cost, int level, GoapAction action, IPointOfInterest target) {
        this.cost = cost;
        this.level = level;
        this.action = action;
        this.target = target;
    }
}
public class MultiJobNode : JobNode{
    public override ActualGoapNode singleNode { get { return null; } }
    public override ActualGoapNode[] multiNode { get { return nodes; } }
    public override int currentNodeIndex { get { return currentIndex; } }

    private ActualGoapNode[] nodes;
    private int currentIndex;
    public MultiJobNode(ActualGoapNode[] nodes) {
        this.nodes = nodes;
        currentIndex = 0;
    }

    #region Overrides
    public override void OnAttachPlanToJob(GoapPlanJob job) {
        for (int i = 0; i < nodes.Length; i++) {
            nodes[i].OnAttachPlanToJob(job);
        }
    }
    public override void SetNextActualNode() {
        currentIndex += 1;
    }
    public override bool IsCurrentActionNode(ActualGoapNode node) {
        for (int i = 0; i < nodes.Length; i++) {
            ActualGoapNode currNode = nodes[i];
            if(currNode == node) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
public class SingleJobNode : JobNode {
    public override ActualGoapNode singleNode { get { return node; } }
    public override ActualGoapNode[] multiNode { get { return null;} }
    public override int currentNodeIndex { get { return -1; } }

    private ActualGoapNode node;
    public SingleJobNode(ActualGoapNode node) {
        this.node = node;
    }

    #region Overrides
    public override void OnAttachPlanToJob(GoapPlanJob job) {
        node.OnAttachPlanToJob(job);
    }
    public override void SetNextActualNode() {
        //Not Applicable
    }
    public override bool IsCurrentActionNode(ActualGoapNode node) {
        return this.node == node;
    }
    #endregion
}
public abstract class JobNode {
    public abstract ActualGoapNode singleNode { get; }
    public abstract ActualGoapNode[] multiNode { get; }
    public abstract int currentNodeIndex { get; }
    public abstract void OnAttachPlanToJob(GoapPlanJob job);
    public abstract void SetNextActualNode();
    public abstract bool IsCurrentActionNode(ActualGoapNode node);
}

//actual nodes located in a finished plan that is going to be executed by a character
public class ActualGoapNode {
    public IPointOfInterest poiTarget { get; private set; }
    public AlterEgoData poiTargetAlterEgo { get; private set; } //The alter ego the target was using while doing this action. only occupied if target is a character
    public Character actor { get; private set; }
    public AlterEgoData actorAlterEgo { get; private set; } //The alter ego the character was using while doing this action.
    public bool isStealth { get; private set; }
    public object[] otherData { get; private set; }
    public int cost { get; private set; }

    public GoapAction action { get; private set; }
    public ACTION_STATUS actionStatus { get; private set; }
    public Log thoughtBubbleLog { get; private set; } //used if the current state of this action has a duration
    public Log thoughtBubbleMovingLog { get; private set; } //used when the actor is moving with this as his/her current action
    public Log descriptionLog { get; private set; } //action log at the end of the action
    public LocationStructure targetStructure { get; private set; }

    public string currentStateName { get; private set; }
    public int currentStateDuration { get; private set; }

    #region getters
    //TODO: Refactor these getters after all errors are resolved.
    public GoapActionState currentState {
        get {
            UnityEngine.Assertions.Assert.IsTrue(action.states.ContainsKey(currentStateName));
            return action.states[currentStateName]; 
        }
    }
    public bool isPerformingActualAction {
        get { return actionStatus == ACTION_STATUS.PERFORMING; }
    }
    public bool isDone {
        get { return actionStatus == ACTION_STATUS.SUCCESS || actionStatus == ACTION_STATUS.FAIL; }
    }
    public INTERACTION_TYPE goapType {
        get { return action.goapType; }
    }
    public string goapName {
        get { return action.goapName; }
    }
    #endregion

    public ActualGoapNode(GoapAction action, Character actor, IPointOfInterest poiTarget, object[] otherData, int cost) {
        this.action = action;
        this.actor = actor;
        this.poiTarget = poiTarget;
        this.otherData = otherData;
        this.cost = cost;
        actionStatus = ACTION_STATUS.NONE;
        currentStateName = string.Empty;
        //Messenger.AddListener<string, ActualGoapNode>(Signals.ACTION_STATE_SET, OnActionStateSet);
    }

    //public void DestroyNode() {
    //    Messenger.RemoveListener<string, ActualGoapNode>(Signals.ACTION_STATE_SET, OnActionStateSet);
    //}

    #region Action
    public virtual void DoAction(JobQueueItem job, GoapPlan plan) {
        actionStatus = ACTION_STATUS.STARTED;
        actor.SetCurrentActionNode(this, job, plan);
        //parentPlan?.SetPlanState(GOAP_PLAN_STATE.IN_PROGRESS);
        Messenger.Broadcast(Signals.CHARACTER_DOING_ACTION, actor, this);
        actor.marker.UpdateActionIcon();
        //poiTarget.AddTargettedByAction(this);

        //Move To Do Action
        actor.marker.pathfindingAI.ResetEndReachedDistance();
        MoveToDoAction();
    }
    private void MoveToDoAction() {
        if (targetStructure == null) {
            targetStructure = action.GetTargetStructure(actor, poiTarget, otherData);
            if (targetStructure == null) { throw new System.Exception(actor.name + " target structure of action " + action.goapName + " is null."); }
        }
        //Only create thought bubble log when characters starts the action/moves to do the action so we can pass the target structure
        CreateThoughtBubbleLog(targetStructure);
        if (actor.specificLocation != targetStructure.location) {
            actor.currentParty.GoToLocation(targetStructure.location.region, PATHFINDING_MODE.NORMAL, doneAction: MoveToDoAction);
        } else {
            if (action.actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET) {
                actor.marker.GoTo(action.GetTargetToGoTo(this), OnArriveAtTargetLocation);
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
                    //No OnArriveAtTargetLocation because it doesn't trigger on arrival rather on vision
                    actor.marker.GoTo(poiTarget);
                }
            }
        }
    }
    private void OnArriveAtTargetLocation() {
        actor.PerformGoapAction();
    }
    public void PerformAction() {
        GoapActionInvalidity goapActionInvalidity = action.IsInvalid(actor, poiTarget, otherData);
        if (goapActionInvalidity.isInvalid) {
            action.LogActionInvalid(goapActionInvalidity, this);
            actor.GoapActionResult(InteractionManager.Goap_State_Fail, this);
            return;
        }
        actionStatus = ACTION_STATUS.PERFORMING;
        actorAlterEgo = actor.currentAlterEgo;
        actor.marker.UpdateAnimation();

        if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character targetCharacter = poiTarget as Character;
            poiTargetAlterEgo = targetCharacter.currentAlterEgo;
            if (!action.doesNotStopTargetCharacter && actor != poiTarget) {
                if (!targetCharacter.isDead) {
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
                    if (targetCharacter.currentActionNode != null) {
                        targetCharacter.StopCurrentActionNode(false);
                    }
                    //if (targetCharacter.stateComponent.currentState != null) {
                    //    targetCharacter.stateComponent.currentState.PauseState();
                    //} else if (targetCharacter.stateComponent.stateToDo != null) {
                    //    targetCharacter.stateComponent.SetStateToDo(null, false, false);
                    //}
                    targetCharacter.DecreaseCanMove();
                    targetCharacter.FaceTarget(actor);
                }
                targetCharacter.AdjustIsStoppedByOtherCharacter(1);
            }
        }
        action.Perform(this);
    }
    public void ActionResult(GoapActionState actionState) {
        string result = GoapActionStateDB.GetStateResult(action.goapType, actionState.name);
        if (result == InteractionManager.Goap_State_Success) {
            actionStatus = ACTION_STATUS.SUCCESS;
        } else {
            actionStatus = ACTION_STATUS.FAIL;
        }
        //actor.OnCharacterDoAction(this);
        StopPerTickEffect();
        //endedAtState = currentState;
        //actor.PrintLogIfActive(action.goapType.ToString() + " action by " + this.actor.name + " Summary: \n" + actionSummary);
        if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER && !action.doesNotStopTargetCharacter && actor != poiTarget) {
            Character targetCharacter = poiTarget as Character;
            if (!targetCharacter.isDead) {
                //if (resumeTargetCharacterState) {
                //    if (targetCharacter.stateComponent.currentState != null && targetCharacter.stateComponent.currentState.isPaused) {
                //        targetCharacter.stateComponent.currentState.ResumeState();
                //    }
                //}
                targetCharacter.IncreaseCanMove();
            }
            targetCharacter.AdjustIsStoppedByOtherCharacter(-1);
        }
        //else {
        //    Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        //    Messenger.RemoveListener<TileObject, Character>(Signals.TILE_OBJECT_DISABLED, OnTileObjectDisabled);
        //}
        OnFinishActionTowardsTarget();
        actor.GoapActionResult(result, this);
        //if (endAction != null) {
        //    endAction(result, this);
        //} else {
        //    if (parentPlan != null) {
        //        //Do not go to result if there is no parent plan, this might mean that the action is just a forced action
        //        actor.GoapActionResult(result, this);
        //    }
        //}
        Messenger.Broadcast(Signals.CHARACTER_FINISHED_ACTION, actor, this.action, result);
        //parentPlan?.OnActionInPlanFinished(actor, this, result);
    }
    public void StopActionNode(bool shouldDoAfterEffect) {
        if (actionStatus == ACTION_STATUS.PERFORMING) {
            OnCancelActionTowardsTarget();
            //ReturnToActorTheActionResult(InteractionManager.Goap_State_Fail);
            action.OnStopWhilePerforming(actor, poiTarget, otherData);
            EndPerTickEffect(shouldDoAfterEffect);

            ////when the action is ended prematurely, make sure to readjust the target character's do not move values
            //if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            //    if (poiTarget != actor) {
            //        Character targetCharacter = poiTarget as Character;
            //        targetCharacter.marker.pathfindingAI.AdjustDoNotMove(-1);
            //        targetCharacter.marker.AdjustIsStoppedByOtherCharacter(-1);
            //    }
            //}
        } else if (actionStatus == ACTION_STATUS.STARTED) {
            //if (action != null && action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            //    Character targetCharacter = action.poiTarget as Character;
            //    targetCharacter.AdjustIsWaitingForInteraction(-1);
            //}
            action.OnStopWhileStarted(actor, poiTarget, otherData);
            //actor.DropPlan(parentPlan, forceProcessPlanJob: true); //TODO: Try to push back instead of dropping plan immediately, only drop plan if push back fails (fail: if no other plan replaces this plan)
        }
    }
    #endregion

    #region Action State
    public void OnActionStateSet(string stateName) {
        currentStateName = stateName;
        OnPerformActualActionToTarget();
        ExecuteCurrentActionState();
    }
    private void ExecuteCurrentActionState() {
        if (!action.states.ContainsKey(currentStateName)) {
            Debug.LogError("Failed to execute current action state for " + actor.name + " because " + action.goapName + " does not have state with name: " + currentStateName);
        }
        GoapActionState currentState = action.states[currentStateName];
        CreateDescriptionLog(currentState);
        currentState.preEffect?.Invoke(this);

        actor.marker.UpdateAnimation();

        for (int i = 0; i < actor.traitContainer.allTraits.Count; i++) {
            Trait currTrait = actor.traitContainer.allTraits[i];
            currTrait.ExecuteActionPreEffects(action.goapType, this);
        }
        for (int i = 0; i < poiTarget.traitContainer.allTraits.Count; i++) {
            Trait currTrait = poiTarget.traitContainer.allTraits[i];
            currTrait.ExecuteActionPreEffects(action.goapType, this);
        }
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
        if(actionStatus == ACTION_STATUS.FAIL || actionStatus == ACTION_STATUS.SUCCESS) { //This means that the action is already finished
            return;
        }
        GoapActionState currentState = action.states[currentStateName];
        ActionResult(currentState);

        //After effect and logs should be done after processing action result so that we can be sure that the action is completely done before doing anything
        if (shouldDoAfterEffect) {
            currentState.afterEffect?.Invoke(this);
            for (int i = 0; i < actor.traitContainer.allTraits.Count; i++) {
                Trait currTrait = actor.traitContainer.allTraits[i];
                currTrait.ExecuteActionAfterEffects(action.goapType, this);
            }
            for (int i = 0; i < poiTarget.traitContainer.allTraits.Count; i++) {
                Trait currTrait = poiTarget.traitContainer.allTraits[i];
                currTrait.ExecuteActionAfterEffects(action.goapType, this);
            }
            if (descriptionLog != null && action.shouldAddLogs) { //only add logs if both the parent action and this state should add logs
                //if (descriptionLog != null) {
                //    AddArrangedLog("description", descriptionLog, null);
                //}
                bool showPopup = false;
                if (action.showIntelNotification) {
                    if (action.shouldIntelNotificationOnlyIfActorIsActive) {
                        showPopup = PlayerManager.Instance.player.ShouldShowNotificationFrom(actor, true);
                    } else {
                        showPopup = PlayerManager.Instance.player.ShouldShowNotificationFrom(actor, descriptionLog);
                    }
                }
                if (showPopup) {
                    if (!action.isNotificationAnIntel) {
                        Messenger.Broadcast(Signals.SHOW_PLAYER_NOTIFICATION, descriptionLog);
                    } else {
                        Messenger.Broadcast(Signals.SHOW_INTEL_NOTIFICATION, InteractionManager.Instance.CreateNewIntel(action, actor));
                    }
                }
                descriptionLog.AddLogToInvolvedObjects();
                //for (int i = 0; i < arrangedLogs.Count; i++) {
                //    arrangedLogs[i].log.SetDate(GameManager.Instance.Today());
                //    arrangedLogs[i].log.AddLogToInvolvedObjects();
                //}
            }
        }
        //else {
        //    parentAction.SetShowIntelNotification(false);
        //}
        Messenger.Broadcast(Signals.CHARACTER_DID_ACTION, actor, action);
        //actor.OnCharacterDoAction(parentAction); //Moved this here to fix intel not being shown, because arranged logs are not added until after the ReturnToActorTheActionResult() call.
        //if (shouldDoAfterEffect) {
        //    action.AfterAfterEffect();
        //}
    }
    private void PerTickEffect() {
        GoapActionState currentState = action.states[currentStateName];
        currentStateDuration++;
        currentState.perTickEffect?.Invoke(this);
        for (int i = 0; i < actor.traitContainer.allTraits.Count; i++) {
            Trait currTrait = actor.traitContainer.allTraits[i];
            currTrait.ExecuteActionPerTickEffects(action.goapType, this);
        }
        for (int i = 0; i < poiTarget.traitContainer.allTraits.Count; i++) {
            Trait currTrait = poiTarget.traitContainer.allTraits[i];
            currTrait.ExecuteActionPerTickEffects(action.goapType, this);
        }
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
            target.OnDoActionToObject(this);
        }
        //else if (poiTarget is Character) {
        //    if (currentState.name != "Target Missing" && !doesNotStopTargetCharacter) {
        //        AddAwareCharacter(poiTarget as Character);
        //    }
        //}
    }
    private void OnFinishActionTowardsTarget() {
        if (actionStatus == ACTION_STATUS.FAIL) {
            return;
        }
        if (poiTarget is TileObject) {
            TileObject target = poiTarget as TileObject;
            target.OnDoneActionToObject(this);
        }
    }
    private void OnCancelActionTowardsTarget() {
        if (poiTarget is TileObject) {
            TileObject target = poiTarget as TileObject;
            target.OnCancelActionTowardsObject(this);
        }
    }
    public void OverrideCurrentStateDuration(int val) {
        currentStateDuration = val;
    }
    #endregion

    #region Log
    private void CreateDescriptionLog(GoapActionState actionState) {
        if (descriptionLog == null) {
            descriptionLog = actionState.CreateDescriptionLog(actor, poiTarget, this);
        }
    }
    private void CreateThoughtBubbleLog(LocationStructure targetStructure) {
        if(thoughtBubbleLog == null) {
            if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", action.GetType().ToString(), "thought_bubble")) {
                thoughtBubbleLog = new Log(GameManager.Instance.Today(), "GoapAction", action.GetType().ToString(), "thought_bubble", this);
                action.AddFillersToLog(thoughtBubbleLog, actor, poiTarget, otherData, targetStructure);
            }
        }
        if (thoughtBubbleMovingLog == null) {
            if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", action.GetType().ToString(), "thought_bubble_m")) {
                thoughtBubbleMovingLog = new Log(GameManager.Instance.Today(), "GoapAction", action.GetType().ToString(), "thought_bubble_m", this);
                action.AddFillersToLog(thoughtBubbleMovingLog, actor, poiTarget, otherData, targetStructure);
            }
        }
    }
    public Log GetCurrentLog() {
        if(actionStatus == ACTION_STATUS.STARTED) {
            return thoughtBubbleMovingLog;
        }else if (actionStatus == ACTION_STATUS.PERFORMING) {
            return thoughtBubbleLog;
        }
        return descriptionLog;
        //if (onlyShowNotifOfDescriptionLog && currentState != null) {
        //    return this.currentState.descriptionLog;
        //}
        //if (actor.currentParty.icon.isTravelling) {
        //    if (currentState != null) {
        //        //character is travelling but there is already a current state
        //        //Note: this will only happen is action has whileMovingState
        //        //Examples are: Imprison Character and Abduct Character actions
        //        return currentState.descriptionLog;
        //    }
        //    //character is travelling
        //    return thoughtBubbleMovingLog;
        //} else {
        //    //character is not travelling
        //    if (this.isDone) {
        //        //action is already done
        //        return this.currentState.descriptionLog;
        //    } else {
        //        //action is not yet done
        //        if (currentState == null) {
        //            //if the actions' current state is null, Use moving log
        //            return thoughtBubbleMovingLog;
        //        } else {
        //            //if the actions current state has a duration
        //            return thoughtBubbleLog;
        //        }
        //    }
        //}
    }
    public void OverrideDescriptionLog(Log log) {
        descriptionLog = log;
    }
    #endregion

    #region Jobs
    public void OnAttachPlanToJob(GoapPlanJob job) {
        isStealth = job.isStealth;
    }
    #endregion
}