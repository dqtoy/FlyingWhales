using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventIntel : Intel {

	public Character actor { get; private set; }
    public IPointOfInterest target { get; private set; }
    public GoapAction action { get; private set; }
    public GoapPlan plan { get; private set; }

    public EventIntel(Character actor, GoapAction action) {
        this.actor = actor;
        target = action.poiTarget;
        this.action = action;
        plan = action.parentPlan;
        SetIntelLog(action.currentState.descriptionLog);
    }
}
