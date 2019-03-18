using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject {

    public int id { get; private set; }
    public IPointOfInterest owner { get; private set; }
    public TILE_OBJECT_TYPE tileObjectType { get; private set; }

    protected void Initialize(IPointOfInterest owner, TILE_OBJECT_TYPE tileObjectType) {
        id = Utilities.SetID(this);
        this.owner = owner;
        this.tileObjectType = tileObjectType;
    }

	public virtual void OnTargetObject(GoapAction action) {
        
    }

    public virtual void OnDoActionToObject(GoapAction action) {
        owner.SetPOIState(POI_STATE.INACTIVE);
    }

    public virtual void OnDoneActionTowardsTarget(GoapAction action) { //called when the action towrds this object has been done aka. setting this object to active again
        owner.SetPOIState(POI_STATE.ACTIVE);
    }
}
