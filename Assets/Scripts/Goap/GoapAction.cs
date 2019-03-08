﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Reflection;

public class GoapAction {
    public INTERACTION_TYPE goapType { get; private set; }
    public INTERACTION_ALIGNMENT alignment { get; private set; }
    public string goapName { get; private set; }
    public IPointOfInterest poiTarget { get; private set; }
    public Character actor { get; private set; }
    public int cost { get { return GetCost(); } }
    public List<Precondition> preconditions { get; private set; }
    public List<GoapEffect> expectedEffects { get; private set; }
    public virtual LocationStructure targetStructure { get { return poiTarget.gridTileLocation.structure; } }
    public virtual LocationGridTile targetTile { get { return null; } }
    public Dictionary<string, GoapActionState> states { get; protected set; }
    public List<GoapEffect> actualEffects { get; private set; } //stores what really happened. NOTE: Only storing relevant data to share intel, no need to store everything that happened.
    public Log thoughtBubbleLog { get; private set; }
    public GoapActionState currentState { get; private set; }
    public GoapPlan parentPlan { get; private set; }

    protected Func<bool> _requirementAction;
    protected System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    protected string actionSummary;

    public GoapAction(INTERACTION_TYPE goapType, INTERACTION_ALIGNMENT alignment, Character actor, IPointOfInterest poiTarget) {
        this.alignment = alignment;
        this.goapType = goapType;
        this.goapName = Utilities.NormalizeStringUpperCaseFirstLetters(goapType.ToString());
        this.poiTarget = poiTarget;
        this.actor = actor;
        preconditions = new List<Precondition>();
        expectedEffects = new List<GoapEffect>();
        actualEffects = new List<GoapEffect>();
        actionSummary = GameManager.Instance.TodayLogString() + actor.name + " created " + goapType.ToString() + " action, targetting " + poiTarget?.ToString() ?? "Nothing";
        Initialize();
    }

    public void SetParentPlan(GoapPlan plan) {
        parentPlan = plan;
    }

    #region States
    public void SetState(string stateName) {
        currentState = states[stateName];
        AddActionLog(GameManager.Instance.TodayLogString() + " Set state to " + currentState.name);
        OnPerformActualActionToTarget();
        currentState.Execute();
        //TODO: Change this to accomodate duration changes
        actor.OnCharacterDoAction(this);
    }
    #endregion

    #region Virtuals
    protected virtual void CreateStates() {
        string summary = "Creating states for goap action (Dynamic) " + goapType.ToString() + " for actor " + actor.name;
        sw.Start();
        states = new Dictionary<string, GoapActionState>();
        if (GoapActionStateDB.goapActionStates.ContainsKey(this.goapType)) {
            StateNameAndDuration[] statesSetup = GoapActionStateDB.goapActionStates[this.goapType];
            for (int i = 0; i < statesSetup.Length; i++) {
                StateNameAndDuration state = statesSetup[i];
                string trimmedState = Utilities.RemoveWhitespace(state.name);
                Type thisType = this.GetType();
                MethodInfo preMethod = thisType.GetMethod("Pre" + trimmedState, BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo perMethod = thisType.GetMethod("PerTick" + trimmedState, BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo afterMethod = thisType.GetMethod("After" + trimmedState, BindingFlags.NonPublic | BindingFlags.Instance);
                Action preAction = null;
                Action perAction = null;
                Action afterAction = null;
                if (preMethod != null) {
                    preAction = (Action)Delegate.CreateDelegate(typeof(Action), this, preMethod, false);
                }
                if (perMethod != null) {
                    perAction = (Action)Delegate.CreateDelegate(typeof(Action), this, perMethod, false);
                }
                if (afterMethod != null) {
                    afterAction = (Action)Delegate.CreateDelegate(typeof(Action), this, afterMethod, false);
                }
                GoapActionState newState = new GoapActionState(state.name, this, preAction, perAction, afterAction, state.duration, state.status);
                states.Add(state.name, newState);
                summary += "\n Creating state " + state.name;
            }

        }
        sw.Stop();
        Debug.Log(summary + "\n" + string.Format("Total creation time is {0}ms", sw.ElapsedMilliseconds));

    }
    protected virtual void ConstructPreconditionsAndEffects() { }
    protected virtual void ConstructRequirement() { }
    protected virtual int GetCost() {
        return 0;
    }
    public virtual void PerformActualAction() { }
    public virtual bool IsHalted() {
        return false;
    }
    protected virtual void CreateThoughtBubbleLog() {
        thoughtBubbleLog = new Log(GameManager.Instance.Today(), "GoapAction", this.GetType().ToString(), "thought_bubble");
        if (thoughtBubbleLog != null) {
            thoughtBubbleLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            thoughtBubbleLog.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
            if (targetStructure != null) {
                thoughtBubbleLog.AddToFillers(targetStructure.location, targetStructure.location.name, LOG_IDENTIFIER.LANDMARK_1);
            } else {
                thoughtBubbleLog.AddToFillers(actor.specificLocation, actor.specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
            }
        }
    }
    public virtual void DoAction(GoapPlan plan) {
        CreateStates(); //Not sure if this is the best place for this.
        if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character targetCharacter = poiTarget as Character;
            targetCharacter.AdjustIsWaitingForInteraction(1);
        }
        ReserveTarget();
        if (actor.specificLocation != targetStructure.location) {
            actor.currentParty.GoToLocation(targetStructure.location, PATHFINDING_MODE.NORMAL, targetStructure, () => actor.PerformGoapAction(plan));
        } else if (actor.currentStructure != targetStructure) {
            actor.MoveToAnotherStructure(targetStructure, targetTile, null, () => actor.PerformGoapAction(plan));
            //actor.PerformGoapAction(plan);
        } else {
            actor.PerformGoapAction(plan);
        }
    }
    public void End() {
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_DEATH)) {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnActorDied);
        }
        Debug.Log(this.goapType.ToString() + " action by " + this.actor.name  + " Summary: \n" + actionSummary);
    }
    #endregion

    #region Utilities
    protected void Initialize() {
        ConstructRequirement();
        ConstructPreconditionsAndEffects();
        CreateThoughtBubbleLog();
    }
    public bool IsThisPartOfActorActionPool(Character actor) {
        List<INTERACTION_TYPE> actorInteractions = RaceManager.Instance.GetNPCInteractionsOfRace(actor);
        return actorInteractions.Contains(goapType);
    }
    public bool CanSatisfyRequirements() {
        if(_requirementAction != null) {
            return _requirementAction();
        }
        return true;
    }
    public void AddTraitTo(Character target, string traitName) {
        if (target.AddTrait(traitName)) {
            AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.ADD_TRAIT, conditionKey = traitName, targetPOI = target });
        }
    }
    public void RemoveTraitFrom(Character target, string traitName) {
        if (target.RemoveTrait(traitName)) {
            AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = traitName, targetPOI = target });
        }
    }
    public void ReturnToActorTheActionResult(string result) {
        End();
        actor.GoapActionResult(result, this);
    }
    protected void AddActionLog(string log) {
        actionSummary += "\n" + log;
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
    protected virtual void ReserveTarget() {
        if (poiTarget is TileObject) {
            TileObject target = poiTarget as TileObject;
            target.OnTargetObject(this);
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnActorDied);
        }
    }
    protected virtual void OnPerformActualActionToTarget() {
        if (poiTarget is TileObject) {
            TileObject target = poiTarget as TileObject;
            target.OnDoActionToObject(this);
        }
    }
    private void OnActorDied(Character character) {
        if (character.id == actor.id) {
            if (poiTarget is TileObject) {
                TileObject target = poiTarget as TileObject;
                target.owner.SetPOIState(POI_STATE.ACTIVE); //this is for when the character that reserved the target object died
            }
        }
    }
    #endregion
}

public struct GoapEffect {
    public GOAP_EFFECT_CONDITION conditionType;
    public object conditionKey;
    public IPointOfInterest targetPOI;

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