using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject {

    public IPointOfInterest owner { get; private set; }

    protected void Initialize(IPointOfInterest owner) {
        this.owner = owner;
    }

	public virtual void OnTargetObject(GoapAction action) {
        owner.SetPOIState(POI_STATE.RESERVED);
    }

    public virtual void OnDoActionToObject(GoapAction action) {
        //if (success) {
            owner.SetPOIState(POI_STATE.INACTIVE);
        //} else {
        //    owner.SetPOIState(POI_STATE.ACTIVE); //return the object to active state
        //}
    }

    public virtual void OnDoneActionTowardsTarget(GoapAction action) { //called when the action towrds this object has been done aka. setting this object to active again
        owner.SetPOIState(POI_STATE.ACTIVE);
    }
}
