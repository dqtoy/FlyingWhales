﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;

public class GoapAction {

    public INTERACTION_TYPE goapType { get; private set; }
    public INTERACTION_ALIGNMENT alignment { get; private set; }
    public string goapName { get; protected set; }
    public IPointOfInterest poiTarget { get; private set; }
    public AlterEgoData poiTargetAlterEgo { get; protected set; } //The alter ego the target was using while doing this action. only occupied if target is a character
    public Character actor { get; private set; }
    public AlterEgoData actorAlterEgo { get; protected set; } //The alter ego the character was using while doing this action.
    public int cost { get { return (GetCost() * CostMultiplier()) + GetDistanceCost(); } }
    public List<Precondition> preconditions { get; private set; }
    public List<GoapEffect> expectedEffects { get; private set; }
    public virtual LocationStructure targetStructure {
        get {
            if (poiTarget is Character) {
                return (poiTarget as Character).currentStructure;
            }
            if(poiTarget.gridTileLocation == null) {
                return null;
            }
            return poiTarget.gridTileLocation.structure;
            //try {
            //    //if (poiTarget is TileObject || poiTarget is SpecialToken) {
            //    //    //if the target is a tile object or a special token, the actor will always go to it's known location instead of actual
            //    //    IAwareness awareness = actor.HasAwareness(poiTarget);
            //    //    if (awareness != null) {
            //    //        return awareness.knownGridLocation.structure;
            //    //    }
            //    //} else 
            //    if (poiTarget is Character) {
            //        return (poiTarget as Character).currentStructure;
            //    }
            //    return poiTarget.gridTileLocation.structure;
            //} catch {
            //    throw new Exception("Error with target structure in " + goapName + " targetting " + poiTarget.ToString() + " by " + actor.name);
            //}
        }
    }
    public LocationGridTile targetTile { get; protected set; }
    public Dictionary<string, GoapActionState> states { get; protected set; }
    public List<GoapEffect> actualEffects { get; private set; }
    public Log thoughtBubbleLog { get; protected set; } //used if the current state of this action has a duration
    public Log thoughtBubbleMovingLog { get; protected set; } //used when the actor is moving with this as his/her current action
    public Log planLog { get; protected set; } //used for notification when a character starts this action. NOTE: Do not show notification if this is null
    public Log targetLog { get; protected set; }
    public GoapActionState currentState { get; private set; }
    public GoapActionState endedAtState { get; private set; } //the state this action ended at
    public GoapPlan parentPlan { get; private set; }
    public bool isStopped { get; private set; }
    public bool isStoppedAsCurrentAction { get; private set; }
    public bool isPerformingActualAction { get; protected set; }
    public bool isDone { get; private set; }
    public ACTION_LOCATION_TYPE actionLocationType { get; protected set; } //This is set in every action's constructor
    public bool showIntelNotification { get; protected set; } //should this action show a notification when it is done by its actor or when it recieves a plan with this action as it's end node?
    public bool shouldAddLogs { get; protected set; } //should this action add logs to it's actor?
    public bool shouldIntelNotificationOnlyIfActorIsActive { get; protected set; } //should this action add logs to it's actor?
    public string actionIconString { get; protected set; }
    public GameDate executionDate { get; protected set; }
    protected virtual string failActionState { get { return "Target Missing"; } }
    private System.Action<string, GoapAction> endAction; //if this is not null, this action will return result here, instead of the default actor.GoapActionResult
    public CRIME committedCrime { get; private set; }
    public bool hasCrimeBeenReported { get; private set; }
    public string result { get; private set; }
    public string animationName { get; protected set; } //what animation should the character be playing while doing this action
    public bool doesNotStopTargetCharacter { get; protected set; }
    public bool resumeTargetCharacterState { get; protected set; } //used to determine whether or not the target character's current state should be resumed after this action is performed towards him
    public bool cannotCancelAction { get; protected set; }
    public bool isNotificationAnIntel { get; protected set; }
    public bool canBeAddedToMemory { get; protected set; }
    public bool onlyShowNotifOfDescriptionLog { get; protected set; }
    public CharacterState characterState { get; protected set; }
    public Character[] crimeCommitters { get; protected set; }
    public List<Character> awareCharactersOfThisAction { get; protected set; } //all characters that witnessed/aware of this action
    public bool isOldNews { get; protected set; }
    public int referenceCount { get; protected set; }
    public bool willAvoidCharactersWhileMoving { get; protected set; }
    public bool isStealth { get; protected set; }
    public bool isRoamingAction { get; protected set; } //Is this action a roaming action like Hunting To Drink Blood or Roaming To Steal
    public object[] otherData { get; protected set; }
    public string whileMovingState { get; protected set; } //The state name of this action while actor is still moving towards the target location/character, this can be empty, in fact the default value is empty, this is only needed when we want an action to have a current state even though he/she is still moving to the target, this does not mean that the actor is performing actual action

    protected virtual bool isTargetMissing {
        get {
            bool targetMissing = false;
            if (parentPlan != null && parentPlan.job != null && parentPlan.job.allowDeadTargets) {
                targetMissing = poiTarget.gridTileLocation == null || actor.specificLocation != poiTarget.specificLocation
                    || !(actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) || !(poiTarget as Character).isDead;
            } else {
                targetMissing = !poiTarget.IsAvailable() || poiTarget.gridTileLocation == null || actor.specificLocation != poiTarget.specificLocation
                    || !(actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation));
            }

            if (targetMissing) {
                return targetMissing;
            } else {
                if (actor != poiTarget) {
                    Invisible invisible = poiTarget.GetNormalTrait("Invisible") as Invisible;
                    if (invisible != null && !invisible.charactersThatCanSee.Contains(actor)) {
                        return true;
                    }
                }
                return targetMissing;
            }
        }
    }

    protected bool _stayInArea; //if the character should stay at his/her current area to do this action

    protected Func<bool> _requirementAction;
    protected Func<bool> _requirementOnBuildGoapTreeAction; //This particular requirement will only be called when building the goap tree in multithread
    protected System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    protected string actionSummary;
    protected TIME_IN_WORDS[] validTimeOfDays;

    public GoapAction(INTERACTION_TYPE goapType, INTERACTION_ALIGNMENT alignment, Character actor, IPointOfInterest poiTarget) {
        this.alignment = alignment;
        this.goapType = goapType;
        this.goapName = Utilities.NormalizeStringUpperCaseFirstLetters(goapType.ToString());
        this.poiTarget = poiTarget;
        this.actor = actor;
        SetIsStopped(false);
        isPerformingActualAction = false;
        isDone = false;
        SetShowIntelNotification(true);
        shouldAddLogs = true;
        preconditions = new List<Precondition>();
        expectedEffects = new List<GoapEffect>();
        actualEffects = new List<GoapEffect>();
        committedCrime = CRIME.NONE;
        animationName = string.Empty;
        resumeTargetCharacterState = true;
        isNotificationAnIntel = true;
        canBeAddedToMemory = true;
        _stayInArea = false;
        awareCharactersOfThisAction = new List<Character>();
        whileMovingState = string.Empty;
        //for testing
        //CRIME[] choices = Utilities.GetEnumValues<CRIME>();
        //committedCrime = choices[Utilities.rng.Next(1, choices.Length)];
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Entertain_Icon;
        actionSummary = GameManager.Instance.TodayLogString() + actor.name + " created " + goapType.ToString() + " action, targetting " + poiTarget?.ToString() ?? "Nothing";
    }
    public void SetParentPlan(GoapPlan plan) {
        parentPlan = plan;
    }

    #region States
    public void SetState(string stateName) {
        currentState = states[stateName];
        AddActionDebugLog(GameManager.Instance.TodayLogString() + " Set state to " + currentState.name);
        OnPerformActualActionToTarget();
        currentState.Execute();
        Messenger.Broadcast(Signals.ACTION_STATE_SET, this, currentState);
    }
    /// <summary>
    /// Change the state that this action is in. This is used when the current action already has a state,
    /// but some change in external conditions needs the action to change effects midway.
    /// </summary>
    /// <param name="newState">The state that this action will switch to.</param>
    public void ChangeState(string newState) {
        GoapActionState nextState = states[newState];
        nextState.OverrideDuration(currentState.duration - currentState.currentDuration); //set the duration of the next state to be the remaining duration of the current state.
        CleanupBeforeChangingStates();
        currentState.StopPerTickEffect();
        SetState(newState);
    }
    /// <summary>
    /// Utility function to cleanup any unneeded mechanics before this action changes states.
    /// </summary>
    protected virtual void CleanupBeforeChangingStates() { }
    #endregion

    #region Virtuals
    public virtual void CreateStates() {
        string summary = "Creating states for goap action (Dynamic) " + goapType.ToString() + " for actor " + actor.name;
        sw.Start();
        states = new Dictionary<string, GoapActionState>();
        if (GoapActionStateDB.goapActionStates.ContainsKey(this.goapType)) {
            StateNameAndDuration[] statesSetup = GoapActionStateDB.goapActionStates[this.goapType];
            for (int i = 0; i < statesSetup.Length; i++) {
                StateNameAndDuration state = statesSetup[i];
                string trimmedState = Utilities.RemoveAllWhiteSpace(state.name);
                Type thisType = this.GetType();
                MethodInfo preMethod = thisType.GetMethod("Pre" + trimmedState, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                MethodInfo perMethod = thisType.GetMethod("PerTick" + trimmedState, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                MethodInfo afterMethod = thisType.GetMethod("After" + trimmedState, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                Action preAction = null;
                Action perAction = null;
                Action afterAction = null;
                if (preMethod != null) {
                    preAction = (Action) Delegate.CreateDelegate(typeof(Action), this, preMethod, false);
                }
                if (perMethod != null) {
                    perAction = (Action) Delegate.CreateDelegate(typeof(Action), this, perMethod, false);
                }
                if (afterMethod != null) {
                    afterAction = (Action) Delegate.CreateDelegate(typeof(Action), this, afterMethod, false);
                }
                GoapActionState newState = new GoapActionState(state.name, this, preAction, perAction, afterAction, state.duration, state.status);
                states.Add(state.name, newState);
                summary += "\n Creating state " + state.name;
            }

        }
        sw.Stop();
        //Debug.Log(summary + "\n" + string.Format("Total creation time is {0}ms", sw.ElapsedMilliseconds));

    }
    protected virtual void ConstructPreconditionsAndEffects() { }
    protected virtual void ConstructRequirement() { }
    protected virtual void ConstructRequirementOnBuildGoapTree() { }
    protected virtual int GetCost() {
        return 0;
    }
    public virtual void PerformActualAction() {
        isPerformingActualAction = true;
        actorAlterEgo = actor.currentAlterEgo;
        //Messenger.AddListener<IPointOfInterest, GoapAction>(Signals.OLD_NEWS_TRIGGER, OldNewsTrigger);
        AddAwareCharacter(actor);
        //poiTarget.AddTargettedByAction(this);
        if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character targetCharacter = poiTarget as Character;
            poiTargetAlterEgo = targetCharacter.currentAlterEgo;
            if (poiTarget != actor && !targetCharacter.isDead) {
                if (!doesNotStopTargetCharacter) {
                    if (targetCharacter.currentParty.icon.isTravelling) {
                        if (targetCharacter.currentParty.icon.travelLine == null) {
                            //This means that the actor currently travelling to another tile in tilemap
                            targetCharacter.marker.StopMovement();
                        } else {
                            //This means that the actor is currently travelling to another area
                            targetCharacter.currentParty.icon.SetOnArriveAction(() => targetCharacter.OnArriveAtAreaStopMovement());
                        }
                    }
                    if (targetCharacter.currentAction != null) {
                        targetCharacter.StopCurrentAction(false);
                    }
                    if (targetCharacter.stateComponent.currentState != null) {
                        targetCharacter.stateComponent.currentState.PauseState();
                    } else if (targetCharacter.stateComponent.stateToDo != null) {
                        targetCharacter.stateComponent.SetStateToDo(null, false, false);
                    }
                    targetCharacter.marker.pathfindingAI.AdjustDoNotMove(1);
                    targetCharacter.AdjustIsStoppedByOtherCharacter(1);
                    targetCharacter.FaceTarget(actor);
                }
            }
        } else {
            Cursed cursed = poiTarget.GetNormalTrait("Cursed") as Cursed;
            if (cursed != null) {
                cursed.Interact(actor, this);
            }
            Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
            Messenger.AddListener<TileObject, Character>(Signals.TILE_OBJECT_DISABLED, OnTileObjectDisabled);
        }
    }
    protected virtual void CreateThoughtBubbleLog() {
        if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", this.GetType().ToString(), "thought_bubble")) {
            thoughtBubbleLog = new Log(GameManager.Instance.Today(), "GoapAction", this.GetType().ToString(), "thought_bubble", this);
            AddDefaultObjectsToLog(thoughtBubbleLog);
        }
        if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", this.GetType().ToString(), "thought_bubble_m")) {
            thoughtBubbleMovingLog = new Log(GameManager.Instance.Today(), "GoapAction", this.GetType().ToString(), "thought_bubble_m", this);
            AddDefaultObjectsToLog(thoughtBubbleMovingLog);
        }
        if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", this.GetType().ToString(), "plan_log")) {
            planLog = new Log(GameManager.Instance.Today(), "GoapAction", this.GetType().ToString(), "plan_log", this);
            AddDefaultObjectsToLog(planLog);
        }
        if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", this.GetType().ToString(), "target_log")) {
            targetLog = new Log(GameManager.Instance.Today(), "GoapAction", this.GetType().ToString(), "target_log", this);
            AddDefaultObjectsToLog(targetLog);
        }
    }
    protected virtual void AddDefaultObjectsToLog(Log log) {
        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
        if (targetStructure != null) {
            log.AddToFillers(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        } else {
            log.AddToFillers(actor.specificLocation, actor.specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    ///<summary>
    ///This is called when the actor decides to do this specific action.
    ///All movement related actions should be done here.
    ///</summary>
    public virtual void DoAction() {
        //CreateStates(); //Not sure if this is the best place for this.
        //SetTargetStructure(); //Update target tile and structure so that the character will not go to the target tile that is calculated during the initialization of this action, so the character will move precisely to the intended target
        actor.SetCurrentAction(this);
        parentPlan?.SetPlanState(GOAP_PLAN_STATE.IN_PROGRESS);
        Messenger.Broadcast(Signals.CHARACTER_DOING_ACTION, actor, this);
        actor.marker.OnThisCharacterDoingAction(this);

        Character targetCharacter = null;
        if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            if ((poiTarget as Character) != actor) {
                targetCharacter = poiTarget as Character;
            }
        }
        poiTarget.AddTargettedByAction(this);
        if (whileMovingState != string.Empty) {
            SetState(whileMovingState);
        }
        MoveToDoAction(targetCharacter);
    }
    public virtual LocationGridTile GetTargetLocationTile() {
        //LocationGridTile knownTargetLocation = null;
        if (poiTarget is TileObject || poiTarget is SpecialToken) {
            //if the target is a tile object or a special token, the actor will always go to it's known location instead of actual
            //IAwareness awareness = actor.HasAwareness(poiTarget);
            //if (awareness != null) {
            //    knownTargetLocation = awareness.knownGridLocation;
            //} else {
            //    knownTargetLocation = poiTarget.gridTileLocation;
            //}
            return poiTarget.gridTileLocation;

            //return InteractionManager.Instance.GetTargetLocationTile(ACTION_LOCATION_TYPE.NEAR_TARGET, actor, knownTargetLocation, targetStructure);
        }
        //else {
        //    knownTargetLocation = poiTarget.gridTileLocation;
        //}
        return null;
    }
    public virtual void SetTargetStructure() {
        if (targetStructure != null) {
            targetTile = GetTargetLocationTile();
        }
    }
    //This is for the waiting time, if this returns true, this action will not be done by the actor momentarily, this will be skipped until this returns false
    public virtual bool IsHalted() {
        //Only waiting condition for now is the time of day
        //The default for the valid time of days is null, if it is null, do not wait meaning return false
        //if (validTimeOfDays != null && !validTimeOfDays.Contains(GameManager.GetCurrentTimeInWordsOfTick())) {
        //    return true;
        //}

        //if this action is a stealth action, check if there are any characters around, if yes, halt(suspend) this action
        //if (_isStealthAction) {
        //    if (_numOfTries < 18 && HasOtherCharacterInRadius()) {
        //        _numOfTries++;
        //        return true;
        //    }
        //}
        return false;
    }
    /// <summary>
    /// Used for Ghost collision handling, when a character sees a ghost collider of a POI
    /// and that POI is the target of this action, this will execute this actions target missing state.
    /// If none will execute whatever fail state this action has.
    /// NOTE: can override state to execute. <see cref="failActionState"/>
    /// </summary>
    public virtual void ExecuteTargetMissing() {
        if (states.ContainsKey("Target Missing")) {
            SetState("Target Missing");
        } else {
            //for cases that the current action has no target missing state
            FailAction();
        }
    }
    /// <summary>
    /// Setup other data needed for an action, This is called during plan creation.
    /// </summary>
    /// <param name="otherData">Array of data</param>
    /// <returns>If any other data was initialized</returns>
    public virtual bool InitializeOtherData(object[] otherData) { return false; }
    protected virtual void MoveToDoAction(Character targetCharacter) {
        //if the actor is NOT at the area where the target structure is, make him/her go there first.
        actor.marker.pathfindingAI.ResetEndReachedDistance();
        if (targetStructure == null) {
            throw new Exception(actor.name + " target structure of action " + this.goapName + " is null.");
        }

        if (actor.specificLocation != targetStructure.location) {
            if (_stayInArea) {
                actor.PerformGoapAction();
            } else {
                actor.currentParty.GoToLocation(targetStructure.location.region, PATHFINDING_MODE.NORMAL, targetStructure, OnArriveAtTargetLocation, null, poiTarget, targetTile);
            }
        } else {
            //if the actor is already at the area where the target structure is, just make the actor move to the specified target structure (ususally the structure where the poiTarget is at).
            if (targetTile != null) {
                actor.marker.GoTo(targetTile, OnArriveAtTargetLocation);
            } else {
                actor.marker.GoTo(poiTarget, OnArriveAtTargetLocation);
            }
        }
    }
    //If this action's current state is being performed and is stopped abruptly, call this
    public virtual void OnStopActionDuringCurrentState() { }
    /// <summary>
    /// What should happen when another character witnesses this action.
    /// NOTE: This only happens when the character finishes the action. NOT during.
    /// </summary>
    /// <param name="witness">The character that witnessed this action</param>
    public virtual void OnWitnessedBy(Character witness) { }
    /// <summary>
    /// This is called when this actions result has been FULLY processed by the actor.
    /// This should be the last thing that is called in the action flow.
    /// </summary>
    public virtual void OnResultReturnedToActor() { }
    /// <summary>
    /// If this action is a crime. Should the given character be allowed to react to it?
    /// </summary>
    /// <param name="character">The character in question.</param>
    /// <returns>If the character is allowed to react or not.</returns>
    public virtual bool CanReactToThisCrime(Character character) {
        //if the witnessed crime is targetting this character, this character should not react to the crime if the crime's doesNotStopTargetCharacter is true
        if (poiTarget == character && doesNotStopTargetCharacter) {
            return false;
        }
        return true;
    }
    /// <summary>
    /// Does this action target the provided POI?
    /// </summary>
    /// <param name="poi">The POI in question.</param>
    /// <returns>True or false.</returns>
    public virtual bool IsTarget(IPointOfInterest poi) {
        return poiTarget == poi;
    }
    /// <summary>
    /// This might change the value of isOldNews to true if the conditions are met
    /// </summary>
    /// <param name="poi">The POI that is the basis for the old news. Usually, it must match with the action's poiTarget.</param>
    /// <param name="action">Can be null. If this is not null, then the listener action must match with this.</param>
    protected virtual void OldNewsTrigger(IPointOfInterest poi, GoapAction action) { }
    /// <summary>
    /// What happens when the parent plan of this action has a job
    /// </summary>
    public virtual void OnSetJob(GoapPlanJob job) {
        if (job.isStealth) {
            SetIsStealth(true);
        }
    }
    /// <summary>
    /// What should happen when an action is stopped while the actor is still travelling towards it's target?
    /// </summary>
    public virtual void OnStopActionWhileTravelling() { }
    public virtual int GetArrangedLogPriorityIndex(string priorityID) { return -1; }
    public virtual bool ShouldBeStoppedWhenSwitchingStates() {
        return true; //by default, when a character is switching states and has a current action, that action will be stopped.
    }
    #endregion

    #region Utilities
    private void OnArriveAtTargetLocation() {
        actor.PerformGoapAction();
    }
    public void Initialize() {
        SetTargetStructure();
        //ConstructRequirement();
        //ConstructRequirementOnBuildGoapTree();
        ConstructPreconditionsAndEffects();
        CreateThoughtBubbleLog();
        CreateStates();
    }
    public bool IsThisPartOfActorActionPool(Character actor) {
        List<INTERACTION_TYPE> actorInteractions = RaceManager.Instance.GetNPCInteractionsOfCharacter(actor);
        return actorInteractions.Contains(goapType);
    }
    public bool CanSatisfyRequirements() {
        bool requirementActionSatisfied = true;
        if (_requirementAction != null) {
            requirementActionSatisfied = _requirementAction();
        }
        if (requirementActionSatisfied) {
            if (goapType.IsDirectCombatAction()) { //Reference: https://trello.com/c/uxZxcOEo/2343-critical-characters-shouldnt-attempt-hostile-actions
                requirementActionSatisfied = actor.IsCombatReady();
            }
        }
        return requirementActionSatisfied; //&& (validTimeOfDays == null || validTimeOfDays.Contains(GameManager.GetCurrentTimeInWordsOfTick()));
    }
    public bool CanSatisfyRequirementOnBuildGoapTree() {
        bool requirementActionSatisfied = true;
        if (_requirementOnBuildGoapTreeAction != null) {
            requirementActionSatisfied = _requirementOnBuildGoapTreeAction();
        }
        if (requirementActionSatisfied) {
            if (goapType.IsDirectCombatAction()) { //Reference: https://trello.com/c/uxZxcOEo/2343-critical-characters-shouldnt-attempt-hostile-actions
                requirementActionSatisfied = actor.IsCombatReady();
            }
        }
        return requirementActionSatisfied;
    }
    public void ReturnToActorTheActionResult(string result) {
        this.result = result;
        actor.OnCharacterDoAction(this);
        currentState.StopPerTickEffect();

        isPerformingActualAction = false;
        isDone = true;
        endedAtState = currentState;
        this.actor.PrintLogIfActive(this.goapType.ToString() + " action by " + this.actor.name + " Summary: \n" + actionSummary);

        if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            if (poiTarget != actor) {
                Character targetCharacter = poiTarget as Character;
                if (!targetCharacter.isDead) {
                    if (!doesNotStopTargetCharacter) {
                        if (resumeTargetCharacterState) {
                            if (targetCharacter.stateComponent.currentState != null && targetCharacter.stateComponent.currentState.isPaused) {
                                targetCharacter.stateComponent.currentState.ResumeState();
                            }
                        }
                        targetCharacter.marker.pathfindingAI.AdjustDoNotMove(-1);
                        targetCharacter.AdjustIsStoppedByOtherCharacter(-1);
                    }
                }
            }
        } else {
            Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
            Messenger.RemoveListener<TileObject, Character>(Signals.TILE_OBJECT_DISABLED, OnTileObjectDisabled);
        }
        OnFinishActionTowardsTarget();
        if (endAction != null) {
            endAction(result, this);
        } else {
            if (parentPlan != null) {
                //Do not go to result if there is no parent plan, this might mean that the action is just a forced action
                actor.GoapActionResult(result, this);
            }
        }
        Messenger.Broadcast(Signals.CHARACTER_FINISHED_ACTION, actor, this, result);
        parentPlan?.OnActionInPlanFinished(actor, this, result);
    }
    protected void AddActionDebugLog(string log) {
        actionSummary += "\n" + log;
    }
    public void SetEndAction(System.Action<string, GoapAction> endAction) {
        this.endAction = endAction;
    }
    public void StopAction(bool removeJobInQueue = false) {
        //GoapAction action = actor.currentAction;
        //if(actor.marker.pathfindingThread != null) {
        //    actor.marker.pathfindingThread.SetDoNotMove(true);
        //}
        actor.SetCurrentAction(null);
        if (actor.currentParty.icon.isTravelling) {
            if (actor.currentParty.icon.travelLine == null) {
                //This means that the actor currently travelling to another tile in tilemap
                actor.marker.StopMovement();
            } else {
                //This means that the actor is currently travelling to another area
                actor.currentParty.icon.SetOnArriveAction(() => actor.OnArriveAtAreaStopMovement());
            }
        }
        //if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
        //    Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        //    Messenger.RemoveListener<TileObject, Character>(Signals.TILE_OBJECT_DISABLED, OnTileObjectDisabled);
        //}

        OnCancelActionTowardsTarget();
        SetIsStopped(true);

        JobQueueItem job = parentPlan.job;

        if (isPerformingActualAction && !isDone) {
            //ReturnToActorTheActionResult(InteractionManager.Goap_State_Fail);
            OnStopActionDuringCurrentState();
            currentState.EndPerTickEffect(false);

            ////when the action is ended prematurely, make sure to readjust the target character's do not move values
            //if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            //    if (poiTarget != actor) {
            //        Character targetCharacter = poiTarget as Character;
            //        targetCharacter.marker.pathfindingAI.AdjustDoNotMove(-1);
            //        targetCharacter.marker.AdjustIsStoppedByOtherCharacter(-1);
            //    }
            //}
        } else {
            //if (action != null && action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            //    Character targetCharacter = action.poiTarget as Character;
            //    targetCharacter.AdjustIsWaitingForInteraction(-1);
            //}
            OnStopActionWhileTravelling();
            actor.DropPlan(parentPlan, forceProcessPlanJob: true);
        }
        //Remove job in queue if job is personal job and removeJobInQueue value is true
        if (removeJobInQueue && job != null && !job.jobQueueParent.isAreaOrQuestJobQueue) {
            job.jobQueueParent.RemoveJobInQueue(job);
        }
        if (UIManager.Instance.characterInfoUI.isShowing) {
            UIManager.Instance.characterInfoUI.UpdateBasicInfo();
        }
        //Messenger.Broadcast<GoapAction>(Signals.STOP_ACTION, this);
        actor.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Stopped action of " + actor.name + " which is " + this.goapName + " targetting " + poiTarget.name + "!");
    }
    public void SetIsStopped(bool state) {
        isStopped = state;
    }
    public void SetIsStoppedAsCurrentAction(bool state) {
        isStoppedAsCurrentAction = state;
    }
    public int GetDistanceCost() {
        if (actor.specificLocation == null) {
            return 1;
            //throw new Exception(actor.name + " specific location is null!");
        }
        if (targetStructure == null) {
            return 1;
            //throw new Exception(actor.name + "'s target structure in " + goapName + " is null! Targetting " + poiTarget.name);
        }
        LocationGridTile tile = targetTile;
        if (tile == null) {
            tile = poiTarget.gridTileLocation;
        }
        //try {
        if (actor.gridTileLocation != null && tile != null) {
            int distance = Mathf.RoundToInt(Vector2.Distance(actor.gridTileLocation.centeredWorldLocation, tile.centeredWorldLocation));
            distance = (int) (distance * 0.25f);
            if (actor.specificLocation != targetStructure.location) {
                return distance + 10;
            }
            return distance;
        }
        //} catch {
        //    Debug.LogError("Distance cost problem for " + poiTarget.name + " with actor " + actor.name + ", poitarget grid location is " + poiTarget.gridTileLocation == null ? "null" : poiTarget.gridTileLocation.ToString());
        //}
        return 1;
        //if (actor.specificLocation != targetStructure.location) {
        //    return 3;
        //} else {
        //    LocationGridTile tile = targetTile;
        //    if (tile == null) {
        //        tile = poiTarget.gridTileLocation;
        //    }
        //    try {
        //        int distance = Mathf.RoundToInt(Vector2.Distance(actor.gridTileLocation.centeredWorldLocation, tile.centeredWorldLocation));
        //        return distance / 6;
        //    } catch (Exception e) {
        //        Debug.LogError("Distance cost problem for " + poiTarget.name + " with actor " + actor.name + ", poitarget grid location is " + poiTarget.gridTileLocation == null ? "null" : poiTarget.gridTileLocation.ToString());
        //    }
        //    return 1;
        //}
    }
    protected bool HasSupply(int neededSupply) {
        return actor.supply >= neededSupply;
    }
    public void SetExecutionDate(GameDate date) {
        executionDate = date;
    }
    public void FailAction() {
        //if (goapType == INTERACTION_TYPE.SLEEP) {
        //    Debug.LogError(actor.name + " failed " + goapName + " action from recalculate path!");
        //}
        if (actor.currentParty.icon.isTravelling && actor.currentParty.icon.travelLine == null) {
            //This means that the actor currently travelling to another tile in tilemap
            actor.marker.StopMovement();
        } else {
            SetState(failActionState);
        }
        //Set state to failed after this (in overrides)
    }
    public void SetCannotCancelAction(bool state) {
        cannotCancelAction = state;
    }
    public void SetShowIntelNotification(bool state) {
        showIntelNotification = state;
    }
    public bool IsFromApprehendJob() {
        if (parentPlan != null && parentPlan.job != null &&
            (parentPlan.job.jobType == JOB_TYPE.KNOCKOUT || parentPlan.job.jobType == JOB_TYPE.APPREHEND)) {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Return the log that best describes the state of this current action.
    /// e.g. Actor is travelling towards target, Actor is currently doing the action, Actor is done doing action
    /// </summary>
    /// <returns>Chosen log.</returns>
    public Log GetCurrentLog() {
        if (onlyShowNotifOfDescriptionLog && currentState != null) {
            return this.currentState.descriptionLog;
        }
        if (actor.currentParty.icon.isTravelling) {
            if(currentState != null) {
                //character is travelling but there is already a current state
                //Note: this will only happen is action has whileMovingState
                //Examples are: Imprison Character and Abduct Character actions
                return currentState.descriptionLog;
            }
            //character is travelling
            return thoughtBubbleMovingLog;
        } else {
            //character is not travelling
            if (this.isDone) {
                //action is already done
                return this.currentState.descriptionLog;
            } else {
                //action is not yet done
                if (currentState == null) {
                    //if the actions' current state is null, Use moving log
                    return thoughtBubbleMovingLog;
                } else {
                    //if the actions current state has a duration
                    return thoughtBubbleLog;
                }
            }
        }
    }
    public bool IsActorAtTargetTile() {
        return actor.gridTileLocation == targetTile;
    }
    public int CostMultiplier() {
        if (validTimeOfDays == null || validTimeOfDays.Contains(GameManager.GetCurrentTimeInWordsOfTick())) {
            return 1;
        }
        return 3;
    }
    public void AddAwareCharacter(Character character) {
        if (!awareCharactersOfThisAction.Contains(character)) {
            awareCharactersOfThisAction.Add(character);
        }
    }
    public void SetIsOldNews(bool state) {
        if (isOldNews != state) {
            isOldNews = state;
            if (isOldNews) {
                if (Messenger.eventTable.ContainsKey(Signals.OLD_NEWS_TRIGGER)) {
                    Messenger.RemoveListener<IPointOfInterest, GoapAction>(Signals.OLD_NEWS_TRIGGER, OldNewsTrigger);
                }
            }
        }
    }
    public void AdjustReferenceCount(int amount) {
        referenceCount += amount;
        referenceCount = Mathf.Max(0, referenceCount);
        if (referenceCount == 0) {
            SetIsOldNews(true);
        }
    }
    public void SetActionLocationType(ACTION_LOCATION_TYPE locType) {
        actionLocationType = locType;
        SetTargetStructure();
    }
    public void SetWillAvoidCharactersWhileMoving(bool state) {
        willAvoidCharactersWhileMoving = state;
    }
    public void SetIsStealth(bool state) {
        isStealth = state;
    }
    public override string ToString() {
        return goapName + " by " + actor.name;
    }
    #endregion

    #region Trait Utilities
    protected bool HasTrait(Character character, string traitName) {
        return character.GetNormalTrait(traitName) != null;
    }
    /// <summary>
    /// Helper function to encapsulate adding a trait to a poi and adding actual effect data based on the added trait.
    /// </summary>
    /// <param name="target">POI that gains a trait</param>
    /// <param name="traitName">Trait to be gained</param>
    protected bool AddTraitTo(IPointOfInterest target, string traitName, Character characterResponsible = null, System.Action onRemoveAction = null) {
        if (target.AddTrait(traitName, characterResponsible, onRemoveAction, this)) {
            AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = traitName, targetPOI = target });
            return true;
        }
        return false;
    }
    protected bool AddTraitTo(IPointOfInterest target, Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null) {
        if (target.AddTrait(trait, characterResponsible, onRemoveAction, this)) {
            AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = trait.name, targetPOI = target });
            return true;
        }
        return false;
    }
    /// <summary>
    /// Helper function to encapsulate removing a trait from a poi and adding actual effect data based on the removed trait.
    /// </summary>
    /// <param name="target">POI that loses a trait</param>
    /// <param name="traitName">Trait to be lost</param>
    protected void RemoveTraitFrom(IPointOfInterest target, string traitName, Character removedBy = null, bool showNotif = true) {
        if (target.RemoveTrait(traitName, removedBy: removedBy)) {
            AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = traitName, targetPOI = target });
        }
    }
    /// <summary>
    /// Helper function to encapsulate removing traits of a type from a poi and adding actual effect data based on the removed traits.
    /// </summary>
    /// <param name="target">POI that loses traits</param>
    /// <param name="type">Type of traits to be lost</param>
    protected void RemoveTraitsOfType(IPointOfInterest target, TRAIT_TYPE type) {
        List<Trait> removedTraits = target.RemoveAllTraitsByType(type);
        for (int i = 0; i < removedTraits.Count; i++) {
            AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = removedTraits[i].name, targetPOI = target });
        }
    }
    #endregion

    #region Preconditions
    protected void AddPrecondition(GoapEffect effect, Func<bool> condition) {
        preconditions.Add(new Precondition(effect, condition));
    }
    public bool CanSatisfyAllPreconditions() {
        for (int i = 0; i < preconditions.Count; i++) {
            if (!preconditions[i].CanSatisfyCondition()) {
                return false;
            }
        }
        return true;
    }
    protected bool HasNonPositiveDisablerTrait() {
        Character target = poiTarget as Character;
        return target.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER);
    }
    #endregion

    #region Effects
    protected void AddExpectedEffect(GoapEffect effect) {
        expectedEffects.Add(effect);
    }
    public bool WillEffectsSatisfyPrecondition(GoapEffect precondition) {
        for (int i = 0; i < expectedEffects.Count; i++) {
            if(EffectPreconditionMatching(expectedEffects[i], precondition)) {
                return true;
            }
        }
        return false;
    }
    private bool EffectPreconditionMatching(GoapEffect effect, GoapEffect precondition) {
        if(effect.targetPOI == precondition.targetPOI && effect.conditionType == precondition.conditionType) {
            if(effect.conditionKey != null && precondition.conditionKey != null) {
                switch (effect.conditionType) {
                    case GOAP_EFFECT_CONDITION.HAS_SUPPLY:
                    case GOAP_EFFECT_CONDITION.HAS_FOOD:
                        int effectInt = (int) effect.conditionKey;
                        int preconditionInt = (int) precondition.conditionKey;
                        return effectInt >= preconditionInt;
                    default:
                        return effect.conditionKey == precondition.conditionKey;
                        //case GOAP_EFFECT_CONDITION.HAS_ITEM:
                        //    string effectString = effect.conditionKey as string;
                        //    string preconditionString = precondition.conditionKey as string;
                        //    return effectString.ToLower() == preconditionString.ToLower();

                        //case GOAP_EFFECT_CONDITION.REMOVE_TRAIT:
                        //case GOAP_EFFECT_CONDITION.HAS_TRAIT:
                        //    effectString = effect.conditionKey as string;
                        //    preconditionString = precondition.conditionKey as string;
                        //    return effectString.ToLower() == preconditionString.ToLower();
                }
            } else {
                return true;
            }
        }
        return false;
    }
    public void AddActualEffect(GoapEffect effect) {
        actualEffects.Add(effect);
    }
    #endregion

    #region Tile Objects
    protected virtual void OnPerformActualActionToTarget() {
        if (GoapActionStateDB.GetStateResult(goapType, currentState.name) != InteractionManager.Goap_State_Success) {
            return;
        }
        if (poiTarget is TileObject) {
            TileObject target = poiTarget as TileObject;
            target.OnDoActionToObject(this);
        } else if (poiTarget is Character) {
            if (currentState.name != "Target Missing" && !doesNotStopTargetCharacter) {
                AddAwareCharacter(poiTarget as Character);
            }
        }
    }
    protected virtual void OnFinishActionTowardsTarget() {
        if (GoapActionStateDB.GetStateResult(goapType, currentState.name) != InteractionManager.Goap_State_Success) {
            return;
        }
        if (poiTarget is TileObject) {
            TileObject target = poiTarget as TileObject;
            target.OnDoneActionToObject(this);
        }
    }
    protected virtual void OnCancelActionTowardsTarget() {
        if (poiTarget is TileObject) {
            TileObject target = poiTarget as TileObject;
            target.OnCancelActionTowardsObject(this);
        }
    }
    private void OnTileObjectRemoved(TileObject tileObj, Character removedBy, LocationGridTile removedFrom) {
        if (poiTarget == tileObj) {
            if (isPerformingActualAction) {
                if (removedBy != null && removedBy == actor) {
                    return; //if the object was removed by the actor, do not stop the action
                }
                StopAction(); //when the target object of this action was removed, and the actor is currently performing the action, stop the action
            }
        }
    }
    private void OnTileObjectDisabled(TileObject tileObj, Character disabledBy) {
        if (poiTarget == tileObj) {
            if (isPerformingActualAction) {
                if (disabledBy != null && disabledBy == actor) {
                    return; //if the object was disabled by the actor, do not stop the action
                }
                StopAction(); //when the target object of this action was disabled, and the actor is currently performing the action, stop the action
            }
        }
    }
    #endregion

    #region Crime System
    public bool IsConsideredACrimeBy(Character reacting) {
        if (actor.faction == FactionManager.Instance.neutralFaction 
            || reacting.faction == FactionManager.Instance.neutralFaction) {
            return false;
        }
        if (parentPlan != null && parentPlan.job != null
                && reacting.homeArea != null && reacting.homeArea.jobQueue.jobsInQueue.Contains(parentPlan.job)) {
            return false;
        }
        return reacting.faction == actor.faction && committedCrime != CRIME.NONE;
    }
    protected void SetCommittedCrime(CRIME crime, Character[] crimeCommitters) {
        committedCrime = crime;
        this.crimeCommitters = crimeCommitters;
    }
    public void OnReportCrime() {
        hasCrimeBeenReported = true;
    }
    #endregion

    #region Special Note: Stealth
    protected bool HasOtherCharacterInRadius() {
        if (poiTarget is Character) {
            List<LocationGridTile> tiles = (poiTarget as Character).currentStructure.location.areaMap.GetTilesInRadius(poiTarget.gridTileLocation, 3, 0, true);
            for (int i = 0; i < tiles.Count; i++) {
                //if (tiles[i].occupant != null && tiles[i].occupant != actor && tiles[i].occupant != poiTarget) {
                //    return true;
                //}
                LocationGridTile currTile = tiles[i];
                List<Character> characters = new List<Character>(currTile.charactersHere);
                characters.Remove(actor);
                characters.Remove(poiTarget as Character);
                for (int j = 0; j < characters.Count; j++) {
                    Character currCharacter = characters[j];
                    //check each character, if a character has a disabler trait, do not consider them as a character that will notice this action
                    if (!currCharacter.HasTraitOf(TRAIT_TYPE.DISABLER)) {
                        return true;
                    }
                }
            }
        } else {
            List<LocationGridTile> tiles = poiTarget.gridTileLocation.structure.location.areaMap.GetTilesInRadius(poiTarget.gridTileLocation, 3, 0, true);
            for (int i = 0; i < tiles.Count; i++) {
                //if (tiles[i].occupant != null && tiles[i].occupant != actor && tiles[i].occupant != poiTarget) {
                //    return true;
                //}
                LocationGridTile currTile = tiles[i];
                List<Character> characters = new List<Character>(currTile.charactersHere);
                characters.Remove(actor);
                for (int j = 0; j < characters.Count; j++) {
                    Character currCharacter = characters[j];
                    //check each character, if a character has a disabler trait, do not consider them as a character that will notice this action
                    if (!currCharacter.HasTraitOf(TRAIT_TYPE.DISABLER)) {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    #endregion
}

public struct GoapEffect {
    public GOAP_EFFECT_CONDITION conditionType;
    public object conditionKey;
    public IPointOfInterest targetPOI; //this is the target that will be affected by the condition type and key

    public GoapEffect(GOAP_EFFECT_CONDITION conditionType, object conditionKey = null, IPointOfInterest targetPOI = null) {
        this.conditionType = conditionType;
        this.conditionKey = conditionKey;
        this.targetPOI = targetPOI;
    }

    public string conditionString() {
        if(conditionKey is string) {
            return conditionKey.ToString();
        } else if (conditionKey is int) {
            return conditionKey.ToString();
        } else if (conditionKey is Character) {
            return (conditionKey as Character).name;
        } else if (conditionKey is Area) {
            return (conditionKey as Area).name;
        } else if (conditionKey is Region) {
            return (conditionKey as Region).name;
        } else if (conditionKey is SpecialToken) {
            return (conditionKey as SpecialToken).name;
        } else if (conditionKey is IPointOfInterest) {
            return (conditionKey as IPointOfInterest).name;
        }
        return string.Empty;
    }
    public string conditionKeyToString() {
        if (conditionKey is string) {
            return (string)conditionKey;
        } else if (conditionKey is int) {
            return ((int)conditionKey).ToString();
        } else if (conditionKey is Character) {
            return (conditionKey as Character).id.ToString();
        } else if (conditionKey is Area) {
            return (conditionKey as Area).id.ToString();
        } else if (conditionKey is Region) {
            return (conditionKey as Region).id.ToString();
        } else if (conditionKey is SpecialToken) {
            return (conditionKey as SpecialToken).id.ToString();
        } else if (conditionKey is IPointOfInterest) {
            return (conditionKey as IPointOfInterest).id.ToString();
        }
        return string.Empty;
    }
    public string conditionKeyTypeString() {
        if (conditionKey is string) {
            return "string";
        } else if (conditionKey is int) {
            return "int";
        } else if (conditionKey is Character) {
            return "character";
        } else if (conditionKey is Area) {
            return "area";
        } else if (conditionKey is Region) {
            return "region";
        } else if (conditionKey is SpecialToken) {
            return "item";
        } else if (conditionKey is IPointOfInterest) {
            return "poi";
        }
        return string.Empty;
    }

    public override bool Equals(object obj) {
        if (obj is GoapEffect) {
            GoapEffect otherEffect = (GoapEffect)obj;
            if (otherEffect.conditionType == conditionType) {
                if (string.IsNullOrEmpty(conditionString())) {
                    return true;
                } else {
                    return otherEffect.conditionString() == conditionString();
                }
            }
        }
        return base.Equals(obj);
    }
    public override int GetHashCode() {
        return base.GetHashCode();
    }
}

[System.Serializable]
public class SaveDataGoapEffect {
    public GOAP_EFFECT_CONDITION conditionType;

    public string conditionKey;
    public string conditionKeyIdentifier;
    public POINT_OF_INTEREST_TYPE conditionKeyPOIType;
    public TILE_OBJECT_TYPE conditionKeyTileObjectType;


    public int targetPOIID;
    public POINT_OF_INTEREST_TYPE targetPOIType;
    public TILE_OBJECT_TYPE targetPOITileObjectType;

    public void Save(GoapEffect goapEffect) {
        conditionType = goapEffect.conditionType;

        if(goapEffect.conditionKey != null) {
            conditionKeyIdentifier = goapEffect.conditionKeyTypeString();
            conditionKey = goapEffect.conditionKeyToString();
            if(goapEffect.conditionKey is IPointOfInterest) {
                conditionKeyPOIType = (goapEffect.conditionKey as IPointOfInterest).poiType;
            }
            if (goapEffect.conditionKey is TileObject) {
                conditionKeyTileObjectType = (goapEffect.conditionKey as TileObject).tileObjectType;
            }
        } else {
            conditionKeyIdentifier = string.Empty;
        }

        if(goapEffect.targetPOI != null) {
            targetPOIID = goapEffect.targetPOI.id;
            targetPOIType = goapEffect.targetPOI.poiType;
            if(goapEffect.targetPOI is TileObject) {
                targetPOITileObjectType = (goapEffect.targetPOI as TileObject).tileObjectType;
            }
        } else {
            targetPOIID = -1;
        }
    }

    public GoapEffect Load() {
        GoapEffect effect = new GoapEffect() {
            conditionType = conditionType,
        };
        if(targetPOIID != -1) {
            GoapEffect tempEffect = effect;
            if (targetPOIType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                tempEffect.targetPOI = CharacterManager.Instance.GetCharacterByID(targetPOIID);
            } else if (targetPOIType == POINT_OF_INTEREST_TYPE.ITEM) {
                tempEffect.targetPOI = TokenManager.Instance.GetSpecialTokenByID(targetPOIID);
            } else if (targetPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                tempEffect.targetPOI = InteriorMapManager.Instance.GetTileObject(targetPOITileObjectType, targetPOIID);
            }
            effect = tempEffect;
        }
        if(conditionKeyIdentifier != string.Empty) {
            GoapEffect tempEffect = effect;
            if (conditionKeyIdentifier == "string") {
                tempEffect.conditionKey = conditionKey;
            } else if (conditionKey == "int") {
                tempEffect.conditionKey = int.Parse(conditionKey);
            } else if (conditionKey == "character") {
                tempEffect.conditionKey = CharacterManager.Instance.GetCharacterByID(int.Parse(conditionKey));
            } else if (conditionKey == "area") {
                tempEffect.conditionKey = LandmarkManager.Instance.GetAreaByID(int.Parse(conditionKey));
            } else if (conditionKey == "region") {
                tempEffect.conditionKey = GridMap.Instance.GetRegionByID(int.Parse(conditionKey));
            } else if (conditionKey == "item") {
                tempEffect.conditionKey = TokenManager.Instance.GetSpecialTokenByID(int.Parse(conditionKey));
            } else if (conditionKey == "poi") {
                if (conditionKeyPOIType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                    tempEffect.conditionKey = CharacterManager.Instance.GetCharacterByID(int.Parse(conditionKey));
                } else if (conditionKeyPOIType == POINT_OF_INTEREST_TYPE.ITEM) {
                    tempEffect.conditionKey = TokenManager.Instance.GetSpecialTokenByID(int.Parse(conditionKey));
                } else if (conditionKeyPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    tempEffect.conditionKey = InteriorMapManager.Instance.GetTileObject(conditionKeyTileObjectType, int.Parse(conditionKey));
                }
            }
            effect = tempEffect;
        }
        return effect;
    }
}

public class GoapActionData {
    public INTERACTION_TYPE goapType { get; protected set; }
    public RACE[] racesThatCanDoAction { get; protected set; }
    public Func<Character, IPointOfInterest, object[], bool> requirementAction { get; protected set; }
    public Func<Character, IPointOfInterest, object[], bool> requirementOnBuildGoapTreeAction { get; protected set; }

    public GoapActionData(INTERACTION_TYPE goapType) {
        this.goapType = goapType;
    }

    #region Virtuals
    public bool CanSatisfyTimeOfDays() {
        return true;
    }
    #endregion

    public bool CanSatisfyRequirements(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool requirementActionSatisfied = true;
        if (requirementAction != null) {
            requirementActionSatisfied = requirementAction.Invoke(actor, poiTarget, otherData);
        }
        if (requirementActionSatisfied) {
            if (goapType.IsDirectCombatAction()) { //Reference: https://trello.com/c/uxZxcOEo/2343-critical-characters-shouldnt-attempt-hostile-actions
                requirementActionSatisfied = actor.IsCombatReady();
            }
        }
        return requirementActionSatisfied;
    }
    public bool CanSatisfyRequirementOnBuildGoapTree(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool requirementActionSatisfied = true;
        if (requirementOnBuildGoapTreeAction != null) {
            requirementActionSatisfied = requirementOnBuildGoapTreeAction.Invoke(actor, poiTarget, otherData);
        }
        if (requirementActionSatisfied) {
            if (goapType.IsDirectCombatAction()) { //Reference: https://trello.com/c/uxZxcOEo/2343-critical-characters-shouldnt-attempt-hostile-actions
                requirementActionSatisfied = actor.IsCombatReady();
            }
        }
        return requirementActionSatisfied;
    }

    public bool DoesCharacterMatchRace(Character character) {
        if(racesThatCanDoAction != null) {
            return racesThatCanDoAction.Contains(character.race);
        }
        return false;
    }
}