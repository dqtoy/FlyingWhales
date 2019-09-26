using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanIntel : Intel {

    public Character actor { get; private set; }
    public GoapPlan plan { get; private set; }
	
    public PlanIntel(Character actor, GoapPlan plan) : base(actor, plan) {
        this.actor = actor;
        this.plan = plan;
        SetIntelLog(plan.endNode.action.planLog);
    }

    public PlanIntel(SaveDataPlanIntel data) : base(data) {
        if (data.actorID != -1) {
            actor = CharacterManager.Instance.GetCharacterByID(data.actorID);
        }
    }
}

public class SaveDataPlanIntel : SaveDataIntel {
    public int actorID;

    public override void Save(Intel intel) {
        base.Save(intel);
        PlanIntel derivedIntel = intel as PlanIntel;
        if (derivedIntel.actor != null) {
            actorID = derivedIntel.actor.id;
        } else {
            actorID = -1;
        }
    }

    //public override Intel Load() {
    //    PlanIntel intel = base.Load() as PlanIntel;
    //    intel.Load(this);
    //    return intel;
    //}
}