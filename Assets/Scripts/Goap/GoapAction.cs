using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GoapAction {
    public INTERACTION_TYPE goapType { get; private set; }
    public string goapName { get; private set; }
    public IPointOfInterest poiTarget { get; private set; }
    public Character actor { get; private set; }
    public int cost { get { return GetCost(); } }
    public List<Precondition> preconditions { get; private set; }
    public List<GoapEffect> effects { get; private set; }

    protected Func<bool> _requirementAction;

    public GoapAction(INTERACTION_TYPE goapType, Character actor, IPointOfInterest poiTarget) {
        this.goapType = goapType;
        this.goapName = Utilities.NormalizeStringUpperCaseFirstLetters(goapType.ToString());
        this.poiTarget = poiTarget;
        this.actor = actor;
        preconditions = new List<Precondition>();
        effects = new List<GoapEffect>();
        Initialize();
    }

    #region Virtuals
    protected virtual void ConstructPreconditionsAndEffects() { }
    protected virtual void ConstructRequirement() { }
    protected virtual int GetCost() {
        return 0;
    }
    public virtual void PerformAction() {

    }
    public virtual bool IsHalted() {
        return false;
    }
    #endregion

    #region Utilities
    protected void Initialize() {
        ConstructRequirement();
        ConstructPreconditionsAndEffects();
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
    #endregion

    #region Preconditions
    protected void AddPrecondition(GoapEffect effect, Func<bool> condition) {
        preconditions.Add(new Precondition(effect, condition));
    }
    #endregion

    #region Effects
    protected void AddEffect(GoapEffect effect) {
        effects.Add(effect);
    }
    public bool WillEffectsSatisfyPrecondition(GoapEffect precondition) {
        for (int i = 0; i < effects.Count; i++) {
            if(EffectPreconditionMatching(effects[i], precondition)) {
                return true;
            }
        }
        return false;
    }
    private bool EffectPreconditionMatching(GoapEffect effect, GoapEffect precondition) {
        if(effect.targetPOI == precondition.targetPOI) {
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
        }
        return false;
    }
    #endregion
}

public struct GoapEffect {
    public GOAP_EFFECT_CONDITION conditionType;
    public object conditionKey;
    public TARGET_POI targetPOI;
}