using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GoapAction {
    public INTERACTION_TYPE goapType { get; private set; }
    public string goapName { get; private set; }
    public IPointOfInterest poiTarget { get; private set; }
    public Character actor { get; private set; }
    public int cost { get; private set; }

    protected Func<bool> _requirementAction;

    public GoapAction(INTERACTION_TYPE goapType, IPointOfInterest poiTarget, int cost = 0) {
        this.goapType = goapType;
        this.goapName = Utilities.NormalizeStringUpperCaseFirstLetters(goapType.ToString());
        this.poiTarget = poiTarget;
        this.cost = cost;
    }

    #region Utilities
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
}
