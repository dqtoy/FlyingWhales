using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanIntel : Intel {

    public Character actor { get; private set; }
    public GoapPlan plan { get; private set; }
	
    public PlanIntel(Character actor, GoapPlan plan) {
        this.actor = actor;
        this.plan = plan;
        SetIntelLog(plan.endNode.action.thoughtBubbleLog);
    }
}
